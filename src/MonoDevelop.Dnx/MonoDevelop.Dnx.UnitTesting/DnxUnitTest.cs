//
// DnxUnitTest.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
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
using Microsoft.Extensions.Testing.Abstractions;
using MonoDevelop.Core.Execution;
using MonoDevelop.UnitTesting;

namespace MonoDevelop.Dnx.UnitTesting
{
	public class DnxUnitTest : UnitTest, IDnxTestProvider
	{
		TestDiscovered test;
		IDnxTestRunner testRunner;
		string name;
		SourceCodeLocation sourceCodeLocation;

		public DnxUnitTest (IDnxTestRunner testRunner, TestDiscovered test)
			: base (test.DisplayName)
		{
			this.testRunner = testRunner;
			this.test = test;

			Init ();
		}

		void Init ()
		{
			TestId = test.Id?.ToString ();
			sourceCodeLocation = new SourceCodeLocation (test.CodeFilePath, test.LineNumber, 0);

			int index = test.FullyQualifiedName.LastIndexOf ('.');
			if (index > 0) {
				name = test.FullyQualifiedName.Substring (index + 1);
				FixtureTypeName = test.FullyQualifiedName.Substring (0, index);

				index = FixtureTypeName.LastIndexOf ('.');
				if (index > 0) {
					FixtureTypeNamespace = FixtureTypeName.Substring (0, index);
					FixtureTypeName =  FixtureTypeName.Substring (index + 1);
				} else {
					FixtureTypeNamespace = String.Empty;
				}
			} else {
				name = test.DisplayName;
			}
		}

		protected override UnitTestResult OnRun (TestContext testContext)
		{
			return testRunner.RunTest (testContext, this);
		}

		protected override bool OnCanRun (IExecutionHandler executionContext)
		{
			return false;
		}

		public override SourceCodeLocation SourceCodeLocation {
			get { return sourceCodeLocation; }
		}

		public override string Name {
			get { return name; }
		}

		public string GetChildNamespace (string name)
		{
			string childNamespace = FixtureTypeNamespace;
			if (name.Length > 0) {
				if (name.Length >= FixtureTypeNamespace.Length) {
					return String.Empty;
				}
				childNamespace = FixtureTypeNamespace.Substring (name.Length + 1);
			}

			int index = childNamespace.IndexOf ('.');
			if (index >= 0) {
				childNamespace = childNamespace.Substring (0, index);
			}

			return childNamespace;
		}

		public IEnumerable<string> GetTests ()
		{
			yield return test.FullyQualifiedName;
		}
	}
}

