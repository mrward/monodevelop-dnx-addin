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
		UnitTest rootTest;
		UnitTest currentTest;

		public DnxTestRunner (TestContext testContext, UnitTest rootTest)
		{
			this.testContext = testContext;
			this.rootTest = rootTest;
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
					testServer.GetTestRunnerStartInfo (null);
				} else if (m.MessageType == TestMessageTypes.TestExecutionTestRunnerProcessStartInfo) {
					var val = m.Payload.ToObject<TestStartInfoMessage> ();
					RunTestsWithDotNetCoreTest (val);
				} else if (m.MessageType == TestMessageTypes.TestRunnerTestResult) {
					var val = m.Payload.ToObject<TestResultMessage> ();
					OnTestResult (val);
				} else if (m.MessageType == TestMessageTypes.TestRunnerTestStarted) {
					var val = m.Payload.ToObject<TestStartedMessage> ();
					OnTestStarted (val);
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
			switch (result.Status) {
				case ResultStatus.Failure:
					result.Failures++;
				break;

				case ResultStatus.Ignored:
					result.Ignored++;
				break;

				case ResultStatus.Success:
					result.Passed++;
				break;

				case ResultStatus.Inconclusive:
					result.Inconclusive++;
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

			currentTest = FindTest (rootTest, testId);
			if (currentTest != null) {
				testContext.Monitor.BeginTest (currentTest);
				currentTest.Status = TestStatus.Running;
			}
		}

		UnitTest FindTest (UnitTest test, string testId)
		{
			var testGroup = test as UnitTestGroup;
			if (testGroup == null)
				return null;

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

			string testId = message.Test?.Id?.ToString ();
			if (testId != currentTest.TestId) {
				currentTest = FindTest (rootTest, testId);
			}

			if (currentTest != null) {
				currentTest.RegisterResult (testContext, result);
				testContext.Monitor.EndTest (currentTest, result);
				currentTest.Status = TestStatus.Ready;
			}
		}
	}
}

