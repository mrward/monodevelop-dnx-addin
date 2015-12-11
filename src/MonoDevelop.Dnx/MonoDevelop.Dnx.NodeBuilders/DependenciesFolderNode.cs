//
// DependenciesFolderNode.cs
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
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Tasks;

namespace MonoDevelop.Dnx.NodeBuilders
{
	public class DependenciesFolderNode
	{
		public DnxProject Project { get; private set; }

		public DependenciesFolderNode (DnxProject project)
		{
			Project = project;
		}

		public string GetLabel ()
		{
			return GettextCatalog.GetString ("Dependencies") + GetRestoringLabel ();
		}

		string GetRestoringLabel ()
		{
			if (IsRestoringPackages) {
				string restoringMessage = GettextCatalog.GetString ("Restoring...");
				return String.Format (" <span color='grey'>({0})</span>", restoringMessage);
			}
			return String.Empty;
		}

		public IconId Icon {
			get { return Stock.OpenReferenceFolder; }
		}

		public IconId ClosedIcon {
			get { return Stock.ClosedReferenceFolder; }
		}

		public TaskSeverity? GetStatusSeverity ()
		{
			if (!DnxServices.ProjectService.HasCurrentDnxRuntime)
				return TaskSeverity.Error;

			return null;
		}

		public string GetStatusMessage ()
		{
			if (!DnxServices.ProjectService.HasCurrentDnxRuntime)
				return DnxServices.ProjectService.CurrentRuntimeError;

			return null;
		}

		public bool HasDependencies ()
		{
			return Project.HasDependencies ();
		}

		public IEnumerable<FrameworkNode> GetFrameworkFolderNodes ()
		{
			foreach (var dependency in Project.GetDependencies ()) {
				yield return new FrameworkNode (dependency);
			}
		}

		public bool IsRestoringPackages {
			get { return Project.IsRestoringPackages; }
		}
	}
}

