//
// XProject.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://xamarin.com)
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

using System;
using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;

namespace MonoDevelop.Dnx
{
	public class XProject : DotNetProject
	{
		public XProject ()
		{
			UseMSBuildEngine = false;
			Initialize (this);
		}

		protected override void OnInitialize ()
		{
			base.OnInitialize ();
			SupportsRoslyn = true;
			StockIcon = "md-csharp-project";
		}

		protected override DotNetCompilerParameters OnCreateCompilationParameters (DotNetProjectConfiguration config, ConfigurationKind kind)
		{
			return new DnxConfigurationParameters ();
		}

		protected override ClrVersion[] OnGetSupportedClrVersions ()
		{
			return new ClrVersion[] {
				ClrVersion.Net_4_5
			};
		}

		public void SetDefaultNamespace (FilePath fileName)
		{
			DefaultNamespace = GetDefaultNamespace (fileName);
		}

		protected override bool OnSupportsFramework (TargetFramework framework)
		{
			if (IsSupportedClientProfile (framework)) {
				return true;
			}
			return base.OnSupportsFramework (framework);
		}

		/// <summary>
		/// If .NET Core MSBuild targets are installed on the machine then
		/// they set the TargetFrameworkProfile to be Client and the
		/// TargetFramework class indicates the ClrVersion is 4.0 even though the
		/// framework version 4.5. .NET 4.0 is not supported by .NET Core which
		/// the XProject indicates in the OnGetSupportedClrVersions method.
		/// This causes the project to fail to load. So here an explicit 
		/// check for a Client profile and if it is .NET 4.5 or higher the
		/// XProject indicates that it is supported.
		/// </summary>
		/// <returns>The supported client profile.</returns>
		/// <param name="framework">Framework.</param>
		bool IsSupportedClientProfile (TargetFramework framework)
		{
			if (framework.Id.Profile == "Client" && framework.Id.Identifier == ".NETFramework") {
				Version version = null;
				if (System.Version.TryParse (framework.Id.Version, out version)) {
					return version.Major >= 4 && version.Minor >= 5;
				}
			}

			return false;
		}
	}
}

