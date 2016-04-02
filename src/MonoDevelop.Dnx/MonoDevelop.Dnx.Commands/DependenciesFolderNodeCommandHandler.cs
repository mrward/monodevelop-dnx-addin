//
// DependenciesFolderNodeCommandHandler.cs
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
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Dnx.NodeBuilders;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;

namespace MonoDevelop.Dnx.Commands
{
	public class DependenciesFolderNodeCommandHandler : NodeCommandHandler
	{
		public override void ActivateItem ()
		{
			var node = (DependenciesFolderNode)CurrentNode.DataItem;
			var handler = new AddNuGetPackagesToSelectedProjectHandler ();
			handler.AddPackages (node.Project);
		}

		[CommandUpdateHandler (DnxCommands.Restore)]
		void OnUpdateRestore (CommandInfo info)
		{
			var node = (DependenciesFolderNode)CurrentNode.DataItem;
			info.Enabled = !node.Project.IsRestoringPackages && node.Project.JsonPath != null;
		}

		[CommandHandler (DnxCommands.Restore)]
		void Restore ()
		{
			try {
				var node = (DependenciesFolderNode)CurrentNode.DataItem;
				DnxServices.ProjectService.Restore (node.Project.JsonPath);
			} catch (Exception ex) {
				LoggingService.LogError ("Restore failed", ex);
				MessageService.ShowError ("Restore failed", ex);
			}
		}
	}
}

