//
// DnxProjectBuilder.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2015 Matthew Ward
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
using System.Threading;
using Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.Dnx
{
	public class DnxProjectBuilder : IDisposable
	{
		DnxProject project;
		IProgressMonitor monitor;
		ManualResetEventSlim waitEvent = new ManualResetEventSlim ();
		bool cancelled;
		DiagnosticsMessage[] messages;

		public DnxProjectBuilder (DnxProject project, IProgressMonitor monitor)
		{
			this.project = project;
			this.monitor = monitor;
			this.monitor.CancelRequested += CancelRequested;
		}

		public string ProjectPath {
			get { return project.JsonPath; }
		}

		void CancelRequested (IProgressMonitor monitor)
		{
			cancelled = true;
			waitEvent.Set ();
		}

		public void Dispose ()
		{
			IProgressMonitor currentMonitor = monitor;
			if (currentMonitor != null) {
				currentMonitor.CancelRequested -= CancelRequested;
				monitor = null;
			}
		}

		public BuildResult Build ()
		{
			DnxServices.ProjectService.GetDiagnostics (this);

			waitEvent.Wait ();

			if (cancelled || messages == null) {
				return new BuildResult ();
			}
			return CreateBuildResult ();
		}

		public void OnDiagnostics (DiagnosticsMessage[] messages)
		{
			this.messages = messages;
			waitEvent.Set ();
		}

		BuildResult CreateBuildResult ()
		{
			foreach (DiagnosticsMessage message in messages) {
				if (project.CurrentFramework == message.Framework.FrameworkName) {
					return message.ToBuildResult ();
				}
			}
			return new BuildResult ();
		}
	}
}

