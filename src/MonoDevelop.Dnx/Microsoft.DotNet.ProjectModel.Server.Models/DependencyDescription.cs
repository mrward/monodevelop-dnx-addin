// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class DependencyDescription
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Version { get; set; }

        public string Path { get; set; }

        public string Type { get; set; }

        public bool Resolved { get; set; }

        public IEnumerable<DependencyItem> Dependencies { get; set; }

        public IEnumerable<DiagnosticMessageView> Errors { get; set; }

        public IEnumerable<DiagnosticMessageView> Warnings { get; set; }
    }
}
