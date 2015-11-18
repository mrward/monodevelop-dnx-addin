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
			DnxOutputPad pad = DnxOutputPad.Instance;
			if (pad != null)
				pad.Log (logLevel, eventId, state, exception, formatter);
		}
	}
}

