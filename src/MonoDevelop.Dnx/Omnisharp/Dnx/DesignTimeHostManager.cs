using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Framework.Logging;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Dnx;

namespace OmniSharp.Dnx
{
    public class DesignTimeHostManager
    {
        private readonly ILogger _logger;
        private readonly DotNetCorePaths _paths;
        private readonly object _processLock = new object();
        ProcessAsyncOperation _designTimeHostOperation;
        private bool _stopped;

        public DesignTimeHostManager(ILoggerFactory loggerFactory, DotNetCorePaths paths)
        {
            _logger = loggerFactory.CreateLogger<DesignTimeHostManager>();
            _paths = paths;
        }

        public TimeSpan DelayBeforeRestart { get; set; }

        public void Start(string hostId, Action<int> onConnected)
        {
            lock (_processLock)
            {
                if (_stopped)
                {
                    return;
                }

                int port = GetFreePort();

                string arguments = string.Format(@"projectmodel-server --port {0} --host-pid {1} --host-name {2}",
                    port,
                    Process.GetCurrentProcess().Id,
                    hostId);

                _logger.LogVerbose(_paths.DotNet + " " + arguments);

                _designTimeHostOperation = Runtime.ProcessService.StartConsoleProcess( 
                    _paths.DotNet,
                    arguments,
                    null,
                    new DotNetCoreOutputOperationConsole (),
                    null,
                    (sender, e) => {
                        _logger.LogWarning("Design time host process ended");

                        Start(hostId, onConnected);
                    }
                );

                // Wait a little bit for it to conncet before firing the callback
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    var t1 = DateTime.UtcNow;
                    var dthTimeout = TimeSpan.FromSeconds(10);
                    while (!socket.Connected && DateTime.UtcNow - t1 < dthTimeout)
                    {
                        Thread.Sleep(500);
                        try
                        {
                            socket.Connect(new IPEndPoint(IPAddress.Loopback, port));
                        }
                        catch (SocketException)
                        {
                            // this happens when the DTH isn't listening yet
                        }
                    }

                    if (!socket.Connected)
                    {
                        // reached timeout
                        _logger.LogError("Failed to launch DesignTimeHost in a timely fashion.");
                        return;
                    }
                }

                if (_designTimeHostOperation.IsCompleted)
                {
                    // REVIEW: Should we quit here or retry?
                    _logger.LogError(string.Format("Failed to launch DesignTimeHost. Process exited with code {0}.", _designTimeHostOperation.ExitCode));
                    return;
                }

                _logger.LogInformation(string.Format("Running DesignTimeHost on port {0}, with PID {1}", port, _designTimeHostOperation.ProcessId));

                onConnected(port);
            }
        }

        public void Stop()
        {
            lock (_processLock)
            {
                if (_stopped)
                {
                    return;
                }

                _stopped = true;

                if (_designTimeHostOperation != null)
                {
                    _logger.LogInformation("Shutting down DesignTimeHost");

                    _designTimeHostOperation.Cancel();
                    _designTimeHostOperation = null;
                }
            }
        }

        private static int GetFreePort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
