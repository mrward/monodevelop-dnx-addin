// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class ReferencesMessage
    {
        public FrameworkData Framework { get; set; }
        public IList<string> FileReferences { get; set; }
        public IList<ProjectReferenceDescription> ProjectReferences { get; set; }
    }
}