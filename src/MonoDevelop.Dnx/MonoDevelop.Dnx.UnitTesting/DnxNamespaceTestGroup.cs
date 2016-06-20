//
// DnxNamespaceTestGroup.cs
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
using Microsoft.Extensions.Testing.Abstractions;
using MonoDevelop.Core.Execution;
using MonoDevelop.UnitTesting;

namespace MonoDevelop.Dnx.UnitTesting
{
	public class DnxNamespaceTestGroup : UnitTestGroup, IDnxTestProvider
	{
		DnxNamespaceTestGroup currentNamespace;
		DnxTestClass currentClass;
		IDnxTestRunner testRunner;

		public DnxNamespaceTestGroup (IDnxTestRunner testRunner, UnitTestGroup parent, string name)
			: base (name)
		{
			currentNamespace = this;
			this.testRunner = testRunner;

			if (parent == null || String.IsNullOrEmpty (parent.FixtureTypeNamespace)) {
				FixtureTypeNamespace = name;
			} else {
				FixtureTypeNamespace = parent.FixtureTypeNamespace + "." + name;
			}
		}

		public void AddTests (IEnumerable<TestDiscovered> tests)
		{
			foreach (TestDiscovered test in tests) {
				var dnxTest = new DnxUnitTest (testRunner, test);
				AddTest (dnxTest);
			}
		}

		void AddTest (DnxUnitTest dnxTest)
		{
			string childNamespace = dnxTest.GetChildNamespace (FixtureTypeNamespace);
			if (String.IsNullOrEmpty (childNamespace)) {
				if (currentClass == null || currentClass.FixtureTypeName != dnxTest.FixtureTypeName) {
					currentClass = new DnxTestClass (testRunner, dnxTest.FixtureTypeName);
					Tests.Add (currentClass);
				}
				currentClass.Tests.Add (dnxTest);
			} else if (currentNamespace.Name == childNamespace) {
				currentNamespace.AddTest (dnxTest);
			} else {
				currentNamespace = new DnxNamespaceTestGroup (testRunner, currentNamespace, childNamespace);
				currentNamespace.AddTest (dnxTest);
				Tests.Add (currentNamespace);
			}
		}

		protected override bool OnCanRun (IExecutionHandler executionContext)
		{
			return false;
		}

		public override bool HasTests {
			get { return true; }
		}

		protected override UnitTestResult OnRun (TestContext testContext)
		{
			return testRunner.RunTest (testContext, this);
		}

		public IEnumerable<string> GetTests ()
		{
			foreach (IDnxTestProvider testProvider in Tests) {
				foreach (string childTest in testProvider.GetTests ()) {
					yield return childTest;
				}
			}
		}
	}
}

