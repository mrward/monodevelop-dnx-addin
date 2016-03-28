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
		CSharpParseOptions parseOptions;
		CSharpCompilationOptions compilationOptions;

		public DnxConfigurationParameters ()
		{
		}

		public DnxConfigurationParameters (CSharpCompilationOptions compilationOptions, CSharpParseOptions parseOptions)
			: this ()
		{
			this.compilationOptions = compilationOptions;
			this.parseOptions = parseOptions;
		}

		public override bool NoStdLib {
			get { return true; }
			set { }
		}

		public override IEnumerable<string> GetDefineSymbols ()
		{
			if (parseOptions != null)
				return parseOptions.PreprocessorSymbolNames;

			return Enumerable.Empty<string> ();
		}

		public override CompilationOptions CreateCompilationOptions ()
		{
			if (compilationOptions != null)
				return compilationOptions;

			return new CSharpCompilationOptions (
				OutputKind.ConsoleApplication,
				false,
				null,
				null,
				"Script",
				null,
				OptimizationLevel.Debug,
				true,
				false,
				null,
				null,
				ImmutableArray<byte>.Empty,
				null,
				Platform.AnyCpu,
				ReportDiagnostic.Default,
				4,
				null,
				false,
				assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default
			);
		}

		public override ParseOptions CreateParseOptions (DotNetProjectConfiguration configuration)
		{
			if (parseOptions != null)
				return parseOptions;

			return new CSharpParseOptions (
				LanguageVersion.CSharp6,
				DocumentationMode.Parse,
				SourceCodeKind.Regular,
				ImmutableArray<string>.Empty.AddRange (GetDefineSymbols ())
			);
		}
	}
}

