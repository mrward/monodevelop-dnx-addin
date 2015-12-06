//
// DnxExecutionHandler.cs
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

using System.Collections.Generic;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core;

namespace MonoDevelop.Dnx
{
	public class DnxExecutionHandler : IExecutionHandler
	{
		public bool CanExecute (ExecutionCommand command)
		{
			return command is DnxProjectExecutionCommand;
		}

		public static DotNetExecutionCommand ConvertCommand (DnxProjectExecutionCommand dnxCommand)
		{
			return new DotNetExecutionCommand (
				dnxCommand.GetCommand (),
				dnxCommand.GetArguments (),
				dnxCommand.WorkingDirectory,
				new Dictionary<string, string> {
					{ "DNX_BUILD_PORTABLE_PDB", GetDnxBuildPortablePDBValue () }
				}
			) {
				UserAssemblyPaths = new List<string> ()
			};
		}

		static string GetDnxBuildPortablePDBValue ()
		{
			if (DnxServices.IsPortablePdbSupported)
				return "1";

			return "0";
		}

		public IProcessAsyncOperation Execute (ExecutionCommand command, IConsole console)
		{
			var dotNetCommand = ConvertCommand ((DnxProjectExecutionCommand)command);
			return Runtime.SystemAssemblyService.DefaultRuntime.GetExecutionHandler ().Execute (dotNetCommand, console);
		}
	}
}

