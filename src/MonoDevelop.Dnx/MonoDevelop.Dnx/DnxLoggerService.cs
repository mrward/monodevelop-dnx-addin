//
// DnxLoggerService.cs
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
using Microsoft.Framework.Logging;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;

namespace MonoDevelop.Dnx
{
	public static class DnxLoggerService
	{
		const string DnxOutputLogLevelProperty = "MonoDevelop.Dnx.DnxOutputLogLevel";
		static LogLevel logLevel;

		static DnxLoggerService ()
		{
			string logLevelText = PropertyService.Get (DnxOutputLogLevelProperty, LogLevel.Warning.ToString ());
			if (!Enum.TryParse (logLevelText, out logLevel))
				logLevel = LogLevel.Warning;

			IdeApp.Workspace.SolutionUnloaded += SolutionUnloaded;
		}

		public static LogLevel LogLevel {
			get { return logLevel; }
			set {
				logLevel = value;
				PropertyService.Set (DnxOutputLogLevelProperty, logLevel.ToString ());
			}
		}

		public static void Log (LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
		{
			if (!IsEnabled (logLevel))
				return;

			LogView logView = DnxOutputPad.LogView;
			string message = formatter.Invoke (state, exception) + Environment.NewLine;
			switch (logLevel) {
				case LogLevel.Verbose:
					logView.WriteConsoleLogText (message);
					break;
				case LogLevel.Information:
				case LogLevel.Warning:
					logView.WriteText (message);
					break;
				case LogLevel.Critical:
				case LogLevel.Error:
					logView.WriteError (message);
					break;
			}
		}

		static public bool IsEnabled (LogLevel logLevel)
		{
			return logLevel >= LogLevel;
		}

		static void SolutionUnloaded (object sender, SolutionEventArgs e)
		{
			DnxOutputPad.LogView.Clear ();
		}
	}
}

