//
// DnxProject.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.DotNet.ProjectModel.Server.Models;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Projects;
using MonoDevelop.Projects.MSBuild;
using OmniSharp.Models;

namespace MonoDevelop.Dnx
{
	public class DnxProject : DotNetProjectExtension
	{
		OmniSharp.Models.DnxProject project;
		bool addingReferences;
		bool loadingFiles;
		Dictionary<string, DependenciesMessage> dependencies = new Dictionary<string, DependenciesMessage> ();
		Dictionary<string, List<string>> savedFileReferences = new Dictionary<string, List<string>> ();
		Dictionary<string, List<string>> savedProjectReferences = new Dictionary<string, List<string>> ();

		Dictionary<string, CSharpCompilationAndParseOptions> savedCompilationOptions = new Dictionary<string, CSharpCompilationAndParseOptions> ();

		public static readonly string ProjectTypeGuid = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";

		public DnxProject ()
		{
		}

		public ProjectItemCollection Items {
			get { return Project.Items; }
		}

		public ProjectReferenceCollection References {
			get { return Project.References; }
		}

		public SolutionItemConfigurationCollection Configurations {
			get { return Project.Configurations; }
		}

		public FilePath BaseDirectory {
			get { return Project.BaseDirectory; }
		}

		public Solution ParentSolution {
			get { return Project.ParentSolution; }
		}

		public string Name {
			get { return Project.Name; }
		}

		public FilePath FileName {
			get { return Project.FileName; }
			set { Project.FileName = value; }
		}

		public string ItemId {
			get { return Project.ItemId; }
		}

		public string DefaultNamespace {
			get { return Project.DefaultNamespace; }
		}

		public SolutionFolder ParentFolder {
			get { return Project.ParentFolder; }
		}

		public Projects.MSBuild.MSBuildProject MSBuildProject {
			get { return Project.MSBuildProject; }
		}

		protected override void OnEndLoad ()
		{
			try {
				loadingFiles = true;
				LoadFiles ();
			} finally {
				loadingFiles = false;
			}
			base.OnEndLoad ();
		}

		void LoadFiles ()
		{
			// Add directories first, to make sure to show empty ones.
			var excludedDirectories = new List<FilePath> ();
			foreach (string directoryName in Directory.GetDirectories (BaseDirectory, "*.*", SearchOption.AllDirectories)) {
				if (!IsExcludedDirectory (directoryName, excludedDirectories)) {
					Items.Add (CreateDirectoryProjectItem (directoryName));
				}
			}

			foreach (string fileName in Directory.GetFiles (BaseDirectory, "*.*", SearchOption.AllDirectories)) {
				if (IsSupportedProjectFileItem (fileName) &&
					!IsPathExcluded (fileName, excludedDirectories)) {
					ProjectFile projectFile = CreateFileProjectItem (fileName);
					Items.Add (projectFile);
					AddProjectFileToMSBuildProject (projectFile);
				}
			}

			AddConfigurations ();
		}

		static string[] excludedDirectoryNames = new string[] { "bin", "obj" };

		bool IsExcludedDirectory (string directory, List<FilePath> excludedDirectories)
		{
			var info = new DirectoryInfo (directory);
			bool excluded = excludedDirectoryNames.Any (name => name.Equals (info.Name, StringComparison.OrdinalIgnoreCase));
			if (!excluded) {
				excluded = IsPathExcluded (directory, excludedDirectories);
			}

			if (excluded) {
				excludedDirectories.Add (new FilePath (directory));
			}

			return excluded;
		}

		static bool IsPathExcluded (string path, List<FilePath> excludedDirectories)
		{
			var filePath = new FilePath (path);
			return excludedDirectories.Any (filePath.IsChildPathOf);
		}

		void AddProjectFileToMSBuildProject (ProjectFile projectFile)
		{
			MSBuildProject.AddNewItem (projectFile.ItemName, projectFile.FilePath);
		}

		void RemoveProjectFileFromMSBuildProject (ProjectFile projectFile)
		{
			MSBuildItem matchedItem = FindMSBuildItem (projectFile);
			if (matchedItem != null) {
				MSBuildProject.RemoveItem (matchedItem);
			} else {
				LoggingService.LogWarning ("Unable to remove project file from MSBuildProject. '{0}'", projectFile.FilePath);
			}
		}

		MSBuildItem FindMSBuildItem (ProjectFile projectFile)
		{
			return MSBuildProject
				.GetAllItems ()
				.ToArray ()
				.FirstOrDefault (item => item.Include == projectFile.FilePath);
		}

		void RenameProjectFileInMSBuildProject (ProjectFile projectFile, FilePath oldFileName)
		{
			var oldProjectFile = new ProjectFile (oldFileName);
			MSBuildItem matchedItem = FindMSBuildItem (oldProjectFile);
			if (matchedItem != null) {
				MSBuildProject.RemoveItem (matchedItem);
				AddProjectFileToMSBuildProject (projectFile);
			} else {
				LoggingService.LogWarning ("Unable to rename project file from MSBuildProject. '{0}'", oldFileName);
			}
		}

		protected override void OnFileAddedToProject (ProjectFileEventArgs e)
		{
			base.OnFileAddedToProject (e);
			if (loadingFiles)
				return;

			foreach (ProjectFileEventInfo info in e) {
				AddProjectFileToMSBuildProject (info.ProjectFile);
			}
		}

		protected override void OnFileRemovedFromProject (ProjectFileEventArgs e)
		{
			base.OnFileRemovedFromProject (e);
			if (loadingFiles)
				return;

			foreach (ProjectFileEventInfo info in e) {
				RemoveProjectFileFromMSBuildProject (info.ProjectFile);
			}
		}

		protected override void OnFileRenamedInProject (ProjectFileRenamedEventArgs e)
		{
			base.OnFileRenamedInProject (e);
			if (loadingFiles)
				return;

			foreach (ProjectFileRenamedEventInfo info in e) {
				RenameProjectFileInMSBuildProject (info.ProjectFile, info.OldName);
			}
		}

		static bool IsSupportedProjectFileItem (string fileName)
		{
			string extension = Path.GetExtension (fileName);
			if (extension.EndsWith ("proj", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals (".sln", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals (".user", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (IsBackupFile (fileName)) {
				return false;
			}
			return true;
		}

		static bool IsBackupFile (string fileName)
		{
			return fileName.EndsWith ("~", StringComparison.Ordinal);
		}

		ProjectFile CreateDirectoryProjectItem (string directory)
		{
			return new ProjectFile (directory, BuildAction.None) {
				Subtype = Subtype.Directory
			};
		}

		ProjectFile CreateFileProjectItem (string fileName)
		{
			var projectFile = new ProjectFile (fileName, OnGetDefaultBuildAction (fileName));

			if (IsProjectJsonLockFile (fileName)) {
				AddProjectJsonDependency (projectFile);
			}

			return projectFile;
		}

		protected override string OnGetDefaultBuildAction (string fileName)
		{
			if (IsCSharpFile (fileName)) {
				return BuildAction.Compile;
			}
			return base.OnGetDefaultBuildAction (fileName);
		}

		static bool IsCSharpFile (string fileName)
		{
			string extension = Path.GetExtension (fileName);
			return String.Equals (".cs", extension, StringComparison.OrdinalIgnoreCase);
		}

		void AddAssemblyReference (string fileName)
		{
			var projectItem = ProjectReference.CreateCustomReference (ReferenceType.Assembly, fileName);
			References.Add (projectItem);
		}

		void AddProjectReference (string fileName)
		{
			DnxProject project = ParentSolution.FindProjectByProjectJsonFileName (fileName);
			if (project != null) {
				var projectItem = ProjectReference.CreateCustomReference (ReferenceType.Project, project.Name);
				References.Add (projectItem);
			} else {
				LoggingService.LogDebug ("Unable to find project by json filename '{0}'.", fileName);
			}
		}

		public string CurrentFramework { get; private set; }

		public void UpdateReferences (OmniSharp.Dnx.FrameworkProject frameworkProject)
		{
			EnsureCurrentFrameworkDefined (frameworkProject);

			List<string> fileReferences = frameworkProject.FileReferences.Keys.ToList ();
			savedFileReferences[frameworkProject.Framework] = fileReferences;

			List<string> projectReferences = frameworkProject.ProjectReferences.Keys.ToList ();
			savedProjectReferences[frameworkProject.Framework] = projectReferences;

			if (CurrentFramework != frameworkProject.Framework)
				return;

			try {
				addingReferences = true;
				RemoveExistingReferences ();
				UpdateReferences (fileReferences);
				UpdateProjectReferences (projectReferences);
			} finally {
				addingReferences = false;
			}
		}

		void EnsureCurrentFrameworkDefined (OmniSharp.Dnx.FrameworkProject frameworkProject)
		{
			if (CurrentFramework == null) {
				CurrentFramework = frameworkProject.Project.ProjectsByFramework.Keys.FirstOrDefault ();
			}
		}

		void RemoveExistingReferences ()
		{
			var oldReferenceItems = Items.OfType<ProjectReference> ().ToList ();
			Items.RemoveRange (oldReferenceItems);
		}

		void UpdateReferences (IEnumerable<string> references)
		{
			foreach (string reference in references) {
				AddAssemblyReference (reference);
			}
		}

		void UpdateProjectReferences (IEnumerable<string> references)
		{
			foreach (string reference in references) {
				AddProjectReference (reference);
			}
		}

		public void Update (OmniSharp.Models.DnxProject project)
		{
			this.project = project;
			UpdateCachedProjectInformation ();
			base.OnExecutionTargetsChanged ();
		}

		protected override void OnInitializeFromTemplate (ProjectCreateInformation projectCreateInfo, XmlElement template)
		{
			base.OnInitializeFromTemplate (projectCreateInfo, template);
			AddConfigurations ();
		}

		internal void AddConfigurations ()
		{
			AddConfiguration ("Debug");
			AddConfiguration ("Release");
		}

		void AddConfiguration (string name)
		{
			var configuration = new DnxProjectConfiguration (name);
			Configurations.Add (configuration);

			EnsureConfigurationHasProjectInstance (configuration);
		}

		/// <summary>
		/// HACK. The Configuration needs to have the ProjectInstance set otherwise the
		/// Project's OnDispose method throws a NullReferenceException if a new DNX project
		/// is created but not when it is loaded from disk. The DNX addin is 
		/// bypassing the normal load and save methods used by the Project class so
		/// the ProjectInstance is never set. Another problem is the ProjectInstance
		/// is internal so the DNX addin has to resort to reflection to set this.
		/// </summary>
		void EnsureConfigurationHasProjectInstance (DnxProjectConfiguration configuration)
		{
			Type type = configuration.GetType ();
			PropertyInfo property = type.GetProperty ("ProjectInstance", BindingFlags.Instance | BindingFlags.NonPublic);
			if (property != null) {
				object instance = property.GetValue (configuration);
				if (instance == null) {
					property.SetValue (configuration, MSBuildProject.CreateInstance ());
				}
			}
		}

		protected override ExecutionCommand OnCreateExecutionCommand (ConfigurationSelector configSel, DotNetProjectConfiguration configuration)
		{
			return CreateDotNetCoreExecutionCommand (configSel, configuration);
		}

		DotNetCoreExecutionCommand CreateDotNetCoreExecutionCommand (ConfigurationSelector configSel, DotNetProjectConfiguration configuration)
		{
			return new DotNetCoreExecutionCommand (
				BaseDirectory,
				configuration.Name,
				DnxServices.ProjectService.CurrentDotNetRuntimePath
			);
		}

		protected override bool OnGetSupportsTarget (string target)
		{
			return true;
		}

		protected override bool OnGetCanExecute (ExecutionContext context, ConfigurationSelector configuration)
		{
			return true;
		}

		protected async override Task<TargetEvaluationResult> OnRunTarget (ProgressMonitor monitor, string target, ConfigurationSelector configuration, TargetEvaluationContext context)
		{
			if (target == ProjectService.BuildTarget) {
				var config = GetConfiguration (configuration) as DotNetProjectConfiguration;
				using (var builder = new DnxProjectBuilder (this, monitor)) {
					BuildResult result = await builder.BuildAsnc (config);
					return new TargetEvaluationResult (result);
				}
			}
			return new TargetEvaluationResult (BuildResult.CreateSuccess ());
		}
		
		SolutionItemConfiguration GetConfiguration (ConfigurationSelector configuration)
		{
			return Project.GetConfiguration (configuration);
		}
		
		protected async override Task OnExecute (ProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configuration)
		{
			var config = GetConfiguration (configuration) as DotNetProjectConfiguration;

			DotNetCoreExecutionCommand executionCommand = CreateDotNetCoreExecutionCommand (configuration, config);
			if (context.ExecutionTarget != null)
				executionCommand.Target = context.ExecutionTarget;

			executionCommand.Initialize ();

			if (executionCommand.IsExecutable) {
				await base.OnExecute (monitor, context, configuration);
				return;
			}

			monitor.Log.WriteLine (GettextCatalog.GetString ("Running {0} ...", Name));

			OperationConsole console = CreateConsole (config, context, monitor);

			try {
				try {
					ProcessAsyncOperation asyncOp = Execute (executionCommand, console);
					await asyncOp.Task;

					monitor.Log.WriteLine (GettextCatalog.GetString ("The application exited with code: {0}", asyncOp.ExitCode));
				} finally {
					console.Dispose ();
				}
			} catch (Exception ex) {
				LoggingService.LogError (string.Format ("Cannot execute \"{0}\"", Name), ex);
				monitor.ReportError (GettextCatalog.GetString ("Cannot execute \"{0}\"", Name), ex);
			}
		}

		OperationConsole CreateConsole (DotNetProjectConfiguration config, ExecutionContext context, ProgressMonitor monitor)
		{
			if (config.ExternalConsole)
				return context.ExternalConsoleFactory.CreateConsole (!config.PauseConsoleOutput, monitor.CancellationToken);
			return context.ConsoleFactory.CreateConsole (monitor.CancellationToken);
		}

		ProcessAsyncOperation Execute (ExecutionCommand command, OperationConsole console)
		{
			var dotNetCoreCommand = (DotNetCoreExecutionCommand)command;
			return Runtime.ProcessService.StartConsoleProcess (
				dotNetCoreCommand.GetCommand (),
				dotNetCoreCommand.GetArguments (),
				dotNetCoreCommand.WorkingDirectory,
				console,
				null);
		}

		protected override FilePath OnGetOutputFileName (ConfigurationSelector configuration)
		{
			return null;
		}

		public void UpdateDependencies (DependenciesMessage message)
		{
			dependencies[message.Framework.FrameworkName] = message;
			OnDependenciesChanged ();
		}

		public event EventHandler DependenciesChanged;

		protected virtual void OnDependenciesChanged ()
		{
			var handler = DependenciesChanged;
			if (handler != null)
				handler (this, new EventArgs ());
		}

		public bool HasDependencies ()
		{
			return dependencies.Any ();
		}

		public IEnumerable<DependenciesMessage> GetDependencies ()
		{
			foreach (DependenciesMessage message in dependencies.Values) {
				yield return message;
			}
		}

		public bool IsRestoringPackages { get; set; }

		public event EventHandler PackageRestoreStarted;

		public void OnPackageRestoreStarted ()
		{
			IsRestoringPackages = true;

			var handler = PackageRestoreStarted;
			if (handler != null)
				handler (this, new EventArgs ());
		}

		public event EventHandler PackageRestoreFinished;

		public void OnPackageRestoreFinished ()
		{
			IsRestoringPackages = false;

			var handler = PackageRestoreFinished;
			if (handler != null)
				handler (this, new EventArgs ());
		}

		internal bool IsDirty { get; set; }

		protected override Task OnSave (ProgressMonitor monitor)
		{
			if (IsDirty) {
				var xproject = (XProject)Project;
				xproject.SetDefaultNamespace (FileName);
				return Save (monitor).ContinueWith (task => xproject.NeedsReload = false);
			}
			IsDirty = false;
			return Task.FromResult (0);
		}

		Task Save (ProgressMonitor monitor)
		{
			if (loadingFiles)
				return Task.FromResult (0);

			var msproject = new Projects.MSBuild.MSBuildProject ();
			var projectBuilder = new DnxMSBuildProjectHandler (this);
			projectBuilder.SaveProject (monitor, msproject);
			return msproject.SaveAsync (FileName);
		}

		public void GenerateNewProjectFileName (Solution solution, string projectName)
		{
			FileName = solution.BaseDirectory
				.Combine ("src", projectName, projectName + ".xproj");
		}

		public void CreateProjectDirectory ()
		{
			CreateDirectory (Project.BaseDirectory);
		}

		static void CreateDirectory (FilePath directory)
		{
			if (!Directory.Exists (directory)) {
				Directory.CreateDirectory (directory);
			}
		}

		public SolutionFolder GetSrcSolutionFolder ()
		{
			return Project.ParentSolution.RootFolder.Items.OfType<SolutionFolder> ()
				.FirstOrDefault (item => item.Name == "src");
		}

		public void CreateWebRootDirectory ()
		{
			FilePath webRootDirectory = Project.BaseDirectory.Combine ("wwwroot");
			CreateDirectory (webRootDirectory);
			IsWebProject = true;
		}

		protected override IEnumerable<ExecutionTarget> OnGetExecutionTargets (ConfigurationSelector configuration)
		{
			if (project == null)
				return Enumerable.Empty<ExecutionTarget> ();

			return GetDotNetCoreExecutionTargets ();
		}

		IEnumerable<ExecutionTarget> GetDotNetCoreExecutionTargets ()
		{
			foreach (DotNetCoreExecutionTarget target in GetDotNetCoreExecutionTargets (project.Frameworks)) {
				yield return target;
			}
		}

		static IEnumerable<DotNetCoreExecutionTarget> GetDotNetCoreExecutionTargets (IEnumerable<DnxFramework> frameworks)
		{
			yield return DotNetCoreExecutionTarget.CreateDefaultTarget ();

			foreach (DnxFramework framework in frameworks) {
				yield return new DotNetCoreExecutionTarget (framework);
			}
		}

		public void UpdateReferences (DnxFramework framework)
		{
			if (framework.Name == CurrentFramework)
				return;

			CurrentFramework = framework.Name;

			RefreshCompilationOptions ();

			try {
				addingReferences = true;
				RemoveExistingReferences ();
				RefreshReferences ();
				RefreshProjectReferences ();
			} finally {
				addingReferences = false;
			}
		}

		void RefreshReferences ()
		{
			List<string> fileReferences = null;
			if (!savedFileReferences.TryGetValue (CurrentFramework, out fileReferences)) {
				LoggingService.LogWarning ("Unable to find references for framework '{0}'.", CurrentFramework);
				return;
			}

			UpdateReferences (fileReferences);
		}

		void RefreshProjectReferences ()
		{
			List<string> projectReferences = null;
			if (!savedProjectReferences.TryGetValue (CurrentFramework, out projectReferences)) {
				LoggingService.LogWarning ("Unable to find project for framework '{0}'.", CurrentFramework);
				return;
			}

			UpdateProjectReferences (projectReferences);
		}

		public string JsonPath {
			get {
				if (project != null) {
					return project.Path;
				}
				return null;
			}
		}

		protected override void OnReferenceAddedToProject (ProjectReferenceEventArgs e)
		{
			base.OnReferenceAddedToProject (e);

			if (addingReferences)
				return;

			if (e.ProjectReference.ReferenceType == ReferenceType.Project) {
				var jsonFile = ProjectJsonFile.Read (this);
				if (jsonFile.Exists) {
					jsonFile.AddProjectReference (e.ProjectReference);
					jsonFile.Save ();
					FileService.NotifyFileChanged (jsonFile.Path);
				} else {
					LoggingService.LogDebug ("Unable to find project.json '{0}'", jsonFile.Path);
				}
			}
		}

		protected override void OnReferenceRemovedFromProject (ProjectReferenceEventArgs e)
		{
			base.OnReferenceRemovedFromProject (e);

			if (addingReferences)
				return;

			if (e.ProjectReference.ReferenceType == ReferenceType.Project) {
				var jsonFile = ProjectJsonFile.Read (this);
				if (jsonFile.Exists) {
					jsonFile.RemoveProjectReference (e.ProjectReference);
					jsonFile.Save ();
					FileService.NotifyFileChanged (jsonFile.Path);
				} else {
					LoggingService.LogDebug ("Unable to find project.json '{0}'", jsonFile.Path);
				}
			}
		}

		public void RemoveProjectReference (string name)
		{
			ProjectReference matchedProjectReference = Items.OfType<ProjectReference> ()
				.FirstOrDefault (projectReference => IsProjectReferenceMatch (projectReference, name));

			if (matchedProjectReference != null) {
				Items.Remove (matchedProjectReference);
			}
		}

		bool IsProjectReferenceMatch (ProjectReference projectReference, string name)
		{
			return (projectReference.ReferenceType == ReferenceType.Project) &&
				(projectReference.Reference == name);
		}

		static bool IsProjectJsonLockFile (FilePath fileName)
		{
			return fileName.FileName.Equals ("project.lock.json", StringComparison.OrdinalIgnoreCase);
		}

		static bool IsProjectJsonFile (FilePath fileName)
		{
			return fileName.FileName.Equals ("project.json", StringComparison.OrdinalIgnoreCase);
		}

		void AddProjectJsonDependency (ProjectFile projectFile)
		{
			FilePath projectJsonFileName = GetProjectJsonFileName ();
			if (projectJsonFileName.IsNotNull) {
				projectFile.DependsOn = projectJsonFileName.FileName;
			}
		}

		FilePath GetProjectJsonFileName ()
		{
			ProjectFile projectJsonFile = Items.OfType<ProjectFile> ()
				.FirstOrDefault (projectFile => IsProjectJsonFile (projectFile.FilePath));
			
			if (projectJsonFile != null)
				return projectJsonFile.FilePath;

			FilePath projectJsonFileName = BaseDirectory.Combine ("project.json");
			if (File.Exists (projectJsonFileName))
				return projectJsonFileName;

			return null;
		}

		public void AddNuGetPackages (IEnumerable<NuGetPackageToAdd> packagesToAdd)
		{
			var jsonFile = ProjectJsonFile.Read (this);
			if (jsonFile.Exists) {
				jsonFile.AddNuGetPackages (packagesToAdd);
				jsonFile.Save ();
				FileService.NotifyFileChanged (jsonFile.Path);
			} else {
				LoggingService.LogDebug ("Unable to find project.json '{0}'", jsonFile.Path);
			}
		}

		protected override Task<List<AssemblyReference>> OnGetReferencedAssemblies (ConfigurationSelector configuration)
		{
			var references = new List<AssemblyReference> ();

			foreach (ProjectReference reference in Project.References.Where (r => r.ReferenceType != ReferenceType.Project)) {
				foreach (string assembly in reference.GetReferencedFileNames (configuration)) {
					references.Add (new AssemblyReference (assembly));
				}
			}

			return Task.FromResult<List<AssemblyReference>> (references);
		}

		protected override IEnumerable<SolutionItem> OnGetReferencedItems (ConfigurationSelector configuration)
		{
			return base.OnGetReferencedItems (configuration);
		}

		public void UpdateCompilationOptions (
			OmniSharp.Dnx.FrameworkProject frameworkProject,
			CSharpCompilationOptions compilationOptions,
			CSharpParseOptions parseOptions)
		{
			EnsureCurrentFrameworkDefined (frameworkProject);

			var options = new CSharpCompilationAndParseOptions (compilationOptions, parseOptions);
			savedCompilationOptions[frameworkProject.Framework] = options;

			if (CurrentFramework != frameworkProject.Framework)
				return;

			UpdateCompilationOptions (options);
		}

		void UpdateCompilationOptions (CSharpCompilationAndParseOptions options)
		{
			foreach (var config in Configurations.OfType<DotNetProjectConfiguration> ()) {
				var parameters = new DnxConfigurationParameters (options.CompilationOptions, options.ParseOptions);
				config.CompilationParameters = parameters;
			}
		}

		void RefreshCompilationOptions ()
		{
			CSharpCompilationAndParseOptions options;
			if (!savedCompilationOptions.TryGetValue (CurrentFramework, out options)) {
				LoggingService.LogWarning ("Unable to find compilation options for framework '{0}'.", CurrentFramework);
				return;
			}

			UpdateCompilationOptions (options);
		}

		public void RemoveNuGetPackage (string frameworkShortName, string packageId)
		{
			var jsonFile = ProjectJsonFile.Read (this);
			if (jsonFile.Exists) {
				jsonFile.RemoveNuGetPackage (frameworkShortName, packageId);
				jsonFile.Save ();
				FileService.NotifyFileChanged (jsonFile.Path);
			} else {
				LoggingService.LogDebug ("Unable to find project.json '{0}'", jsonFile.Path);
			}
		}

		public IEnumerable<DnxFramework> GetFrameworks ()
		{
			if (project != null) {
				foreach (DnxFramework framework in project.Frameworks) {
					yield return framework;
				}
			}
		}

		public bool IsWebProject { get; set; }

		void UpdateCachedProjectInformation ()
		{
			string[] frameworkNames = project.Frameworks.Select (framework => framework.Name).ToArray ();

			RemoveMissingKeys (dependencies, frameworkNames);
			RemoveMissingKeys (savedFileReferences, frameworkNames);
			RemoveMissingKeys (savedProjectReferences, frameworkNames);
			RemoveMissingKeys (savedCompilationOptions, frameworkNames);

			if (CurrentFramework != null) {
				if (!frameworkNames.Contains (CurrentFramework)) {
					CurrentFramework = null;
				}
			}
		}

		void RemoveMissingKeys<TKey, TValue>(IDictionary<TKey, TValue> items, TKey[] keys)
		{
			TKey[] missingKeys = items.Keys.Where (key => !keys.Contains (key)).ToArray ();
			foreach (TKey key in missingKeys) {
				items.Remove (key);
			}
		}
	}
}

