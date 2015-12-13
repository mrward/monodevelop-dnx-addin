//
// DependenciesFolderNodeBuilder.cs
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
using MonoDevelop.Dnx.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;

namespace MonoDevelop.Dnx.NodeBuilders
{
	public class DependenciesFolderNodeBuilder : TypeNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof(DependenciesFolderNode); }
		}

		public override string ContextMenuAddinPath {
			get { return "/MonoDevelop/Dnx/ContextMenu/ProjectPad/DependenciesFolderNode"; }
		}

		public override Type CommandHandlerType {
			get { return typeof(DependenciesFolderNodeCommandHandler); }
		}

		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			return "Dependencies";
		}

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, NodeInfo nodeInfo)
		{
			var node = (DependenciesFolderNode)dataObject;
			nodeInfo.Label = node.GetLabel ();
			nodeInfo.Icon = Context.GetIcon (node.Icon);
			nodeInfo.ClosedIcon = Context.GetIcon (node.ClosedIcon);
			nodeInfo.StatusSeverity = node.GetStatusSeverity ();
			nodeInfo.StatusMessage = node.GetStatusMessage ();
		}

		public override int CompareObjects (ITreeNavigator thisNode, ITreeNavigator otherNode)
		{
			if (otherNode.DataItem is ProjectReferenceCollection) {
				return 1;
			}
			return -1;
		}

		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			var node = (DependenciesFolderNode)dataObject;
			return node.HasDependencies ();
		}

		public override void BuildChildNodes (ITreeBuilder treeBuilder, object dataObject)
		{
			var node = (DependenciesFolderNode)dataObject;
			foreach (FrameworkNode frameworkFolderNode in node.GetFrameworkFolderNodes ()) {
				treeBuilder.AddChild (frameworkFolderNode);
			}
		}

		public override void OnNodeAdded (object dataObject)
		{
			base.OnNodeAdded (dataObject);

			var node = (DependenciesFolderNode)dataObject;
			var project = node.Project;
			project.DependenciesChanged += ProjectDependenciesChanged;
			project.PackageRestoreStarted += PackageRestoreStarted;
			project.PackageRestoreFinished += PackageRestoreFinished;
			DnxServices.ProjectService.ProjectSystemLoadFailed += ProjectSystemLoadFailed;
		}

		public override void OnNodeRemoved (object dataObject)
		{
			base.OnNodeRemoved (dataObject);

			var node = (DependenciesFolderNode)dataObject;
			var project = node.Project;
			project.DependenciesChanged -= ProjectDependenciesChanged;
			project.PackageRestoreStarted -= PackageRestoreStarted;
			project.PackageRestoreFinished -= PackageRestoreFinished;
			DnxServices.ProjectService.ProjectSystemLoadFailed -= ProjectSystemLoadFailed;
		}

		void ProjectDependenciesChanged (object sender, EventArgs e)
		{
			RefreshNode (sender);
		}

		void ProjectSystemLoadFailed (object sender, EventArgs e)
		{
			RefreshAllDnxProjects ();
		}

		void RefreshAllDnxProjects ()
		{
			foreach (Solution solution in IdeApp.Workspace.GetAllSolutions ()) {
				foreach (DnxProject project in solution.GetDnxProjects ()) {
					RefreshNode (project);
				}
			}
		}

		void RefreshNode (object project)
		{
			ITreeBuilder builder = GetBuilder ((DnxProject)project);
			if (builder != null)
				builder.UpdateAll ();
		}

		ITreeBuilder GetBuilder (object sender)
		{
			var project = (DnxProject)sender;
			ITreeBuilder builder = Context.GetTreeBuilder (project.Project);
			if (builder != null && builder.MoveToChild ("Dependencies", typeof(DependenciesFolderNode)))
				return builder;

			return null;
		}

		void PackageRestoreStarted (object sender, EventArgs e)
		{
			RefreshNode (sender);
		}

		void PackageRestoreFinished (object sender, EventArgs e)
		{
			RefreshNode (sender);
		}
	}
}

