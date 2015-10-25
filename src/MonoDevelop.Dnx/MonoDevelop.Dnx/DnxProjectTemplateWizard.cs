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
		bool createDnxProject;

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

		public override void ConfigureWizard ()
		{
			createDnxProject = !Parameters.GetBoolean ("CreateSolution");
			Parameters["CreateDnxProject"] = createDnxProject.ToString ();

			bool shouldUseKestrel = !Platform.IsWindows;
			Parameters["UseKestrel"] = shouldUseKestrel.ToString ();
		}

		public override void ItemsCreated (IEnumerable<IWorkspaceFileObject> items)
		{
			Solution solution = GetCreatedSolution (items);
			if (solution != null) {
				SolutionFolder srcFolder = AddSolutionFoldersToSolution (solution);
				CreateProject (solution, srcFolder);
				return;
			}

			DnxProject project = GetCreatedProject (items);
			solution = project.ParentSolution;
			SolutionFolder existingSrcFolder = project.GetSrcSolutionFolder ();
			RemoveProjectFromSolution (project);

			CreateProject (solution, existingSrcFolder, false);
		}

		static Solution GetCreatedSolution (IEnumerable<IWorkspaceFileObject> items)
		{
			if (items.Count () == 1) {
				return items.OfType<Solution> ().FirstOrDefault ();
			}
			return null;
		}

		static DnxProject GetCreatedProject (IEnumerable<IWorkspaceFileObject> items)
		{
			return items.OfType<DnxProject> ().FirstOrDefault ();
		}

		DnxProject CreateProject (Solution solution, string projectName)
		{
			var project = new DnxProject ();
			project.IsDirty = true;
			project.GenerateNewProjectFileName (solution, projectName);

			return project;
		}

		void CreateFilesFromTemplate (DnxProject project)
		{
			string projectTemplateName = Parameters["Template"];

			string[] files = Parameters["Files"].Split ('|');
			FileTemplateProcessor.CreateFilesFromTemplate (project, projectTemplateName, files);
		}

		void RemoveProjectFromSolution (DnxProject project)
		{
			project.ParentFolder.Items.Remove (project);
			project.Dispose ();
		}

		SolutionFolder AddSolutionFoldersToSolution (Solution solution)
		{
			AddSolutionItemsFolder (solution);
			FileTemplateProcessor.CreateFileFromCommonTemplate (solution, "global.json");

			return solution.AddSolutionFolder ("src");
		}

		void CreateProject (Solution solution, SolutionFolder srcFolder, bool newSolution = true)
		{
			string projectName = Parameters["UserDefinedProjectName"];
			DnxProject project = CreateProject (solution, projectName);
			srcFolder.AddItem (project);

			project.AddConfigurations ();

			if (newSolution) {
				solution.GenerateDefaultDnxProjectConfigurations (project);
				solution.StartupItem = project;
			} else {
				solution.EnsureConfigurationHasBuildEnabled (project);
			}

			project.CreateProjectDirectory ();

			if (Parameters.GetBoolean ("CreateWebRoot")) {
				project.CreateWebRootDirectory ();
			}

			RemoveProjectDirectoryCreatedByNewProjectDialog (solution.BaseDirectory, projectName);

			CreateFilesFromTemplate (project);

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

