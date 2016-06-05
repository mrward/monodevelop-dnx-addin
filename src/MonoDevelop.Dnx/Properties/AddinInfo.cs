using Mono.Addins;

[assembly:Addin ("Dnx",
	Namespace = "MonoDevelop",
	Version = "0.3",
	Category = "IDE extensions")]

[assembly:AddinName ("DNX")]
[assembly:AddinDescription ("Adds .NET Core and ASP.NET Core support.")]

[assembly:AddinDependency ("Core", "6.0")]
[assembly:AddinDependency ("Ide", "6.0")]
[assembly:AddinDependency ("DesignerSupport", "6.0")]
[assembly:AddinDependency ("Debugger", "6.0")]
[assembly:AddinDependency ("Debugger.Soft", "6.0")]
[assembly:AddinDependency ("SourceEditor2", "6.0")]
