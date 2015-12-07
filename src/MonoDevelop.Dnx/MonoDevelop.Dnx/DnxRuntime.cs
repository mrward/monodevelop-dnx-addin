//
// DnxRuntime.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2015 Matthew Ward
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Linq;
using MonoDevelop.Core;
using OmniSharp.Models;

namespace MonoDevelop.Dnx
{
	public class DnxRuntime
	{
		bool? usesCurrentDirectoryByDefault;

		public DnxRuntime (string path)
		{
			Path = new FilePath (path);
		}

		public FilePath Path { get; private set; }

		/// <summary>
		/// Returns true if the dnx runtime uses the current directory by default.
		/// </summary>
		public bool UsesCurrentDirectoryByDefault {
			get {
				if (!usesCurrentDirectoryByDefault.HasValue) {
					usesCurrentDirectoryByDefault = IsBeta7OrHigher (Path);
				}
				return usesCurrentDirectoryByDefault.Value;
			}
		}

		static string[] preBeta7Versions = new string[] {
			"beta4",
			"beta5",
			"beta6"
		};

		static bool IsBeta7OrHigher (FilePath path)
		{
			string runtimeName = path.FullPath.FileName;
			if (String.IsNullOrEmpty (runtimeName))
				return false;

			return !preBeta7Versions.Any (version => runtimeName.Contains (version));
		}

		public FilePath GetRuntimePath (DnxFramework framework)
		{
			string modifiedFileName = GetModifiedRuntimeFileName (Path.FileName, framework);
			return Path.ParentDirectory.Combine (modifiedFileName);
		}

		static string GetModifiedRuntimeFileName (string fileName, DnxFramework framework)
		{
			if (framework.IsCoreClr ()) {
				if (Platform.IsWindows) {
					return fileName.Replace ("-clr-", "-coreclr-");
				} else if (Platform.IsMac) {
					return fileName.Replace ("-mono", "-coreclr-darwin-x64");
				} else if (Platform.IsLinux) {
					return fileName.Replace ("-mono", "-coreclr-linux-x64");
				}
			}

			return fileName;
		}
	}
}

