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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.UnitTesting;

namespace MonoDevelop.Dnx.UnitTesting
{
	public class DnxProjectTestSuite : UnitTestGroup, IDnxTestProvider, IDnxTestRunner
	{
		DnxProject project;
		DnxTestLoader testLoader;
		DateTime? lastBuildTime;
		UnitTest[] oldTests;
		bool projectHasTestRunner;

		public DnxProjectTestSuite (XProject xproject, DnxProject project)
			: base (project.Name, xproject)
		{
			this.project = project;
			lastBuildTime = project.LastBuildTime;
			projectHasTestRunner = project.HasTestRunner ();

			CreateResultsStore ();

			testLoader = new DnxTestLoader ();
			testLoader.DiscoveryCompleted += TestLoaderDiscoveryCompleted;
			testLoader.DiscoveryFailed += TestLoaderDiscoveryFailed;

			IdeApp.ProjectOperations.EndBuild += AfterBuild;
		}

		void CreateResultsStore ()
		{
			string storeId = Path.GetFileName (project.FileName);
			string resultsPath = UnitTestService.GetTestResultsDirectory (project.BaseDirectory);
			ResultsStore = new BinaryResultsStore (resultsPath, storeId);
		}

		public override bool HasTests {
			get { return true; }
		}

		protected override UnitTestResult OnRun (TestContext testContext)
		{
			return RunTest (testContext, this);
		}

		public UnitTestResult RunTest (TestContext testContext, IDnxTestProvider testProvider)
		{
			if (!projectHasTestRunner) {
				string message = GettextCatalog.GetString ("Project has no testRunner defined.");
				DnxOutputPad.WriteError (message);
				throw new ApplicationException (message);
			}

			using (var runner = new DnxTestRunner (testContext, testProvider)) {
				runner.WorkingDirectory = project.BaseDirectory;
				runner.Run ();

				while (!runner.IsCompleted) {
					if (testContext.Monitor.CancellationToken.IsCancellationRequested)
						break;

					Thread.Sleep (100);
				}
				Status = TestStatus.Ready;
				return runner.TestResult;
			}
		}

		protected override bool OnCanRun (IExecutionHandler executionContext)
		{
			return false;
		}

		protected override void OnCreateTests ()
		{
			if (!projectHasTestRunner)
				return;

			if (!testLoader.IsRunning) {
				AddOldTests ();
				Status = TestStatus.Loading;
				testLoader.Start (project.BaseDirectory);
			}
		}

		void AddOldTests ()
		{
			if (oldTests != null) {
				foreach (var test in oldTests) {
					Tests.Add (test);
				}
			}
		}

		public override Task Refresh (CancellationToken ct)
		{
			return base.Refresh (ct);
		}

		public override int CountTestCases ()
		{
			return base.CountTestCases ();
		}

		public override void Dispose ()
		{
			IdeApp.ProjectOperations.EndBuild -= AfterBuild;

			testLoader.DiscoveryFailed -= TestLoaderDiscoveryFailed;
			testLoader.DiscoveryCompleted -= TestLoaderDiscoveryCompleted;
			testLoader.Dispose ();
			base.Dispose ();
		}

		void TestLoaderDiscoveryCompleted (object sender, EventArgs e)
		{
			testLoader.BuildTestInfo (this);
			var tests = testLoader.GetTests ().ToList ();

			Runtime.RunInMainThread (() => {
				Status = TestStatus.Ready;

				Tests.Clear ();

				foreach (UnitTest test in tests) {
					Tests.Add (test);
				}

				OnTestChanged ();
			});
		}

		void TestLoaderDiscoveryFailed (object sender, EventArgs e)
		{
			Runtime.RunInMainThread (() => {
				Status = TestStatus.LoadError;
			});
		}

		void AfterBuild (object sender, BuildEventArgs args)
		{
			if (!CheckProjectHasTestRunner ())
				return;

			if (RefreshRequired ()) {
				lastBuildTime = project.LastBuildTime.Value;

				SaveOldTests ();

				UpdateTests ();
			}
		}

		bool RefreshRequired ()
		{
			DateTime? buildTime = project.LastBuildTime;
			if (buildTime.HasValue) {
				if (lastBuildTime.HasValue) {
					return buildTime > lastBuildTime;
				}
				return true;
			}

			return false;
		}

		void SaveOldTests ()
		{
			if (Tests.Count > 0) {
				oldTests = new UnitTest[Tests.Count];
				Tests.CopyTo (oldTests, 0);
			}
		}

		public IEnumerable<string> GetTests ()
		{
			return null;
		}

		bool CheckProjectHasTestRunner ()
		{
			if (!projectHasTestRunner) {
				Runtime.RunInMainThread (() => {
					projectHasTestRunner = project.HasTestRunner ();
				}).Wait ();
			}

			return projectHasTestRunner;
		}

		public override string Name {
			get {
				if (projectHasTestRunner) {
					return base.Name;
				}
				return base.Name + " " + GettextCatalog.GetString ("(Project has no testRunner)");
			}
		}
	}
}

