//
// DnxProjectExecutionCommand.cs
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

using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;

namespace MonoDevelop.Dnx
{
	public class DnxProjectExecutionCommand : ExecutionCommand
	{
		public DnxProjectExecutionCommand (string directory, DnxRuntime runtime)
		{
			WorkingDirectory = directory;
			DnxRuntime = runtime;
		}

		public DnxRuntime DnxRuntime { get; private set; }
		public string WorkingDirectory { get; private set; }

		public DnxExecutionTarget DnxExecutionTarget {
			get { return base.Target as DnxExecutionTarget; }
		}

		public string GetCommand ()
		{
			return GetDnxRuntimePath ().Combine ("bin", GetDnxFileName ());
		}

		string GetDnxFileName ()
		{
			if (Platform.IsWindows) {
				return "dnx.exe";
			}
			return "Microsoft.Dnx.Host.Mono.dll";
		}

		public string GetArguments ()
		{
			if (DnxRuntime.UsesCurrentDirectoryByDefault) {
				return GetDnxCommand ();
			} else {
				return string.Format (". {0}", GetDnxCommand ());
			}
		}

		public FilePath GetDnxRuntimePath ()
		{
			if (DnxExecutionTarget == null || DnxExecutionTarget.Framework == null)
				return DnxRuntime.Path;

			return DnxRuntime.GetRuntimePath (DnxExecutionTarget.Framework);
		}

		string GetDnxCommand ()
		{
			if (DnxExecutionTarget == null)
				return null;

			return DnxExecutionTarget.Command;
		}
	}
}

