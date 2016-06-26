//
// DiagnosticsMessageExtensions.cs
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.DotNet.ProjectModel.Server.Models;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.Dnx
{
	public static class DiagnosticsMessageExtensions
	{
		delegate BuildResult AddError (string file, int line, int col, string errorNum, string text);

		public static BuildResult ToBuildResult (this DiagnosticsListMessage message, DnxProject project)
		{
			var result = new BuildResult ();
			AddErrors (result.AddWarning, message.Warnings, project);
			AddErrors (result.AddError, message.Errors, project);
			result.SourceTarget = project.Project;
			return result;
		}

		static void AddErrors (AddError addError, IEnumerable<DiagnosticMessageView> messages, DnxProject project)
		{
			foreach (DiagnosticMessageView message in messages) {
				BuildError error = CreateBuildError (message);
				error.SourceTarget = project.Project;
				addError (error.FileName, error.Line, error.Column, error.ErrorNumber, error.ErrorText);
			}
		}

		static BuildError CreateBuildError (DiagnosticMessageView message)
		{
			return new BuildError {
				FileName = message.SourceFilePath,
				Line =  message.StartLine,
				Column = message.StartColumn,
				ErrorNumber = message.ErrorCode,
				ErrorText = message.Message
			};
		}
	}
}

