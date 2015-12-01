//
// DnxServices.cs
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
using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;

namespace MonoDevelop.Dnx
{
	public static class DnxServices
	{
		static readonly DnxProjectService projectService = new DnxProjectService ();

		public static DnxProjectService ProjectService {
			get { return projectService; }
		}

		public static void Initialize ()
		{
			ProjectService.Initialize ();
			IsPortablePdbSupported = GetPortablePdbSupported ();
		}

		public static bool IsPortablePdbSupported { get; private set; }

		static bool GetPortablePdbSupported ()
		{
			if (Platform.IsWindows) {
				return true;
			}

			return IsAtLeastMono43 ();
		}

		static bool IsAtLeastMono43 ()
		{
			MonoRuntimeInfo monoInfo = MonoRuntimeInfo.FromCurrentRuntime ();
			if (monoInfo == null)
				return false;

			Version version;
			if (!Version.TryParse (monoInfo.MonoVersion, out version))
				return false;

			var version43 = new Version ("4.3.0");
			return version >= version43;
		}
	}
}

