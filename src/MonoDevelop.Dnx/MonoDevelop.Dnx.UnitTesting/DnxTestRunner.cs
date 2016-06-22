//
// DnxTestRunner.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
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
using System.Linq;
using Microsoft.DotNet.ProjectModel.Server.Models;
using Microsoft.DotNet.Tools.Test;
using Microsoft.Extensions.Testing.Abstractions;
using MonoDevelop.Core;
using MonoDevelop.UnitTesting;

namespace MonoDevelop.Dnx.UnitTesting
{
	public class DnxTestRunner : IDisposable
	{
		TestContext testContext;
		DotNetCoreTestServer testServer;
		DotNetCoreTestConsoleWrapper dotNetTestConsole;
		IDnxTestProvider rootTest;
		UnitTest currentTest;
		bool runningSingleTest;

		public DnxTestRunner (TestContext testContext, IDnxTestProvider rootTest)
		{
			this.testContext = testContext;
			this.rootTest = rootTest;
			runningSingleTest = rootTest is DnxUnitTest;
			TestResult = UnitTestResult.CreateSuccess ();
		}

		public string WorkingDirectory { get; set; }

		public bool IsCompleted { get; private set; }

		public UnitTestResult TestResult { get; private set; }

		public void Run ()
		{
			try {
				testServer = new DotNetCoreTestServer (OnMessageReceived);
				testServer.Open ();

				RunDotNetCoreTest ();
			} catch (Exception ex) {
				TestResult = UnitTestResult.CreateFailure (ex);
				IsCompleted = true;
			}
		}

		public void Dispose ()
		{
			if (testServer != null) {
				testServer.Dispose ();
			}
		}

		void OnMessageReceived (Message m)
		{
			try {
				if (m.MessageType == TestMessageTypes.TestSessionConnected) {
					testServer.GetTestRunnerStartInfo (rootTest.GetTests ());
				} else if (m.MessageType == TestMessageTypes.TestExecutionTestRunnerProcessStartInfo) {
					var val = m.Payload.ToObject<TestStartInfoMessage> ();
					RunTestsWithDotNetCoreTest (val);
				} else if (m.MessageType == TestMessageTypes.TestRunnerTestResult) {
					var val = m.Payload.ToObject<TestResultMessage> ();
					OnTestResult (val);
				} else if (m.MessageType == TestMessageTypes.TestRunnerTestStarted) {
					if (!runningSingleTest) {
						var val = m.Payload.ToObject<TestStartedMessage> ();
						OnTestStarted (val);
					}
				} else if (m.MessageType == TestMessageTypes.TestExecutionCompleted) {
					testServer.TerminateTestSession ();
					IsCompleted = true;
				} else if (m.MessageType == "Error") {
					var val = m.Payload.ToObject<Microsoft.DotNet.Tools.Test.ErrorMessage> ();
					LoggingService.LogError ("Test runner error", val.Message);
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Test runner error", ex);
			}
		}

		void RunDotNetCoreTest ()
		{
			var console = testContext.ExecutionContext.ConsoleFactory.CreateConsole (testContext.Monitor.CancellationToken);
			dotNetTestConsole = new DotNetCoreTestConsoleWrapper (console);

			Runtime.ProcessService.StartConsoleProcess (
				DnxServices.ProjectService.CurrentDotNetRuntimePath,
				String.Format ("test --port {0} --no-build", testServer.Port),
				WorkingDirectory,
				console,
				null,
				(sender, e) => {
					//Console.WriteLine ("Exited");
				});
		}

		void RunTestsWithDotNetCoreTest (TestStartInfoMessage testStartInfo)
		{
			Runtime.ProcessService.StartConsoleProcess (
				testStartInfo.FileName,
				testStartInfo.Arguments,
				WorkingDirectory,
				dotNetTestConsole,
				null,
				(sender, e) => {
					//Console.WriteLine ("dotnet test testRunner exited");
				});
		}

		UnitTestResult AddTestResult (TestResultMessage message)
		{
			var result = new UnitTestResult {
				ConsoleError = message.ErrorMessage,
				ConsoleOutput = GetConsoleOutput (message),
				Message = message.ErrorMessage,
				Status = ToResultStatus (message.Outcome),
				StackTrace = message.ErrorStackTrace,
				TestDate = message.StartTime.DateTime,
				Time = message.Duration,
			};

			UpdateCounts (result);

			TestResult.Add (result);

			return result;
		}

		void UpdateCounts (UnitTestResult result)
		{
			UpdateCounts (result, result);
		}

		void UpdateCounts (UnitTestResult parentResult, UnitTestResult result)
		{
			switch (result.Status) {
				case ResultStatus.Failure:
					parentResult.Failures++;
				break;

				case ResultStatus.Ignored:
					parentResult.Ignored++;
				break;

				case ResultStatus.Success:
					parentResult.Passed++;
				break;

				case ResultStatus.Inconclusive:
					parentResult.Inconclusive++;
				break;
			}
		}

		static ResultStatus ToResultStatus (TestOutcome outcome)
		{
			switch (outcome) {
				case TestOutcome.Passed:
					return ResultStatus.Success;
					
				case TestOutcome.Failed:
				case TestOutcome.NotFound:
					return ResultStatus.Failure;

				case TestOutcome.None:
					return ResultStatus.Inconclusive;

				case TestOutcome.Skipped:
					return ResultStatus.Ignored;

				default:
					return ResultStatus.Inconclusive;
			}
		}

		string GetConsoleOutput (TestResultMessage message)
		{
			if (message.Messages != null) {
				return String.Join (Environment.NewLine, message.Messages);
			}

			return String.Empty;
		}

		void OnTestStarted (TestStartedMessage message)
		{
			string testId = message.Id?.ToString ();
			if (testId == null)
				return;

			currentTest = FindTest (rootTest as UnitTest, testId);
			if (currentTest != null) {
				testContext.Monitor.BeginTest (currentTest);
				currentTest.Status = TestStatus.Running;
				UpdateParentStatus ();
			}
		}

		UnitTest FindTest (UnitTest test, string testId)
		{
			var testGroup = test as UnitTestGroup;
			if (testGroup == null) {
				if (test.TestId == testId) {
					return test;
				}
				return null;
			}

			foreach (UnitTest child in testGroup.Tests) {
				if (child.TestId == testId) {
					return child;
				}

				var foundTest = FindTest (child, testId);
				if (foundTest != null) {
					return foundTest;
				}
			}

			return null;
		}

		void OnTestResult (TestResultMessage message)
		{
			UnitTestResult result = AddTestResult (message);

			if (runningSingleTest)
				return;

			string testId = message.Test?.Id?.ToString ();
			if (currentTest == null || testId != currentTest.TestId) {
				currentTest = FindTest (rootTest as UnitTest, testId);
			}

			if (currentTest != null) {
				currentTest.RegisterResult (testContext, result);
				testContext.Monitor.EndTest (currentTest, result);
				currentTest.Status = TestStatus.Ready;
				UpdateParentStatus ();
			}
		}

		void UpdateParentStatus ()
		{
			var parent = currentTest.Parent as UnitTestGroup;

			while (parent != null && parent != rootTest && !(parent is DnxProjectTestSuite)) {
				if (currentTest.Status == TestStatus.Running) {
					OnChildTestRunning (parent);
				} else if (currentTest.Status == TestStatus.Ready) {
					OnChildTestReady (parent);
				}

				parent = parent.Parent as UnitTestGroup;
			} 
		}

		void OnChildTestRunning (UnitTestGroup parent)
		{
			if (parent.Status != TestStatus.Running) {
				testContext.Monitor.BeginTest (parent);
				parent.Status = TestStatus.Running;
			}
		}

		void OnChildTestReady (UnitTestGroup parent)
		{
			if (parent.Tests.All (test => test.Status == TestStatus.Ready)) {
				UnitTestResult result = GenerateResultFromChildTests (parent);
				parent.RegisterResult (testContext, result);
				testContext.Monitor.EndTest (parent, result);
				parent.Status = TestStatus.Ready;
			}
		}

		UnitTestResult GenerateResultFromChildTests (UnitTestGroup parent)
		{
			var result = UnitTestResult.CreateSuccess ();
			foreach (UnitTest test in parent.Tests) {
				UnitTestResult childResult = test.GetLastResult ();
				result.Add (childResult);
				UpdateCounts (result, childResult);
			}
			return result;
		}
	}
}

