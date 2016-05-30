// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class DiagnosticMessageView
    {
        public string ErrorCode { get; set; }

        public string SourceFilePath { get; set; }

        public string Message { get; set; }

        public DiagnosticMessageSeverity Severity { get; set; }

        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public string FormattedMessage { get; set; }

        public object Source { get; set; }
    }
}
