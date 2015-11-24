using Mono.Addins;

[assembly:Addin ("Dnx",
	Namespace = "MonoDevelop",
	Version = "0.1",
	Category = "IDE extensions")]

[assembly:AddinName ("DNX")]
[assembly:AddinDescription ("Adds DNX and ASP.NET 5 support.")]

[assembly:AddinDependency ("Core", "6.0")]
[assembly:AddinDependency ("Ide", "6.0")]
[assembly:AddinDependency ("DesignerSupport", "6.0")]
