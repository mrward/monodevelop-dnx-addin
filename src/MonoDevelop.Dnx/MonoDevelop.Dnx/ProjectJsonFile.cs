//
// ProjectJsonFile.cs
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
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Dnx
{
	public class ProjectJsonFile : JsonFile
	{
		JObject dependencies;
		JObject frameworks;

		ProjectJsonFile (FilePath filePath)
			: base (filePath)
		{
		}

		public static ProjectJsonFile Read (DnxProject project)
		{
			var jsonFile = new ProjectJsonFile (project.BaseDirectory.Combine ("project.json"));
			jsonFile.Read ();
			return jsonFile;
		}

		public string Version { get; set; }

		protected override void AfterRead ()
		{
			ReadPropertiesFromJsonObject ();
		}

		void ReadPropertiesFromJsonObject ()
		{
			JToken version;
			if (!jsonObject.TryGetValue ("version", out version))
				return;

			Version = version.ToString ();
		}

		public void AddProjectReference (ProjectReference projectReference)
		{
			JObject dependencies = GetOrCreateDependencies ();
			var projectDependency = new JProperty (projectReference.Reference, "1.0.0-*");
			InsertSorted (dependencies, projectDependency);
		}

		public void RemoveProjectReference (ProjectReference projectReference)
		{
			JObject dependencies = GetDependencies ();
			if (dependencies == null) {
				LoggingService.LogDebug ("Unable to find dependencies in project.json");
				return;
			}

			dependencies.Remove (projectReference.Reference);
		}

		JObject GetDependencies ()
		{
			if (dependencies != null)
				return dependencies;

			JToken token;
			if (jsonObject.TryGetValue ("dependencies", out token)) {
				dependencies = token as JObject;
			}

			return dependencies;
		}

		JObject GetOrCreateDependencies ()
		{
			if (GetDependencies () != null)
				return dependencies;

			dependencies = new JObject ();
			jsonObject.Add ("dependencies", dependencies);

			return dependencies;
		}

		public void AddNuGetPackages (IEnumerable<NuGetPackageToAdd> packagesToAdd)
		{
			JObject dependencies = GetOrCreateDependencies ();
			foreach (NuGetPackageToAdd package in packagesToAdd) {
				var packageDependency = new JProperty (package.Id, package.Version);
				InsertSorted (dependencies, packageDependency);
			}
		}

		public void RemoveNuGetPackage (string frameworkShortName, string packageId)
		{
			JObject dependencies = GetDependencies ();
			if (dependencies == null) {
				LoggingService.LogDebug ("Unable to find dependencies in project.json");
				return;
			}

			if (dependencies.Remove (packageId))
				return;

			if (string.IsNullOrEmpty (frameworkShortName)) {
				LoggingService.LogDebug ("Unable to find null framework in project.json");
				return;
			}

			JObject frameworkDependencies = GetFrameworkDependencies (frameworkShortName);
			if (frameworkDependencies == null) {
				LoggingService.LogDebug ("Unable to find dependencies for framework '{0}' in project.json", frameworkShortName);
				return;
			}

			frameworkDependencies.Remove (packageId);
		}

		JObject GetFrameworkDependencies (string name)
		{
			JObject frameworks = GetFrameworks ();
			if (frameworks == null) {
				LoggingService.LogDebug ("Unable to find frameworks in project.json");
				return null;
			}

			JObject framework = null;
			JToken token;
			if (frameworks.TryGetValue (name, out token)) {
				framework = token as JObject;
			} else {
				LoggingService.LogDebug ("Unable to find framework '{0}' in project.json", name);
				return null;
			}

			JObject frameworkDependencies = null;
			if (framework.TryGetValue ("dependencies", out token)) {
				frameworkDependencies = token as JObject;
			}

			return frameworkDependencies;
		}

		JObject GetFrameworks ()
		{
			if (frameworks != null)
				return frameworks;

			JToken token;
			if (jsonObject.TryGetValue ("frameworks", out token)) {
				frameworks = token as JObject;
			}

			return frameworks;
		}

		public bool HasTestRunner ()
		{
			JToken testRunner;
			if (!jsonObject.TryGetValue ("testRunner", out testRunner))
				return false;

			return !String.IsNullOrEmpty (testRunner.ToString ());
		}
	}
}

