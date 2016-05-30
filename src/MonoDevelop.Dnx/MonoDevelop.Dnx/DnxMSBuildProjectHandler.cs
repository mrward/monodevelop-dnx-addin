//
// DnxMSBuildProjectHandler.cs
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
using System.Reflection;
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Projects.MSBuild;

namespace MonoDevelop.Dnx
{
	public class DnxMSBuildProjectHandler
	{
		DnxProject dnxProject;
		MSBuildProject msbuildProject;

		public DnxMSBuildProjectHandler (DnxProject project)
		{
			dnxProject = project;
		}

		DnxProject EntityItem {
			get { return dnxProject; }
		}

		public void SaveProject (ProgressMonitor monitor, MSBuildProject project)
		{
			dnxProject = EntityItem as DnxProject;
			if (dnxProject == null || !dnxProject.IsDirty)
				return;

			msbuildProject = project;

			CreateMSBuildProject ();
		}

		void CreateMSBuildProject ()
		{
			msbuildProject.ToolsVersion = GetToolsVersion ();
			msbuildProject.DefaultTargets = "Build";

			MSBuildPropertyGroup propertyGroup = msbuildProject.PropertyGroups.FirstOrDefault ();
			if (propertyGroup != null)
				msbuildProject.Remove (propertyGroup);

			string projectGuid = GetProjectGuid (dnxProject.ItemId);
			string rootNamespace = dnxProject.DefaultNamespace;

			AddVisualStudioProperties ();
			MSBuildPropertyGroup globals = AddGlobalsProperties (projectGuid, rootNamespace);
			AddSchemaVersion ();
			AddDotNetTargets ();
			AddDotNetCoreProps (globals);
		}

		static string GetToolsVersion ()
		{
			if (SupportsMSBuild14 ())
				return "14.0";

			return "4.0";
		}

		static bool SupportsMSBuild14 ()
		{
			Version ideVersion = null;
			if (Version.TryParse (GetBuildInfoVersion (), out ideVersion)) {
				var version510 = new Version (5, 10);
				return ideVersion >= version510;
			}
			return false;
		}

		static string GetBuildInfoVersion ()
		{
			Type type = typeof (BuildInfo);
			FieldInfo field = type.GetField ("Version");
			if (field != null)
				return (string)field.GetValue (null);

			return BuildInfo.Version;
		}

		void AddVisualStudioProperties()
		{
			MSBuildPropertyGroup propertyGroup = AddPropertyGroup ();
			AddPropertyWithNotEmptyCondition (propertyGroup, "VisualStudioVersion", "14.0");
			AddPropertyWithNotEmptyCondition (propertyGroup, "VSToolsPath", @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)");
		}

		MSBuildPropertyGroup AddPropertyGroup ()
		{
			return msbuildProject.AddNewPropertyGroup (true);
		}

		void AddPropertyWithNotEmptyCondition (MSBuildPropertyGroup propertyGroup, string name, string unevaluatedValue)
		{
			string condition = String.Format("'$({0})' == ''", name);
			propertyGroup.SetValue (name, unevaluatedValue, condition: condition);
		}

		MSBuildProperty AddProperty (MSBuildPropertyGroup propertyGroup, string name, string unevaluatedValue)
		{
			propertyGroup.SetValue (name, unevaluatedValue);
			return propertyGroup.GetProperty (name);
		}

		void AddDotNetCoreProps (MSBuildPropertyGroup globals)
		{
			AddImport (@"$(VSToolsPath)\DotNet\Microsoft.DotNet.Props", "'$(VSToolsPath)' != ''", globals);
		}

		void AddImport (string name, string condition, MSBuildObject before = null)
		{
			msbuildProject.AddNewImport (name, condition, before);
		}

		void AddSchemaVersion ()
		{
			MSBuildPropertyGroup propertyGroup = AddPropertyGroup ();
			AddProperty (propertyGroup, "SchemaVersion", "2.0");
		}

		void AddDotNetTargets ()
		{
			if (dnxProject.IsWebProject) {
				AddImport (@"$(VSToolsPath)\DotNet.Web\Microsoft.DotNet.Web.targets", "'$(VSToolsPath)' != ''");
			} else {
				AddImport (@"$(VSToolsPath)\DotNet\Microsoft.DotNet.targets", "'$(VSToolsPath)' != ''");
			}
		}

		MSBuildPropertyGroup AddGlobalsProperties (string projectGuid, string rootNamespace)
		{
			MSBuildPropertyGroup propertyGroup = AddPropertyGroup ();
			propertyGroup.Label = "Globals";

			AddProperty (propertyGroup, "ProjectGuid", projectGuid);
			AddProperty (propertyGroup, "RootNamespace", rootNamespace);
			AddPropertyWithNotEmptyCondition (propertyGroup, "BaseIntermediateOutputPath", @".\obj");
			AddPropertyWithNotEmptyCondition (propertyGroup, "OutputPath", @".\bin\");
			AddProperty (propertyGroup, "TargetFrameworkVersion", "v4.5");

			return propertyGroup;
		}

		static string GetProjectGuid (string projectGuid)
		{
			string guid = projectGuid.ToLowerInvariant ();
			return guid.Substring (1, guid.Length - 2);
		}
	}
}

