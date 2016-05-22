// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class DependenciesMessage
    {
        public FrameworkData Framework { get; set; }
        public string RootDependency { get; set; }
        public IDictionary<string, DependencyDescription> Dependencies { get; set; }
    }
}