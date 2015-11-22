# DNX Support for MonoDevelop and Xamarin Studio

Provides [DNX and ASP.NET 5](http://docs.asp.net/en/latest/dnx/index.html) support for MonoDevelop and Xamarin Studio 5.9 or above.

This addin uses source code from [OmniSharp](https://github.com/OmniSharp/omnisharp-roslyn) in order to communicate with the DNX host. It also uses source code from [Roslyn](https://github.com/dotnet/roslyn), [Microsoft.AspNet.Hosting](https://github.com/aspnet/Hosting), [Microsoft.Framework.Logging](https://github.com/aspnet/Logging) and [Microsoft.Framework.OptionsModel](https://github.com/aspnet/Options/), since the Roslyn version of OmniSharp uses types from their corresponding NuGet packages.

# Licenses

 - DNX addin - MIT
 - [Microsoft.AspNet.Hosting](https://github.com/aspnet/Hosting) - Apache 2.0
 - [Microsoft.Framework.Logging](https://github.com/aspnet/Logging) - Apache 2.0
 - [Microsoft.Framework.OptionsModel](https://github.com/aspnet/Options/) - Apache 2.0
 - [OmniSharp](https://github.com/OmniSharp/omnisharp-roslyn) - MIT
 - [Roslyn](https://github.com/dotnet/roslyn) - Apache 2.0

# Building from source

From the src directory run NuGet restore.

    cd src
    nuget restore MonoDevelop.Dnx.sln
    
Then build the MonoDevelop.Dnx.sln using xbuild, MSBuild, or Xamarin Studio.

To create the addin .mpack file run:

    mdtool.exe setup pack bin/merged/MonoDevelop.Dnx.dll
    
# Debugging

You can debug the DNX addin if you have the monodevelop source code git cloned into the same parent directory as the DNX addin. So if the DNX addin was cloned into:

    /Projects/monodevelop-dnx-addin

The clone monodevelop into:

    /Projects/monodevelop
    
Build MonoDevelop. Then you can debug the DNX addin by opening the solution into Xamarin Studio and selecting Start Debugging from the Run menu.

Alternatively you can change the custom command used to run the DNX addin by editing the MonoDevelop.Dnx.csproj file. The path to MonoDevelop.exe should be changed so it points to where MonoDevelop.exe or XamarinStudio.exe is on your machine:

        <Command type="Execute" command="../../../monodevelop/main/build/bin/MonoDevelop.exe" workingdir="../../../monodevelop/main/build/bin">
