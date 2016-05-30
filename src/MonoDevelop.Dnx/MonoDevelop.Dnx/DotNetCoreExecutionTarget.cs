//
// DotNetCoreExecutionTarget.cs
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
using MonoDevelop.Core.Execution;
using OmniSharp.Models;

namespace MonoDevelop.Dnx
{
	public class DotNetCoreExecutionTarget : ExecutionTarget
	{
		string id;
		string name;

		public static readonly string DefaultTargetId = "Profile";

		DotNetCoreExecutionTarget (string id, string name)
		{
			this.id = id;
			this.name = name;
		}

		public DotNetCoreExecutionTarget (DnxFramework framework)
		{
			Framework = framework;
			name = GenerateName ();
			id = framework.ShortName;
		}

		public static DotNetCoreExecutionTarget CreateDefaultTarget ()
		{
			return new DotNetCoreExecutionTarget (DefaultTargetId, GettextCatalog.GetString ("Default")) {
				IsDefaultProfile = true
			};
		}

		public override string Id {
			get { return id; }
		}

		public override string Name {
			get { return name; }
		}

		public DnxFramework Framework { get; private set; }
		public bool IsDefaultProfile { get; private set; }

		string GenerateName ()
		{
			if (Framework == null) {
				return Id;
			}

			return Framework.FriendlyName;
		}

		public override string ToString ()
		{
			return Name;
		}
	}
}

