//
// DotNetCoreExecutionHandler.cs
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

using System.Collections.Generic;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core;

namespace MonoDevelop.Dnx
{
	public class DotNetCoreExecutionHandler : IExecutionHandler
	{
		public bool CanExecute (ExecutionCommand command)
		{
			return command is DotNetCoreExecutionCommand;
		}

		public static DotNetExecutionCommand ConvertCommand (DotNetCoreExecutionCommand command)
		{
			command.Initialize ();

			return new DotNetExecutionCommand (
				command.GetCommand (),
				command.GetArguments (),
				command.WorkingDirectory
			) {
				UserAssemblyPaths = new List<string> ()
			};
		}

		public ProcessAsyncOperation Execute (ExecutionCommand command, OperationConsole console)
		{
			var dotNetCommand = ConvertCommand ((DotNetCoreExecutionCommand)command);
			return Runtime.SystemAssemblyService.DefaultRuntime.GetExecutionHandler ().Execute (dotNetCommand, console);
		}
	}
}

