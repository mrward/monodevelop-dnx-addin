
using Mono.Debugging.Client;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Debugger.Soft;

namespace MonoDevelop.Dnx
{
	public class DotNetCoreDebuggerEngine : SoftDebuggerEngine
	{
		public override bool CanDebugCommand (ExecutionCommand cmd)
		{
			return cmd is DotNetCoreExecutionCommand && !Platform.IsWindows;
		}

		public override DebuggerStartInfo CreateDebuggerStartInfo (ExecutionCommand c)
		{
			var dotNetCommand = DotNetCoreExecutionHandler.ConvertCommand ((DotNetCoreExecutionCommand)c);
			return base.CreateDebuggerStartInfo (dotNetCommand);
		}
	}
}

