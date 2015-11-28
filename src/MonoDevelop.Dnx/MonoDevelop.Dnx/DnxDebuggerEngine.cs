
using Mono.Debugging.Client;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Debugger.Soft;

namespace MonoDevelop.Dnx
{
	public class DnxDebuggerEngine : SoftDebuggerEngine
	{
		public override bool CanDebugCommand (ExecutionCommand cmd)
		{
			return cmd is DnxProjectExecutionCommand && !Platform.IsWindows;
		}

		public override DebuggerStartInfo CreateDebuggerStartInfo (ExecutionCommand c)
		{
			var dotNetCommand = DnxExecutionHandler.ConvertCommand ((DnxProjectExecutionCommand)c);
			return base.CreateDebuggerStartInfo (dotNetCommand);
		}
	}
}

