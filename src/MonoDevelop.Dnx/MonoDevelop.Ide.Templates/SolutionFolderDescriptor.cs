//
// SolutionFolderDescriptor.cs
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
using System.Xml;

using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.Ide.Templates
{
	class SolutionFolderDescriptor
	{
		List<SolutionFolderFileDescriptor> files = new List<SolutionFolderFileDescriptor> ();
		List<ProjectDescriptor> projects = new List<ProjectDescriptor> ();

		SolutionFolderDescriptor ()
		{
		}

		public string Name { get; set; }

		public static SolutionFolderDescriptor CreateDescriptor (XmlElement xmlElement, FilePath baseDirectory)
		{
			var folder = new SolutionFolderDescriptor ();
			folder.Name = GetName (xmlElement);
			folder.files = GetFiles (xmlElement).ToList ();
			folder.projects = GetProjects (xmlElement, baseDirectory).ToList ();

			return folder;
		}

		static string GetName (XmlElement xmlElement)
		{
			XmlAttribute nameAttribute = xmlElement.Attributes["name"];
			if (nameAttribute != null)
				return nameAttribute.Value;
			else
				throw new InvalidOperationException ("Attribute 'name' not found");
		}

		public SolutionFolder CreateFolder (ProjectCreateInformation projectCreateInformation)
		{
			return new SolutionFolder {
				Name = GetName (projectCreateInformation)
			};
		}

		public void Initialize (SolutionFolder folder, ProjectCreateInformation projectCreateInformation, string defaultLanguage)
		{
			foreach (SolutionFolderFileDescriptor fileDescriptor in files) {
				fileDescriptor.AddFileToFolder (folder, projectCreateInformation);
			}

			foreach (ProjectDescriptor projectDescriptor in projects) {
				CreateProject (folder, projectDescriptor, projectCreateInformation, defaultLanguage);
			}
		}

		string GetName (ProjectCreateInformation projectCreateInformation)
		{
			return StringParserService.Parse (Name, new string[,] {
				{"ProjectName", projectCreateInformation.ProjectName}
			});
		}

		static IEnumerable<SolutionFolderFileDescriptor> GetFiles (XmlElement xmlElement)
		{
			XmlElement solutionFileItems = xmlElement["SolutionFolderFileItems"];
			if (solutionFileItems != null) {
				foreach (XmlElement fileItem in solutionFileItems.OfType<XmlElement> ()) {
					yield return SolutionFolderFileDescriptor.CreateDescriptor (fileItem);
				}
			}
		}

		static IEnumerable<ProjectDescriptor> GetProjects (XmlElement xmlElement, FilePath baseDirectory)
		{
			foreach (XmlElement projectElement in xmlElement.ChildNodes.OfType<XmlElement> ()
				.Where (e => e.Name == "Project")) {
				yield return ProjectDescriptor.CreateProjectDescriptor (projectElement, baseDirectory);
			}
		}

		void CreateProject (
			SolutionFolder folder,
			ProjectDescriptor projectDescriptor,
			ProjectCreateInformation originalProjectCreateInformation,
			string defaultLanguage)
		{
			ProjectCreateInformation projectCreateInformation =
				CreateProjectCreateInformation (projectDescriptor, originalProjectCreateInformation);

			SolutionEntityItem project = projectDescriptor.CreateItem (projectCreateInformation, defaultLanguage);
			if (project == null)
				return;

			projectDescriptor.InitializeItem (folder.ParentSolution.RootFolder, projectCreateInformation, defaultLanguage, project);

			AddProjectConfigurationToSolution (project, folder.ParentSolution);

			folder.Items.Add (project);
		}

		ProjectCreateInformation CreateProjectCreateInformation (ProjectDescriptor projectDescriptor, ProjectCreateInformation originalProjectCreateInformation)
		{
			var entry = projectDescriptor as ICustomProjectCIEntry;
			if (entry == null)
				return originalProjectCreateInformation;

			var projectCreateInformation = entry.CreateProjectCI (originalProjectCreateInformation);
			return new ProjectTemplateCreateInformation (projectCreateInformation, originalProjectCreateInformation.ProjectName);
		}

		void AddProjectConfigurationToSolution (SolutionEntityItem info, Solution solution)
		{
			var configurationTarget = info as IConfigurationTarget;
			if (configurationTarget == null)
				return;

			foreach (ItemConfiguration configuration in configurationTarget.Configurations) {
				bool flag = false;
				foreach (SolutionConfiguration solutionCollection in solution.Configurations) {
					if (solutionCollection.Id == configuration.Id)
						flag = true;
				}
				if (!flag)
					solution.AddConfiguration (configuration.Id, true);
			}
		}
	}
}

