// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Code based on https://github.com/dotnet/cli/src/Microsoft.DotNet.ProjectModel.Workspaces/ProjectJsonWorkspace.cs

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.DotNet.ProjectModel;

namespace OmniSharp.Dnx
{
	public static class CompilerOptionsConverter
	{
		public static CSharpCompilationOptions GetCompilationOptions(CommonCompilerOptions compilerOptions)
		{
			var outputKind = compilerOptions.EmitEntryPoint.GetValueOrDefault() ?
				OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary;
			var options = new CSharpCompilationOptions(outputKind);

			string platformValue = compilerOptions.Platform;
			bool allowUnsafe = compilerOptions.AllowUnsafe ?? false;
			bool optimize = compilerOptions.Optimize ?? false;
			bool warningsAsErrors = compilerOptions.WarningsAsErrors ?? false;

			Microsoft.CodeAnalysis.Platform platform;
			if (!Enum.TryParse(value: platformValue, ignoreCase: true, result: out platform)) {
				platform = Microsoft.CodeAnalysis.Platform.AnyCpu;
			}

			options = options
				.WithAllowUnsafe(allowUnsafe)
				.WithPlatform(platform)
				.WithGeneralDiagnosticOption(warningsAsErrors ? ReportDiagnostic.Error : ReportDiagnostic.Default)
				.WithOptimizationLevel(optimize ? OptimizationLevel.Release : OptimizationLevel.Debug)
				.WithSpecificDiagnosticOptions(compilerOptions.SuppressWarnings.ToDictionary(
					suppress => suppress, _ => ReportDiagnostic.Suppress));

			return options;
		}

		public static LanguageVersion GetLanguageVersion (string languageVersionText)
		{
			LanguageVersion languageVersion;
			if (!Enum.TryParse<LanguageVersion>(value: languageVersionText,
				ignoreCase: true,
				result: out languageVersion))
			{
				languageVersion = LanguageVersion.CSharp6;
			}
			return languageVersion;
		}
	}
}

