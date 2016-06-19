//
// DnxProjectTestSuite.cs
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

using MonoDevelop.Core.Execution;
using MonoDevelop.UnitTesting;

namespace MonoDevelop.Dnx.UnitTesting
{
	public class DnxProjectTestSuite : UnitTest//UnitTestGroup
	{
		DnxProject project;

		public DnxProjectTestSuite (XProject xproject, DnxProject project)
			: base (project.Name, xproject)
		{
			this.project = project;
		}

		//public override bool HasTests {
		//	get {
		//		return true;
		//	}
		//}

		protected override UnitTestResult OnRun (TestContext testContext)
		{
			using (var runner = new DnxTestRunner (testContext)) {
				runner.WorkingDirectory = project.BaseDirectory;
				runner.Run ();

				while (!runner.IsCompleted) {
					if (testContext.Monitor.CancellationToken.IsCancellationRequested)
						break;

					System.Threading.Thread.Sleep (100);
				}
				return runner.TestResult;
			}
		}

		protected override bool OnCanRun (IExecutionHandler executionContext)
		{
			return false;
		}
	}
}

