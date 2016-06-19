// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Extensions.Testing.Abstractions
{
	public sealed class TestResultMessage
	{
		public Test Test { get; set; }

		public TestOutcome Outcome { get; set; }

		public string ErrorMessage { get; set; }

		public string ErrorStackTrace { get; set; }

		public string DisplayName { get; set; }

		public Collection<string> Messages { get; set; }

		public string ComputerName { get; set; }

		public TimeSpan Duration { get; set; }

		public DateTimeOffset StartTime { get; set; }

		public DateTimeOffset EndTime { get; set; }
	}
}