// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class ProjectInformationMessage
    {
        public string Name { get; set; }

        public IList<FrameworkData> Frameworks { get; set; }

        public IList<string> Configurations { get; set; }

        public IDictionary<string, string> Commands { get; set; }

        public IList<string> ProjectSearchPaths { get; set; }

        public string GlobalJsonPath { get; set; }
    }
}