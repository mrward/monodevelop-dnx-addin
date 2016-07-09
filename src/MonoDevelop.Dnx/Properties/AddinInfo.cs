using Mono.Addins;

[assembly:Addin ("Dnx",
	Namespace = "MonoDevelop",
	Version = "0.6",
	Category = "IDE extensions")]

[assembly:AddinName ("DNX")]
[assembly:AddinDescription ("Adds .NET Core and ASP.NET Core support.\n\nPlease uninstall any older version of the addin and restart the application before installing this new version.")]

[assembly:AddinDependency ("Core", "6.0")]
[assembly:AddinDependency ("Ide", "6.0")]
[assembly:AddinDependency ("DesignerSupport", "6.0")]
[assembly:AddinDependency ("Debugger", "6.0")]
[assembly:AddinDependency ("Debugger.Soft", "6.0")]
[assembly:AddinDependency ("SourceEditor2", "6.0")]
[assembly:AddinDependency ("UnitTesting", "6.0")]
