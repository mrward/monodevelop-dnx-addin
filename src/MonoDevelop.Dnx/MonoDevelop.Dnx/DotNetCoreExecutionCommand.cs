//
// DotNetCoreExecutionCommand.cs
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
using System.IO;
using MonoDevelop.Core.Execution;

namespace MonoDevelop.Dnx
{
	public class DotNetCoreExecutionCommand : ExecutionCommand
	{
		string dotNetRuntimePath;

		public DotNetCoreExecutionCommand (string directory, string outputPath, string dotNetRuntimePath)
		{
			WorkingDirectory = directory;
			OutputPath = outputPath;
			this.dotNetRuntimePath = dotNetRuntimePath;

			Init ();
		}

		public string WorkingDirectory { get; private set; }
		public string OutputPath { get; private set; }
		public string Command { get; private set; }
		public string Arguments { get; private set; }
		public bool IsExecutable { get; private set; }

		void Init ()
		{
			string extension = GetOutputPathFileExtension ();
			if (extension == ".exe") {
				Command = OutputPath;
				IsExecutable = true;
			} else if (extension == ".dll") {
				Command = dotNetRuntimePath;
				Arguments = String.Format ("\"{0}\"", OutputPath);
			} else {
				Command = dotNetRuntimePath;
				Arguments = "run";
			}
		}

		string GetOutputPathFileExtension ()
		{
			if (!String.IsNullOrEmpty (OutputPath)) {
				return Path.GetExtension (OutputPath);
			}

			return null;
		}

		public string GetCommand ()
		{
			return Command;
		}

		public string GetArguments ()
		{
			return Arguments;
		}
	}
}

