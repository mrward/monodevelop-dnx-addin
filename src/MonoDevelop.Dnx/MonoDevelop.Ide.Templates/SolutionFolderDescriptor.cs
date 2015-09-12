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
using System.Xml;
using MonoDevelop.Projects;
using MonoDevelop.Core;

namespace MonoDevelop.Ide.Templates
{
	class SolutionFolderDescriptor
	{
		SolutionFolderDescriptor ()
		{
			Name = "";
		}

		public string Name { get; set; }

		public static SolutionFolderDescriptor CreateDescriptor (XmlElement xmlElement)
		{
			var folder = new SolutionFolderDescriptor ();
			folder.Name = GetName (xmlElement);

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

		string GetName (ProjectCreateInformation projectCreateInformation)
		{
			return StringParserService.Parse (Name, new string[,] {
				{"ProjectName", projectCreateInformation.ProjectName}
			});
		}
	}
}

