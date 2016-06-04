//
// VSCodeDebuggerEngine.cs
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

using System.Linq;
using Mono.Debugging.Client;
using MonoDevelop.Core.Execution;
using MonoDevelop.Debugger;

namespace MonoDevelop.Dnx
{
	public class VSCodeDebuggerEngine : DebuggerEngineBackend
	{
		static DebuggerEngine engine;

		public static bool Exists ()
		{
			return DebuggerEngine != null;
		}

		static DebuggerEngine DebuggerEngine {
			get {
				if (engine == null) {
					engine = DebuggingService
						.GetDebuggerEngines ()
						.FirstOrDefault (e => e.Id == "VSCodeDebugger");
				}
				return engine;
			}
		}

		public override bool CanDebugCommand (ExecutionCommand cmd)
		{
			var dotNetCommand = ConvertCommand (cmd);
			var result = dotNetCommand != null &&
				DebuggerEngine != null &&
				DebuggerEngine.CanDebugCommand (dotNetCommand);
			return result;
		}

		public override bool IsDefaultDebugger (ExecutionCommand cmd)
		{
			return cmd is DotNetCoreExecutionCommand;
		}

		public override DebuggerStartInfo CreateDebuggerStartInfo (ExecutionCommand cmd)
		{
			var dotNetCommand = ConvertCommand (cmd);
			return DebuggerEngine.CreateDebuggerStartInfo (dotNetCommand);
		}

		public override DebuggerSession CreateSession ()
		{
			return DebuggerEngine.CreateSession ();
		}

		DotNetExecutionCommand ConvertCommand (ExecutionCommand cmd)
		{
			var dotNetCoreExecutionCommand = cmd as DotNetCoreExecutionCommand;
			if (dotNetCoreExecutionCommand != null) {
				dotNetCoreExecutionCommand.AllowCoreClrDebugging = true;
				return DotNetCoreExecutionHandler.ConvertCommand (dotNetCoreExecutionCommand);
			}

			return null;
		}
	}
}

