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

using System.IO;
using System.Linq;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.InternalAbstractions;

namespace MonoDevelop.DotNet.ProjectModel
{
	public class DotNetProjectBuildOutputPathResolver
	{
		readonly string projectDirectory;

		public DotNetProjectBuildOutputPathResolver (string projectDirectory)
		{
			this.projectDirectory = projectDirectory;
		}

		public string ResolveExecutablePath (string configuration = "Debug")
		{
			var workspace = new BuildWorkspace (ProjectReaderSettings.ReadFromEnvironment ());
			var contextCollection =  workspace.GetProjectContextCollection (projectDirectory);
			if (contextCollection == null)
				return null;

			var frameworkContexts = contextCollection.FrameworkOnlyContexts.ToList ();

			var rids = RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers ().ToList ();

			foreach (ProjectContext frameworkContext in frameworkContexts.ToList ()) {
				var runtimeContext = workspace.GetRuntimeContext (frameworkContext, rids);

				OutputPaths paths = runtimeContext.GetOutputPaths (configuration);
				return paths.RuntimeFiles.Executable;
			}

			return null;
		}
	}
}

