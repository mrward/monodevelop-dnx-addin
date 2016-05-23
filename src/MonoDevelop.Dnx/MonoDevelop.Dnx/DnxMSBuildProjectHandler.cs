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
using System.Reflection;
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Projects.Formats.MSBuild;

namespace MonoDevelop.Dnx
{
	public class DnxMSBuildProjectHandler : MSBuildProjectHandler
	{
		DnxProject dnxProject;
		MSBuildProject msbuildProject;

		public DnxMSBuildProjectHandler ()
		{
		}

		public DnxMSBuildProjectHandler (string typeGuid, string import, string itemId)
			: base (typeGuid, import, itemId)
		{
		}

		public static void InstallHandler (DnxProject project)
		{
			var itemType = new DnxMSBuildProjectItemType ();
			itemType.InitializeHandler (project);
		}

		protected override MSBuildProject SaveProject (IProgressMonitor monitor)
		{
			dnxProject = EntityItem as DnxProject;
			if (dnxProject == null || !dnxProject.IsDirty)
				return new MSBuildProject ();

			return CreateMSBuildProject ();
		}

		MSBuildProject CreateMSBuildProject ()
		{
			msbuildProject = new MSBuildProject ();
			msbuildProject.ToolsVersion = GetToolsVersion ();
			msbuildProject.DefaultTargets = "Build";

			string projectGuid = GetProjectGuid (dnxProject.ItemId);
			string rootNamespace = dnxProject.DefaultNamespace;

			AddVisualStudioProperties ();
			AddDotNetCoreProps ();
			AddGlobalsProperties (projectGuid, rootNamespace);
			AddSchemaVersion ();
			AddDotNetTargets ();

			return msbuildProject;
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
			XmlElement propertyGroup = AddPropertyGroup ();
			AddPropertyWithNotEmptyCondition (propertyGroup, "VisualStudioVersion", "14.0");
			AddPropertyWithNotEmptyCondition (propertyGroup, "VSToolsPath", @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)");
		}

		XmlElement AddPropertyGroup ()
		{
			XmlElement propertyGroup = msbuildProject.Document.CreateElement (null, "PropertyGroup", MSBuildProject.Schema);
			msbuildProject.Document.DocumentElement.AppendChild (propertyGroup);
			return propertyGroup;
		}

		void AddPropertyWithNotEmptyCondition (XmlElement propertyGroup, string name, string unevaluatedValue)
		{
			string condition = String.Format("'$({0})' == ''", name);
			XmlElement property = AddProperty (propertyGroup, name, unevaluatedValue);
			property.SetAttribute ("Condition", condition);
		}

		XmlElement AddProperty (XmlElement propertyGroup, string name, string unevaluatedValue)
		{
			XmlElement property = msbuildProject.Document.CreateElement (null, name, MSBuildProject.Schema);
			property.InnerText = unevaluatedValue;
			propertyGroup.AppendChild (property);
			return property;
		}

		void AddDotNetCoreProps ()
		{
			AddImport (@"$(VSToolsPath)\DotNet\Microsoft.DotNet.Props", "'$(VSToolsPath)' != ''");
		}

		void AddImport (string name, string condition)
		{
			XmlElement import = msbuildProject.Document.CreateElement (null, "Import", MSBuildProject.Schema);
			import.SetAttribute ("Project", name);
			import.SetAttribute ("Condition", condition);
			msbuildProject.Document.DocumentElement.AppendChild (import);
		}

		void AddSchemaVersion ()
		{
			XmlElement propertyGroup = AddPropertyGroup ();
			AddProperty (propertyGroup, "SchemaVersion", "2.0");
		}

		void AddDotNetTargets()
		{
			if (dnxProject.IsWebProject) {
				AddImport (@"$(VSToolsPath)\DotNet.Web\Microsoft.DotNet.Web.targets", "'$(VSToolsPath)' != ''");
			} else {
				AddImport (@"$(VSToolsPath)\DotNet\Microsoft.DotNet.targets", "'$(VSToolsPath)' != ''");
			}
		}

		void AddGlobalsProperties (string projectGuid, string rootNamespace)
		{
			XmlElement propertyGroup = AddPropertyGroup ();
			propertyGroup.SetAttribute ("Label", "Globals");

			AddProperty (propertyGroup, "ProjectGuid", projectGuid);
			AddProperty (propertyGroup, "RootNamespace", rootNamespace);
			AddPropertyWithNotEmptyCondition (propertyGroup, "BaseIntermediateOutputPath", @".\obj");
			AddPropertyWithNotEmptyCondition (propertyGroup, "OutputPath", @".\bin\");
			AddProperty (propertyGroup, "TargetFrameworkVersion", "v4.5");
		}

		static string GetProjectGuid (string projectGuid)
		{
			string guid = projectGuid.ToLowerInvariant ();
			return guid.Substring (1, guid.Length - 2);
		}
	}
}

