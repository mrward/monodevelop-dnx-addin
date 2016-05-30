// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class ProjectReferenceDescription
    {
        private ProjectReferenceDescription() { }

        public FrameworkData Framework { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string WrappedProjectPath { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as ProjectReferenceDescription;
            return other != null &&
                   string.Equals(Name, other.Name) &&
                   string.Equals(Path, other.Path) &&
                   string.Equals(WrappedProjectPath, other.WrappedProjectPath);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
