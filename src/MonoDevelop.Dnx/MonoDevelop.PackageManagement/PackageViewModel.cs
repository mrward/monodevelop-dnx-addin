// 
// PackageViewModel.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2013 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Versioning;
using MonoDevelop.PackageManagement;
using NuGet;

namespace ICSharpCode.PackageManagement
{
	public class PackageViewModel : ViewModelBase<PackageViewModel>
	{
		IPackageFromRepository package;
		IPackageViewModelParent parent;
		string summary;
		List<PackageDependency> dependencies;
		bool isChecked;

		public PackageViewModel(
			IPackageViewModelParent parent,
			IPackageFromRepository package)
		{
			this.parent = parent;
			this.package = package;
		}
		
		public IPackageViewModelParent GetParent()
		{
			return parent;
		}

		public IPackage GetPackage()
		{
			return package;
		}
		
		public bool HasLicenseUrl {
			get { return LicenseUrl != null; }
		}
		
		public Uri LicenseUrl {
			get { return package.LicenseUrl; }
		}
		
		public bool HasProjectUrl {
			get { return ProjectUrl != null; }
		}
		
		public Uri ProjectUrl {
			get { return package.ProjectUrl; }
		}
		
		public bool HasReportAbuseUrl {
			get { return ReportAbuseUrl != null; }
		}
		
		public Uri ReportAbuseUrl {
			get { return package.ReportAbuseUrl; }
		}
		
		public IEnumerable<PackageDependency> Dependencies {
			get {
				if (dependencies == null) {
					FrameworkName targetFramework = null; // = selectedProjects.GetTargetFramework ();
					dependencies = package
						.GetCompatiblePackageDependencies (targetFramework)
						.ToList ();
				}
				return dependencies;
			}
		}
		
		public bool HasDependencies {
			get { return Dependencies.Any (); }
		}
		
		public bool HasNoDependencies {
			get { return !HasDependencies; }
		}
		
		public IEnumerable<string> Authors {
			get { return package.Authors; }
		}
		
		public bool HasDownloadCount {
			get { return package.DownloadCount >= 0; }
		}
		
		public string Id {
			get { return package.Id; }
		}
		
		public string Name {
			get { return package.GetName(); }
		}
		
		public bool HasGalleryUrl {
			get { return GalleryUrl != null; }
		}
		
		public bool HasNoGalleryUrl {
			get { return !HasGalleryUrl; }
		}
		
		public Uri GalleryUrl {
			get { return package.GalleryUrl; }
		}
		
		public Uri IconUrl {
			get { return package.IconUrl; }
		}

		public bool HasIconUrl {
			get { return IconUrl != null; }
		}

		public string Summary {
			get {
				if (summary == null) {
					summary = StripNewLinesAndIndentation (package.SummaryOrDescription ());
				}
				return summary;
			}
		}

		string StripNewLinesAndIndentation (string text)
		{
			return PackageListViewTextFormatter.Format (text);
		}
		
		public SemanticVersion Version {
			get { return package.Version; }
		}
		
		public int DownloadCount {
			get { return package.DownloadCount; }
		}
		
		public string Description {
			get { return package.Description; }
		}
		
		public DateTimeOffset? LastPublished {
			get { return package.Published; }
		}
		
		public bool HasLastPublished {
			get { return package.Published.HasValue; }
		}
		
		static readonly string DisplayTextMarkupFormat = "<span weight='bold'>{0}</span>\n{1}";
		
		public string GetDisplayTextMarkup ()
		{
			return MarkupString.Format (DisplayTextMarkupFormat, Name, Summary);
		}
		
		public string GetAuthors()
		{
			return String.Join (", ", Authors);
		}
		
		public string GetLastPublishedDisplayText()
		{
			if (HasLastPublished) {
				return LastPublished.Value.Date.ToShortDateString ();
			}
			return String.Empty;
		}
		
		public string GetDownloadCountOrVersionDisplayText ()
		{
			if (ShowVersionInsteadOfDownloadCount) {
				return Version.ToString ();
			}

			return GetDownloadCountDisplayText ();
		}

		public string GetDownloadCountDisplayText ()
		{
			if (HasDownloadCount) {
				return DownloadCount.ToString ("N0");
			}
			return String.Empty;
		}

		public string GetPackageDependenciesDisplayText ()
		{
			var displayText = new StringBuilder ();
			foreach (PackageDependency dependency in Dependencies) {
				displayText.AppendLine (dependency.ToString ());
			}
			return displayText.ToString ();
		}

		public string GetNameMarkup ()
		{
			return GetBoldText (Name);
		}
			
		static string GetBoldText (string text)
		{
			return String.Format ("<b>{0}</b>", text);
		}

		public bool IsOlderPackageInstalled ()
		{
			return false;
//			return selectedProjects.HasOlderPackageInstalled (package);
		}

		public bool IsChecked {
			get { return isChecked; }
			set {
				if (value != isChecked) {
					isChecked = value;
					parent.OnPackageCheckedChanged (this);
				}
			}
		}

		public bool ShowVersionInsteadOfDownloadCount { get; set; }

		public override bool Equals (object obj)
		{
			var other = obj as PackageViewModel;
			if (other == null)
				return false;

			var packageName = new PackageName (package.Id, package.Version);
			var otherPackageName = new PackageName (other.package.Id, other.package.Version);
			return packageName.Equals (otherPackageName);
		}

		public override int GetHashCode ()
		{
			return package.ToString ().GetHashCode ();
		}
	}
}
