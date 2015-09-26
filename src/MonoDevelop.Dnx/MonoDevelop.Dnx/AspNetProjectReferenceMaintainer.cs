//
// AspNetProjectReferenceMaintainer.cs
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

using Microsoft.CodeAnalysis;
using OmniSharp.Dnx;

namespace MonoDevelop.Dnx
{
	public class AspNetProjectReferenceMaintainer
	{
		readonly DnxContext context;

		public AspNetProjectReferenceMaintainer(DnxContext context)
		{
			this.context = context;
		}

		public void UpdateReferences (ProjectId projectId, FrameworkProject frameworkProject)
		{
			DnxProject project = FindProject (projectId);
			if (project != null) {
				UpdateReferences (project, frameworkProject);
			}
		}

		DnxProject FindProject (ProjectId projectId)
		{
			var locator = new AspNetProjectLocator (context);
			return locator.FindProject (projectId);
		}

		void UpdateReferences (DnxProject project, FrameworkProject frameworkProject)
		{
			if (!project.IsCurrentFramework (frameworkProject.Framework, frameworkProject.Project.ProjectsByFramework.Keys))
				return;

			project.UpdateReferences (frameworkProject.FileReferences.Keys);
		}
	}
}

