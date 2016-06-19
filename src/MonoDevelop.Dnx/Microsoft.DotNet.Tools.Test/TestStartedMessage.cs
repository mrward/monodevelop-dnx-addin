// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Extensions.Testing.Abstractions
{
	public class TestStartedMessage
	{
		public string CodeFilePath { get; set; }
		public string DisplayName { get; set; }
		public string FullyQualifiedName { get; set; }
		public Guid? Id { get; set; }
	}
}