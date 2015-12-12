using Mono.Addins;

[assembly:Addin ("Dnx",
	Namespace = "MonoDevelop",
	Version = "0.1",
	Category = "IDE extensions")]

[assembly:AddinName ("DNX")]
[assembly:AddinDescription ("Adds DNX and ASP.NET 5 support.")]

[assembly:AddinDependency ("Core", "5.9")]
[assembly:AddinDependency ("Ide", "5.9")]
[assembly:AddinDependency ("DesignerSupport", "5.9")]
[assembly:AddinDependency ("Debugger", "5.9")]
[assembly:AddinDependency ("Debugger.Soft", "5.9")]
[assembly:AddinDependency ("SourceEditor2", "5.9")]