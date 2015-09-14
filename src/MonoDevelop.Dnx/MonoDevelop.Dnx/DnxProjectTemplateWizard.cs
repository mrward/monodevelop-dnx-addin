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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Templates;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Core;
using System.Xml;

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

		Solution GetSolution (IEnumerable<IWorkspaceFileObject> items)
		{
			if (items.Count () == 1) {
				return items.OfType<Solution> ().FirstOrDefault ();
			}
			return null;
		}

		void AddSolutionFoldersToSolution (Solution solution)
		{
			var solutionItemsFolder = new SolutionFolder {
				Name = "Solution Items"
			};
			solutionItemsFolder.Files.Add (solution.BaseDirectory.Combine ("global.json"));
			solution.RootFolder.AddItem (solutionItemsFolder);

			var srcFolder = new SolutionFolder {
				Name = "src"
			};
			solution.RootFolder.AddItem (srcFolder);

			var project = new DnxProject ();
			project.IsDirty = true;
			string projectName = Parameters["UserDefinedProjectName"];
			project.FileName = solution.BaseDirectory.Combine ("src", projectName, projectName).ChangeExtension (".xproj");

			if (!Directory.Exists (project.BaseDirectory)) {
				Directory.CreateDirectory (project.BaseDirectory);
			}

			project.AddConfigurations ();
			srcFolder.AddItem (project, true);

			CreateFileFromTemplate (solution, "global.json");
			CreateFileFromTemplate (project, "LibraryProject.MyClass.cs");
			CreateFileFromTemplate (project, "LibraryProject.project.json");

			project.Save (new NullProgressMonitor ());
			solution.Save (new NullProgressMonitor ());

			string fileToOpen = project.BaseDirectory.Combine ("MyClass.cs");

			IdeApp.Workbench.OpenDocument (fileToOpen, project, false);

			var pad = IdeApp.Workbench.Pads.SolutionPad;
			if (pad != null) {
				var solutionPad = pad.Content as SolutionPad;
				if (solutionPad != null) {
					SelectFile (solutionPad.TreeView, project, fileToOpen);
				}
			}

			DnxServices.ProjectService.LoadAspNetProjectSystem (solution);
		}

		void CreateFileFromTemplate (Project project, string templateName)
		{
			CreateFileFromTemplate (project, project.ParentSolution.RootFolder, templateName);
		}

		void CreateFileFromTemplate (Project project, SolutionItem policyItem, string templateName)
		{
			FilePath templateSourceDirectory = GetTemplateSourceDirectory ();
			string templateFileName = templateSourceDirectory.Combine (templateName + ".xft.xml");
			using (Stream stream = File.OpenRead (templateFileName)) {
				var document = new XmlDocument ();
				document.Load (stream);

				foreach (XmlElement templateElement in document.DocumentElement["TemplateFiles"].ChildNodes.OfType<XmlElement> ()) {
					var template = FileDescriptionTemplate.CreateTemplate (templateElement, templateSourceDirectory);
					template.AddToProject (policyItem, project, "C#", project.BaseDirectory, null);
				}
			}
		}

		void CreateFileFromTemplate (Solution solution, string templateName)
		{
			var project = new GenericProject ();
			project.BaseDirectory = solution.BaseDirectory;
			CreateFileFromTemplate (project, solution.RootFolder, templateName);
		}

		FilePath GetTemplateSourceDirectory ()
		{
			var assemblyPath = new FilePath (typeof(DnxProjectTemplateWizard).Assembly.Location);
			return assemblyPath.ParentDirectory.Combine ("Templates", "Projects");
		}

		void SelectFile (ExtensibleTreeView treeView, Project project, string file)
		{
			var projectFile = project.Files.GetFile (file);
			if (projectFile == null)
				return;

			ITreeNavigator navigator = treeView.GetNodeAtObject (projectFile, true);
			if (navigator != null) {
				navigator.ExpandToNode ();
				navigator.Selected = true;
			}
		}
	}
}

