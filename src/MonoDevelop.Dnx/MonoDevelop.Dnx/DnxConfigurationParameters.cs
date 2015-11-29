//
// DnxConfigurationParameters.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2015 Matthew Ward
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MonoDevelop.Projects;

namespace MonoDevelop.Dnx
{
	public class DnxConfigurationParameters : DotNetCompilerParameters
	{
		List<string> defineSymbols = new List<string> ();

		public DnxConfigurationParameters ()
		{
			NoStdLib = true;
		}

		public override bool NoStdLib { get; set; }

		public void UpdatePreprocessorSymbols (IEnumerable<string> symbols)
		{
			defineSymbols = symbols.ToList ();
		}

		public override IEnumerable<string> GetDefineSymbols ()
		{
			return defineSymbols;
		}

		public override CompilationOptions CreateCompilationOptions ()
		{
			var xproject = (XProject)ParentProject;
			var dnxProject = xproject.AsFlavor<DnxProject> ();

			return new CSharpCompilationOptions (
				OutputKind.ConsoleApplication,
				null,
				null, //project.MainClass,
				"Script",
				null,
				OptimizationLevel.Debug,
				true, //GenerateOverflowChecks,
				false, //UnsafeCode,
				null,
				null,
				ImmutableArray<byte>.Empty,
				null,
				Platform.AnyCpu,
				ReportDiagnostic.Default,
				4, //WarningLevel,
				null,
				false,
				assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default
			);
		}

		public override ParseOptions CreateParseOptions ()
		{
			return new CSharpParseOptions (
				LanguageVersion.CSharp6,
				DocumentationMode.Parse,
				SourceCodeKind.Regular,
				ImmutableArray<string>.Empty.AddRange (GetDefineSymbols ())
			);
		}
	}
}

