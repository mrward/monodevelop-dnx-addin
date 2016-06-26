//
// RestoreProgressMonitor.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
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
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.Dnx
{
	class RestoreProgressMonitor
	{
		ProgressMonitor currentProgressMonitor;
		int concurrentRestoreCount;
		bool success;

		public void OnRestoreStarted ()
		{
			if (currentProgressMonitor == null) {
				concurrentRestoreCount = 0;
				success = true;
				currentProgressMonitor = CreateProgressMonitor ();
			}

			concurrentRestoreCount++;
		}

		public void OnRestoreFinished (bool success)
		{
			concurrentRestoreCount--;
			if (!success)
				this.success = false;

			if (concurrentRestoreCount <= 0) {
				if (this.success) {
					currentProgressMonitor.ReportSuccess (GettextCatalog.GetString ("Dependencies restored successfully."));
				} else {
					currentProgressMonitor.ReportError (GettextCatalog.GetString ("Could not restore dependencies."));
				}
				currentProgressMonitor.Dispose ();
				currentProgressMonitor = null;
			}
		}

		static ProgressMonitor CreateProgressMonitor ()
		{
			Pad pad = IdeApp.Workbench.GetPad<DnxOutputPad> ();
			return IdeApp.Workbench.ProgressMonitors.GetStatusProgressMonitor (
				GettextCatalog.GetString ("Restoring dependencies..."),
				Stock.StatusSolutionOperation,
				false,
				false,
				false,
				pad);
		}
	}
}

