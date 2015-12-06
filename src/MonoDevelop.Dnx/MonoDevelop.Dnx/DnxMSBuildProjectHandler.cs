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
using MonoDevelop.Core;
using MonoDevelop.Projects.Formats.MSBuild;
using System.Xml;

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
			AddDnxProps ();
			AddGlobalsProperties (projectGuid, rootNamespace);
			AddSchemaVersion ();
			AddDnxTargets ();

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
			if (Version.TryParse (BuildInfo.Version, out ideVersion)) {
				var version510 = new Version (5, 10);
				return ideVersion >= version510;
			}
			return false;
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

		void AddDnxProps ()
		{
			AddImport (@"$(VSToolsPath)\DNX\Microsoft.DNX.Props", "'$(VSToolsPath)' != ''");
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

		void AddDnxTargets()
		{
			AddImport (@"$(VSToolsPath)\DNX\Microsoft.DNX.targets", "'$(VSToolsPath)' != ''");
		}

		void AddGlobalsProperties (string projectGuid, string rootNamespace)
		{
			XmlElement propertyGroup = AddPropertyGroup ();
			propertyGroup.SetAttribute ("Label", "Globals");

			AddProperty (propertyGroup, "ProjectGuid", projectGuid);
			AddProperty (propertyGroup, "RootNamespace", rootNamespace);
			AddPropertyWithNotEmptyCondition (propertyGroup, "BaseIntermediateOutputPath", @"..\..\artifacts\obj\$(MSBuildProjectName)");
			AddPropertyWithNotEmptyCondition (propertyGroup, "OutputPath", @"..\..\artifacts\bin\$(MSBuildProjectName)\");
		}

		static string GetProjectGuid (string projectGuid)
		{
			string guid = projectGuid.ToLowerInvariant ();
			return guid.Substring (1, guid.Length - 2);
		}
	}
}

