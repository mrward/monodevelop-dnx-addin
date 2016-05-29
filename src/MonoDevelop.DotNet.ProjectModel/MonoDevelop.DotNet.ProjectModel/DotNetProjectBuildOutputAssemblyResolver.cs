//
// DotNetProjectBuildOutputPathResolver.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
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
using System.IO;
using System.Linq;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.InternalAbstractions;

namespace MonoDevelop.DotNet.ProjectModel
{
	public class DotNetProjectBuildOutputAssemblyResolver
	{
		readonly string projectDirectory;

		public DotNetProjectBuildOutputAssemblyResolver (string projectDirectory)
		{
			this.projectDirectory = projectDirectory;
		}

		public string ResolveOutputPath (string configuration = "Debug")
		{
			var workspace = new BuildWorkspace (ProjectReaderSettings.ReadFromEnvironment ());
			var contextCollection =  workspace.GetProjectContextCollection (projectDirectory);
			if (contextCollection == null)
				return null;

			var frameworkContexts = contextCollection.FrameworkOnlyContexts.ToList ();

			var runtimeIds = RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers ().ToList ();

			string outputPath = ResolveOutputPath (frameworkContexts, workspace, configuration, runtimeIds);
			if (outputPath == null) {
				// Try alternative architecture.
				runtimeIds = runtimeIds.Select (runtimeId => ConvertArchitecture (runtimeId)).ToList ();
				return ResolveOutputPath (frameworkContexts, workspace, configuration, runtimeIds);
			}

			return outputPath;
		}

		string ResolveOutputPath (
			IEnumerable<ProjectContext> frameworkContexts,
			BuildWorkspace workspace,
			string configuration,
			IEnumerable<string> rids)
		{
			foreach (ProjectContext frameworkContext in frameworkContexts) {
				ProjectContext runtimeContext = workspace.GetRuntimeContext (frameworkContext, rids);

				OutputPaths paths = runtimeContext.GetOutputPaths (configuration);
				if (File.Exists (paths.RuntimeFiles.Executable)) {
					return paths.RuntimeFiles.Executable;
				}
			}

			return null;
		}

		static string ConvertArchitecture (string runtimeId)
		{
			string currentArchitecture = RuntimeEnvironment.RuntimeArchitecture;
			string otherArchitecture = GetAlternativeArch ();
			return runtimeId.Replace (currentArchitecture, otherArchitecture);
		}

		/// <summary>
		/// Returns the alternative architecture to the current architecture.
		/// So if the current architecture is x86 then this method returns x64 and
		/// vice versa.
		/// </summary>
		static string GetAlternativeArch ()
		{
			return Environment.Is64BitProcess ? "x86" : "x64";
		}
	}
}

