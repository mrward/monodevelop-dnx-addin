//
// DotNetCoreTestServer.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.DotNet.ProjectModel.Server.Models;
using Microsoft.DotNet.Tools.Test;
using Microsoft.Extensions.Testing.Abstractions;
using MonoDevelop.Core;
using MonoDevelop.Dnx.Omnisharp;
using Newtonsoft.Json.Linq;
using OmniSharp;

namespace MonoDevelop.Dnx.UnitTesting
{
	public class DotNetCoreTestServer : IDisposable
	{
		Action<Message> messageReceived;
		Socket listeningSocket;
		ProcessingQueue queue;
		bool disposed;

		public DotNetCoreTestServer (Action<Message> messageReceived)
		{
			this.messageReceived = messageReceived;
		}

		public void Open ()
		{
			Port = GetFreePort ();
			StartListening (Port);
		}

		public int Port { get; private set; }

		static int GetFreePort()
		{
			var l = new TcpListener (IPAddress.Loopback, 0);
			l.Start ();
			int port = ((IPEndPoint)l.LocalEndpoint).Port;
			l.Stop ();
			return port;
		}


		void StartListening (int port)
		{
			listeningSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listeningSocket.Bind (new IPEndPoint (IPAddress.Loopback, port));
			listeningSocket.Listen (4);

			try
			{
				listeningSocket.BeginAccept (OnClientConnected, null);
			}
			catch (Exception ex)
			{
				LoggingService.LogError ("DotNetCoreTestServer error.", ex);
			}
		}

		void OnClientConnected (IAsyncResult result)
		{
			try
			{
				Socket socket = listeningSocket.EndAccept(result);
				OnSocketConnected (socket);

				listeningSocket.BeginAccept (OnClientConnected, null);
			}
			catch (ObjectDisposedException ex)
			{
				if (!disposed) {
					LoggingService.LogError ("DotNetCoreTestServer error.", ex);
				}
			}
			catch (Exception ex)
			{
				LoggingService.LogError ("DotNetCoreTestServer error.", ex);
			}
		}

		void OnSocketConnected (Socket socket)
		{
			var networkStream = new NetworkStream (socket);

			queue = new ProcessingQueue (networkStream, new Logger ());

			queue.OnReceive += messageReceived;

			queue.Start ();
		}

		public void Dispose ()
		{
			disposed = true;
			if (queue != null) {
				queue.Stop ();
			}
			if (listeningSocket != null) {
				listeningSocket.Close ();
			}
		}

		public void GetTestRunnerStartInfo (IEnumerable<string> tests)
		{
			var runTestsMessage = new RunTestsMessage ();
			if (tests != null && tests.Any ())
				runTestsMessage.Tests = tests.ToList ();

			var message = new Message {
				MessageType = TestMessageTypes.TestExecutionGetTestRunnerProcessStartInfo,
				Payload = JToken.FromObject (runTestsMessage)
			};
			queue.Post (message);
		}

		public void TerminateTestSession ()
		{
			var message = new Message {
				MessageType = TestMessageTypes.TestSessionTerminate
			};
			queue.Post (message);
		}

		public void StartTestDiscovery ()
		{
			var message = new Message {
				MessageType = TestMessageTypes.TestDiscoveryStart
			};
			queue.Post (message);
		}
	}
}

