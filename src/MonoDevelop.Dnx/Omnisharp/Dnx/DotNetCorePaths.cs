
using OmniSharp;
using System.IO;

namespace OmniSharp.Dnx
{
	public class DotNetCorePaths
	{
		public DotNetCorePaths ()
		{
			DotNet = GetDotNetFileName ();
		}

		public string DotNet { get; private set; }

		private static string GetDotNetFileName ()
		{
			if (PlatformHelper.IsMono) {
				const string dotnetFileName = "/usr/local/share/dotnet/dotnet";
				if (File.Exists (dotnetFileName)) {
					return dotnetFileName;
				}
			}

			return "dotnet";
		}
	}
}

