//
// DependencyNode.cs
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
using System.Linq;
using Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Tasks;

namespace MonoDevelop.Dnx.NodeBuilders
{
	public class DependencyNode
	{
		DependenciesMessage message;
		DependencyDescription dependency;

		public DependencyNode (DependenciesMessage message, DependencyDescription dependency)
		{
			this.message = message;
			this.dependency = dependency;
		}

		public string Name {
			get { return dependency.Name; }
		}

		public string Version {
			get { return dependency.Version; }
		}

		public string Type {
			get { return dependency.Type; }
		}

		public string Path {
			get { return dependency.Path; }
		}

		public string GetLabel ()
		{
			return String.Format ("{0} <span color='grey'>({1})</span>", dependency.Name, dependency.Version);
		}

		public IconId GetIconId ()
		{
			if (Type == "Package" || Unresolved)
				return new IconId ("md-dnx-nuget-package");

			return Stock.Reference;
		}

		public bool HasDependencies ()
		{
			return dependency.Dependencies.Any ();
		}

		public IEnumerable<DependencyNode> GetDependencies ()
		{
			foreach (DependencyItem item in dependency.Dependencies) {
				var matchedDependency = message.Dependencies[item.Name];
				if (matchedDependency != null) {
					yield return new DependencyNode (message, matchedDependency);
				}
			}
		}

		public TaskSeverity? GetStatusSeverity ()
		{
			if (Unresolved)
				return TaskSeverity.Warning;

			return null;
		}

		public string GetStatusMessage ()
		{
			if (Unresolved)
				return GettextCatalog.GetString ("Dependency has not been resolved");

			return null;
		}

		public bool IsDisabled ()
		{
			return Unresolved;
		}

		public bool Unresolved {
			get { return dependency.Type == "Unresolved"; }
		}
	}
}

