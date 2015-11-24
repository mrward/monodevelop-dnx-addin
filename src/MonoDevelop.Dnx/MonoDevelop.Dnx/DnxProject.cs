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
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Projects;
using OmniSharp.Models;

using DependenciesMessage = Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages.DependenciesMessage;
using FrameworkData = Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages.FrameworkData;
using System.Threading.Tasks;

namespace MonoDevelop.Dnx
{
	public class DnxProject : DotNetProjectExtension
	{
		OmniSharp.Models.DnxProject project;
		FilePath fileName;
		string name;
		bool addingReferences;
		Dictionary<string, DependenciesMessage> dependencies = new Dictionary<string, DependenciesMessage> ();
		Dictionary<string, List<string>> savedFileReferences = new Dictionary<string, List<string>> ();
		Dictionary<string, List<string>> savedProjectReferences = new Dictionary<string, List<string>> ();

		Dictionary<string, List<string>> preprocessorSymbols = new Dictionary<string, List<string>> ();

		public static readonly string ProjectTypeGuid = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";

		public DnxProject ()
//			: base ("C#")
		{
//			DnxMSBuildProjectHandler.InstallHandler (this);
//			UseMSBuildEngine = false;
		}

//		public DnxProject (ProjectCreateInformation info, XmlElement projectOptions)
//			: this ()
//		{
//		}

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

//		public override IEnumerable<string> GetProjectTypes ()
//		{
//			yield return "Dnx";
//		}

		protected override void OnEndLoad ()
		{
			LoadFiles ();
			base.OnEndLoad ();
		}

		void LoadFiles ()
		{
			foreach (string fileName in Directory.GetFiles (BaseDirectory, "*.*", SearchOption.AllDirectories)) {
				if (IsSupportedProjectFileItem (fileName)) {
					Items.Add (CreateFileProjectItem(fileName));
				}
			}
			AddConfigurations ();
		}

		bool IsSupportedProjectFileItem (string fileName)
		{
			string extension = Path.GetExtension(fileName);
			if (extension.EndsWith ("proj", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals (".sln", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals (".user", StringComparison.OrdinalIgnoreCase)) {
				return false;
			}
			return true;
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
		}

		protected override ExecutionCommand OnCreateExecutionCommand (ConfigurationSelector configSel, DotNetProjectConfiguration configuration)
		{
			return new DnxProjectExecutionCommand (
				BaseDirectory,
				DnxServices.ProjectService.CurrentDnxRuntime
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

//		protected override bool OnGetNeedsBuilding (ConfigurationSelector configuration)
//		{
//			return false;
//		}

		protected async override Task<TargetEvaluationResult> OnRunTarget (ProgressMonitor monitor, string target, ConfigurationSelector configuration, TargetEvaluationContext context)
		{
			if (target == ProjectService.BuildTarget) {
				using (var builder = new DnxProjectBuilder (this, monitor)) {
					return (new TargetEvaluationResult (builder.Build ()));
				}
			}
			return new TargetEvaluationResult(BuildResult.CreateSuccess ());
		}

		protected async override Task OnExecute (ProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configuration)
		{
			var config = GetConfiguration (configuration) as DotNetProjectConfiguration;
			monitor.Log.WriteLine (GettextCatalog.GetString ("Running {0} ...", Name));

			OperationConsole console = CreateConsole (config, context, monitor);

			try {
				try {
					ExecutionCommand executionCommand = OnCreateExecutionCommand (configuration, config);
					if (context.ExecutionTarget != null)
						executionCommand.Target = context.ExecutionTarget;

					ProcessAsyncOperation asyncOp = new DnxExecutionHandler ().Execute (executionCommand, console);
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

		SolutionItemConfiguration GetConfiguration (ConfigurationSelector configuration)
		{
			return Project.GetConfiguration (configuration);
		}

		OperationConsole CreateConsole (DotNetProjectConfiguration config, ExecutionContext context, ProgressMonitor monitor)
		{
			if (config.ExternalConsole)
				return context.ExternalConsoleFactory.CreateConsole (!config.PauseConsoleOutput, monitor.CancellationToken);
			return context.ConsoleFactory.CreateConsole (monitor.CancellationToken);
		}

		protected override FilePath OnGetOutputFileName (ConfigurationSelector configuration)
		{
			return null;
		}

//		/// <summary>
//		/// Have to override the SolutionEntityItem otherwise the FileFormat 
//		/// changes the file extension back to .csproj when GetValidFileName is called.
//		/// This is because the FileFormat finds the DotNetProjectNode for csproj files
//		/// when looking at the /MonoDevelop/ProjectModel/MSBuildItemTypes extension.
//		/// There does not seem to be a way to insert the DotNetProjectNode for DnxProjects
//		/// since these extensions do not have an id.
//		/// </summary>
//		public override FilePath FileName {
//			get {
//				return fileName;
//			}
//			set {
//				fileName = value;
//				fileName = fileName.ChangeExtension (".xproj");
//				if (ItemHandler.SyncFileName)
//					Name = fileName.FileNameWithoutExtension;
//				NotifyModified ("FileName");
//			}
//		}

//		public override string Name {
//			get {
//				return name ?? string.Empty;
//			}
//			set {
//				if (name == value)
//					return;
//				string oldName = name;
//				name = value;
//				if (!Loading && ItemHandler.SyncFileName) {
//					if (string.IsNullOrEmpty (fileName))
//						FileName = value;
//					else {
//						string ext = fileName.Extension;
//						FileName = fileName.ParentDirectory.Combine (value) + ext;
//					}
//				}
//				OnNameChanged (new SolutionItemRenamedEventArgs(this, oldName, name));
//			}
//		}

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
				return base.OnSave (monitor);
			}
			IsDirty = false;
			return Task.FromResult (0);
		}

//		public override bool SupportsConfigurations ()
//		{
//			return true;
//		}

		public void GenerateNewProjectFileName (Solution solution, string projectName)
		{
			FileName = solution.BaseDirectory
				.Combine ("src", projectName, projectName)
				.ChangeExtension (".xproj");
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
		}

		protected override IEnumerable<ExecutionTarget> OnGetExecutionTargets (ConfigurationSelector configuration)
		{
			if (project == null)
				return Enumerable.Empty<ExecutionTarget> ();

			return GetDnxExecutionTargets ();
		}

		IEnumerable<ExecutionTarget> GetDnxExecutionTargets ()
		{
			foreach (string command in project.Commands.Keys) {
				foreach (DnxExecutionTarget target in GetDnxExecutionTargets (command, project.Frameworks)) {
					yield return target;
				}
			}
		}

		static IEnumerable<DnxExecutionTarget> GetDnxExecutionTargets (string command, IEnumerable<DnxFramework> frameworks)
		{
			yield return DnxExecutionTarget.CreateDefaultTarget (command);

			foreach (DnxFramework framework in frameworks) {
				yield return new DnxExecutionTarget (command, framework);
			}
		}

		public void UpdateReferences (DnxExecutionTarget executionTarget)
		{
			if (IsCurrentFramework (executionTarget))
				return;

			UpdateCurrentFramework (executionTarget);

			RefreshPreprocessorSymbols ();

			try {
				addingReferences = true;
				RemoveExistingReferences ();
				RefreshReferences ();
				RefreshProjectReferences ();
			} finally {
				addingReferences = false;
			}
		}

		bool IsCurrentFramework (DnxExecutionTarget executionTarget)
		{
			if (executionTarget.IsDefaultProfile) {
				return CurrentFramework == savedFileReferences.Keys.FirstOrDefault ();
			}

			return executionTarget.Framework.Name == CurrentFramework;
		}

		void UpdateCurrentFramework (DnxExecutionTarget executionTarget)
		{
			if (executionTarget.IsDefaultProfile) {
				CurrentFramework = savedFileReferences.Keys.FirstOrDefault ();
			} else {
				CurrentFramework = executionTarget.Framework.Name;
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

		public void UpdateParseOptions (OmniSharp.Dnx.FrameworkProject frameworkProject, Microsoft.CodeAnalysis.ParseOptions options)
		{
			EnsureCurrentFrameworkDefined (frameworkProject);

			List<string> symbols = options.PreprocessorSymbolNames.ToList ();
			preprocessorSymbols[frameworkProject.Framework] = symbols;

			if (CurrentFramework != frameworkProject.Framework)
				return;

			UpdatePreprocessorSymbols (symbols);
		}

		void UpdatePreprocessorSymbols (IEnumerable<string> symbols)
		{
			foreach (var config in Configurations.OfType<DotNetProjectConfiguration> ()) {
				var parameters = new DnxConfigurationParameters ();
				parameters.UpdatePreprocessorSymbols (symbols);
				config.CompilationParameters = parameters;
			}
		}

		void RefreshPreprocessorSymbols ()
		{
			List<string> symbols = null;
			if (!preprocessorSymbols.TryGetValue (CurrentFramework, out symbols)) {
				LoggingService.LogWarning ("Unable to find preprocessor symbols for framework '{0}'.", CurrentFramework);
				return;
			}

			UpdatePreprocessorSymbols (symbols);
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

		protected override void OnWriteProject (ProgressMonitor monitor, MonoDevelop.Projects.Formats.MSBuild.MSBuildProject msproject)
		{
			var projectBuilder = new DnxMSBuildProjectHandler (this);
			projectBuilder.SaveProject (monitor, msproject);
		}

		protected override Task<List<string>> OnGetReferencedAssemblies (ConfigurationSelector configuration)
		{
			var references = new List<string> ();

			foreach (ProjectReference reference in Project.References.Where (r => r.ReferenceType != ReferenceType.Project)) {
				foreach (string assembly in reference.GetReferencedFileNames (configuration)) {
					references.Add (assembly);
				}
			}

			return Task.FromResult<List<string>> (references);
		}

		protected override IEnumerable<SolutionItem> OnGetReferencedItems (ConfigurationSelector configuration)
		{
			return base.OnGetReferencedItems (configuration);
		}
	}
}

