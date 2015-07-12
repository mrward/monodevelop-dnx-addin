//
// DnxProject.cs
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
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.Dnx
{
	public class DnxProject : Project
	{
		public DnxProject ()
		{
		}

		public DnxProject (ProjectCreateInformation info, XmlElement projectOptions)
		{
		}

		public override IEnumerable<string> GetProjectTypes ()
		{
			yield return "DnxProject";
		}

		protected override void OnEndLoad ()
		{
			foreach (string fileName in Directory.GetFiles (BaseDirectory, "*.*", SearchOption.AllDirectories)) {
				if (IsSupportedProjectFileItem (fileName)) {
					Items.Add (CreateFileProjectItem(fileName));
				}
			}
			base.OnEndLoad ();
		}

		bool IsSupportedProjectFileItem (string fileName)
		{
			string extension = Path.GetExtension(fileName);
			if (extension.EndsWith ("proj", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals (".sln", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals (".user", StringComparison.OrdinalIgnoreCase)) {
				return false;
			}
			return true;
		}

		ProjectFile CreateFileProjectItem (string fileName)
		{
			return new ProjectFile (fileName, GetDefaultBuildAction (fileName));
		}

		public override string GetDefaultBuildAction (string fileName)
		{
			if (IsCSharpFile (fileName)) {
				return BuildAction.Compile;
			}
			return base.GetDefaultBuildAction (fileName);
		}

		static bool IsCSharpFile (string fileName)
		{
			string extension = Path.GetExtension (fileName);
			return String.Equals (".cs", extension, StringComparison.OrdinalIgnoreCase);
		}
	}
}

