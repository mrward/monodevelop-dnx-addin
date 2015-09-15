//
// DnxProjectTemplateWizard.cs
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

using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Templates;
using MonoDevelop.Projects;
using MonoDevelop.Core;

namespace MonoDevelop.Dnx
{
	public class DnxProjectTemplateWizard : TemplateWizard
	{
		public override WizardPage GetPage (int pageNumber)
		{
			return null;
		}

		public override int TotalPages {
			get { return 0; }
		}

		public override string Id {
			get { return "MonoDevelop.Dnx.ProjectTemplateWizard"; }
		}

		public override void ItemsCreated (IEnumerable<IWorkspaceFileObject> items)
		{
			Solution solution = GetSolution (items);
			if (solution == null)
				return;

			AddSolutionFoldersToSolution (solution);
		}

		static Solution GetSolution (IEnumerable<IWorkspaceFileObject> items)
		{
			if (items.Count () == 1) {
				return items.OfType<Solution> ().FirstOrDefault ();
			}
			return null;
		}

		DnxProject CreateProject (Solution solution, string projectName)
		{
			var project = new DnxProject ();
			project.IsDirty = true;
			project.GenerateNewProjectFileName (solution, projectName);
			project.AddConfigurations ();

			return project;
		}

		void CreateFilesFromTemplate (Solution solution, DnxProject project)
		{
			string projectTemplateName = Parameters ["Template"];

			string[] files = Parameters["Files"].Split ('|');
			FileTemplateProcessor.CreateFilesFromTemplate (solution, project, projectTemplateName, files);
		}

		void AddSolutionFoldersToSolution (Solution solution)
		{
			AddSolutionItemsFolder (solution);

			SolutionFolder srcFolder = solution.AddSolutionFolder ("src");

			string projectName = Parameters["UserDefinedProjectName"];
			DnxProject project = CreateProject (solution, projectName);
			srcFolder.AddItem (project, true);

			project.CreateProjectDirectory ();

			RemoveProjectDirectoryCreatedByNewProjectDialog (solution.BaseDirectory, projectName);

			CreateFilesFromTemplate (solution, project);

			solution.Save (new NullProgressMonitor ());

			OpenProjectFile (project);

			DnxServices.ProjectService.LoadAspNetProjectSystem (solution);
		}

		void AddSolutionItemsFolder (Solution solution)
		{
			string globalJsonFile = solution.BaseDirectory.Combine ("global.json");
			solution.AddSolutionFolder ("Solution Items", globalJsonFile);
		}

		void RemoveProjectDirectoryCreatedByNewProjectDialog (FilePath parentDirectory, string projectName)
		{
			FilePath projectDirectory = parentDirectory.Combine (projectName);
			EmptyDirectoryRemover.Remove (projectDirectory);
		}

		void OpenProjectFile (DnxProject project)
		{
			IdeApp.Workbench.OpenProjectFile (project, Parameters["OpenFile"]);
		}
	}
}

