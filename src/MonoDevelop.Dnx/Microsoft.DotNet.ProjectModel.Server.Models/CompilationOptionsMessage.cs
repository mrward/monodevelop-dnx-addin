// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.ProjectModel.Server.Models
{
    public class CompilationOptionsMessage
    {
        public FrameworkData Framework { get; set; }

        public CommonCompilerOptions Options { get; set; }
    }
}