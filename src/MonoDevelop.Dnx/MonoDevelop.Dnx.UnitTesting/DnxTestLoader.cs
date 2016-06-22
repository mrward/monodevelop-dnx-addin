//
// DnxTestLoader.cs
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
using System.Diagnostics;
using Microsoft.DotNet.ProjectModel.Server.Models;
using Microsoft.DotNet.Tools.Test;
using Microsoft.Extensions.Testing.Abstractions;
using MonoDevelop.Core;
using MonoDevelop.UnitTesting;

namespace MonoDevelop.Dnx.UnitTesting
{
	public class DnxTestLoader : IDisposable
	{
		DotNetCoreTestServer testServer;
		Process dotNetTestProcess;
		List<TestDiscovered> tests = new List<TestDiscovered> ();
		DnxNamespaceTestGroup parentNamespace;
		bool discoveryCompleted;

		public void Dispose ()
		{
			IsRunning = false;

			try {
				if (testServer != null) {
					testServer.Dispose ();
					testServer = null;
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Test loader dispose error.", ex);
			}

			try {
				if (dotNetTestProcess != null) {
					dotNetTestProcess.Exited -= DotNetTestProcessExited;
					if (!dotNetTestProcess.HasExited) {
						dotNetTestProcess.Kill ();
						dotNetTestProcess = null;
					}
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Test loader dispose error.", ex);
			}
		}

		public bool IsRunning { get; private set; }

		public void Start (string projectDirectory)
		{
			try {
				discoveryCompleted = false;

				tests.Clear ();
				testServer = new DotNetCoreTestServer (OnMessageReceived);
				testServer.Open ();

				RunDotNetCoreTest (projectDirectory);
				IsRunning = true;
			} catch (Exception ex) {
				OnError (ex);
			}
		}

		public void Stop ()
		{
			Dispose ();
		}

		public event EventHandler DiscoveryCompleted;

		void OnDiscoveryCompleted ()
		{
			DiscoveryCompleted?.Invoke (this, new EventArgs ());
		}

		void OnMessageReceived (Message m)
		{
			try {
				if (m.MessageType == TestMessageTypes.TestSessionConnected) {
					testServer.StartTestDiscovery ();
				} else if (m.MessageType == TestMessageTypes.TestDiscoveryTestFound) {
					var val = m.Payload.ToObject<TestDiscovered> ();
					tests.Add (val);
				} else if (m.MessageType == TestMessageTypes.TestDiscoveryCompleted) {
					discoveryCompleted = true;
					testServer.TerminateTestSession ();
					OnDiscoveryCompleted ();
					Stop ();
				} else if (m.MessageType == "Error") {
					var val = m.Payload.ToObject<Microsoft.DotNet.Tools.Test.ErrorMessage> ();
					LoggingService.LogError ("Test loader error: {0}", val.Message);
					OnDiscoveryFailed ();
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Test loader error", ex);
				OnDiscoveryFailed ();
			}
		}

		void RunDotNetCoreTest (string projectDirectory)
		{
			string arguments = String.Format (@"test --port {0} --parentProcessId {1} --no-build",
				testServer.Port,
				Process.GetCurrentProcess ().Id);

			var startInfo = new ProcessStartInfo {
				FileName = "dotnet",
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				Arguments = arguments,
				WorkingDirectory = projectDirectory,
			};

			dotNetTestProcess = Process.Start (startInfo);
			dotNetTestProcess.EnableRaisingEvents = true;
			dotNetTestProcess.Exited += DotNetTestProcessExited;
			if (dotNetTestProcess.HasExited) {
				OnDotNetTestProcessExitedBeforeCompleted ();
			}
		}

		public void BuildTestInfo (DnxProjectTestSuite projectTestSuite)
		{
			tests.Sort (OrderByName);

			parentNamespace = new DnxNamespaceTestGroup (projectTestSuite, null, String.Empty);
			parentNamespace.AddTests (tests);
		}

		static int OrderByName (Test x, Test y)
		{
			return StringComparer.Ordinal.Compare (x.FullyQualifiedName, y.FullyQualifiedName);
		}

		public IEnumerable<UnitTest> GetTests ()
		{
			return parentNamespace.Tests;
		}

		public event EventHandler DiscoveryFailed;

		void OnError (Exception ex)
		{
			LoggingService.LogError ("Test loader error", ex);
			OnDiscoveryFailed ();
		}

		void OnDiscoveryFailed ()
		{
			Stop ();
			DiscoveryFailed?.Invoke (this, new EventArgs ());
		}

		void DotNetTestProcessExited (object sender, EventArgs e)
		{
			if (!discoveryCompleted) {
				OnDotNetTestProcessExitedBeforeCompleted ();
			}
		}

		void OnDotNetTestProcessExitedBeforeCompleted ()
		{
			int? exitCode = dotNetTestProcess?.ExitCode;
			LoggingService.LogError ("dotnet test exited. {0}", exitCode);
			OnDiscoveryFailed ();
		}
	}
}

