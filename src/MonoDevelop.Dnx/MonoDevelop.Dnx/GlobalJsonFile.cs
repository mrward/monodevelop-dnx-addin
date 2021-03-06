﻿//
// GlobalJsonFile.cs
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Dnx
{
	public class GlobalJsonFile : JsonFile
	{
		JObject sdkObject;
		List<string> projectDirectories = new List<string> ();

		GlobalJsonFile (FilePath filePath)
			: base (filePath)
		{
		}

		public static GlobalJsonFile Read (DnxProject project)
		{
			return Read (project.ParentSolution);
		}

		public static GlobalJsonFile Read (Solution solution)
		{
			var jsonFile = new GlobalJsonFile (solution.BaseDirectory.Combine ("global.json"));
			jsonFile.Read ();
			return jsonFile;
		}

		public string DnxRuntimeVersion { get; set; }

		protected override void AfterRead ()
		{
			ReadSdkInformation ();
			ReadProjectDirectories ();
		}

		void ReadSdkInformation ()
		{
			JToken sdkToken;
			if (!jsonObject.TryGetValue ("sdk", out sdkToken))
				return;

			sdkObject = sdkToken as JObject;
			if (sdkObject == null)
				return;

			JToken version;
			if (!sdkObject.TryGetValue ("version", out version))
				return;

			DnxRuntimeVersion = version.ToString ();
		}

		protected override void BeforeSave ()
		{
			sdkObject["version"] = new JValue (DnxRuntimeVersion);
		}

		void ReadProjectDirectories ()
		{
			JToken projectsToken;
			if (!jsonObject.TryGetValue ("projects", out projectsToken)) {
				if (!jsonObject.TryGetValue ("sources", out projectsToken)) {
					return;
				}
			}

			var projectsArray = projectsToken as JArray;
			if (projectsArray == null)
				return;

			projectDirectories = projectsArray.Select (directory => directory.ToString ())
				.ToList ();
		}

		public IEnumerable<string> ProjectDirectories {
			get { return projectDirectories; }
		}
	}
}

