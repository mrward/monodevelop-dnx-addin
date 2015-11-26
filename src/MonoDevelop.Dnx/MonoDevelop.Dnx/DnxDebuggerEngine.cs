using System;
using MonoDevelop.Debugger.Soft;
using MonoDevelop.Core.Execution;
using Mono.Debugging.Soft;
using System.Reflection;

namespace MonoDevelop.Dnx
{
	public class DnxDebuggerEngine : SoftDebuggerEngine
	{
		public override bool CanDebugCommand (MonoDevelop.Core.Execution.ExecutionCommand cmd)
		{
			return cmd is DnxProjectExecutionCommand;
		}

		public override Mono.Debugging.Client.DebuggerStartInfo CreateDebuggerStartInfo (MonoDevelop.Core.Execution.ExecutionCommand c)
		{
			var dotNetCommand = DnxExecutionHandler.ConvertCommand ((DnxProjectExecutionCommand)c);
			return base.CreateDebuggerStartInfo (dotNetCommand);
		}
	}
}

