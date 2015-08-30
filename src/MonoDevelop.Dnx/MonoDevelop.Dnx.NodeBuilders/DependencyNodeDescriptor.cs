//
// DependencyNodeDescriptor.cs
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

using MonoDevelop.Core;
using MonoDevelop.DesignerSupport;

namespace MonoDevelop.Dnx.NodeBuilders
{
	public class DependencyNodeDescriptor : CustomDescriptor
	{
		DependencyNode node;

		public DependencyNodeDescriptor (DependencyNode node)
		{
			this.node = node;
		}

		[LocalizedCategory ("Dependency")]
		[LocalizedDisplayName ("Name")]
		public string Name {
			get { return node.Name; }
		}

		[LocalizedCategory ("Dependency")]
		[LocalizedDisplayName ("Path")]
		public string Path {
			get { return node.Path; }
		}

		[LocalizedCategory ("Dependency")]
		[LocalizedDisplayName ("Type")]
		public string Type {
			get { return node.Type; }
		}

		[LocalizedCategory ("Dependency")]
		[LocalizedDisplayName ("Version")]
		public string Version {
			get { return node.Version; }
		}
	}
}

