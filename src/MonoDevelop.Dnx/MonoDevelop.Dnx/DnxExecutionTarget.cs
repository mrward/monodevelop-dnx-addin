//
// DnxExecutionTarget.cs
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
using MonoDevelop.Core.Execution;
using OmniSharp.Models;

namespace MonoDevelop.Dnx
{
	public class DnxExecutionTarget : ExecutionTarget
	{
		string id;
		string name;

		public static readonly string DefaultTargetId = "Profile";

		DnxExecutionTarget (string id, string name)
		{
			this.id = id;
			this.name = name;
		}

		public DnxExecutionTarget (string command, DnxFramework framework)
		{
			Command = command;
			Framework = framework;
			name = GenerateName ();
			id = GenerateId (command, framework.ShortName);
		}

		public static DnxExecutionTarget CreateDefaultTarget (string command)
		{
			return new DnxExecutionTarget (GenerateId (command, DefaultTargetId), command) {
				Command = command
			};
		}

		public override string Id {
			get { return id; }
		}

		public override string Name {
			get { return name; }
		}

		public DnxFramework Framework { get; private set; }
		public string Command { get; private set; }

		public bool IsDefaultProfile {
			get { return Id == DefaultTargetId; }
		}

		string GenerateName ()
		{
			if (IsDefaultProfile) {
				return Command;
			}

			return String.Format ("{0} {1}", Command, Framework.FriendlyName);
		}

		static string GenerateId (string command, string framework)
		{
			return String.Format ("{0}|{1}", command, framework);
		}

		public override string ToString ()
		{
			return Name;
		}
	}
}

