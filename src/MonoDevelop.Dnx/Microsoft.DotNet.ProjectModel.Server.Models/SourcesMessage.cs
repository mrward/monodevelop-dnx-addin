// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class SourcesMessage
    {
        public FrameworkData Framework { get; set; }
        public IList<string> Files { get; set; }
        public IDictionary<string, string> GeneratedFiles { get; set; }
    }
}
