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

namespace MonoDevelop.Dnx
{
	public class DnxProject : DotNetAssemblyProject
	{
		OmniSharp.Models.DnxProject project;
		FilePath fileName;
		string name;
		Dictionary<string, DependenciesMessage> dependencies = new Dictionary<string, DependenciesMessage> ();

		public static readonly string ProjectTypeGuid = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";

		public DnxProject ()
			: base ("C#")
		{
			DnxMSBuildProjectHandler.InstallHandler (this);
			UseMSBuildEngine = false;
		}

		public DnxProject (ProjectCreateInformation info, XmlElement projectOptions)
			: this ()
		{
			AddConfigurations ();
		}

		public override IEnumerable<string> GetProjectTypes ()
		{
			yield return "Dnx";
		}

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
			return new ProjectFile (fileName, GetDefaultBuildAction (fileName));
		}

		public override string GetDefaultBuildAction (string fileName)
		{
			if (IsCSharpFile (fileName)) {
				return BuildAction.Compile;
			}
			return base.GetDefaultBuildAction (fileName);
		}

		static bool IsCSharpFile (string fileName)
		{
			string extension = Path.GetExtension (fileName);
			return String.Equals (".cs", extension, StringComparison.OrdinalIgnoreCase);
		}

		public void AddAssemblyReference (string fileName)
		{
			var projectItem = new ProjectReference (ReferenceType.Assembly, fileName);
			References.Add (projectItem);
		}

		public bool IsCurrentFramework (string framework, IEnumerable<string> frameworks)
		{
			if (CurrentFramework == null) {
				CurrentFramework = frameworks.FirstOrDefault ();
			}

			return CurrentFramework == framework;
		}

		public string CurrentFramework { get; private set; }

		public void UpdateReferences (IEnumerable<string> references)
		{
			var oldReferenceItems = Items.OfType<ProjectReference> ().ToList ();
			Items.RemoveRange (oldReferenceItems);

			foreach (string reference in references) {
				AddAssemblyReference (reference);
			}
		}

		public void Update (OmniSharp.Models.DnxProject project)
		{
			this.project = project;
			base.OnExecutionTargetsChanged ();
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

		protected override ExecutionCommand CreateExecutionCommand (ConfigurationSelector configSel, DotNetProjectConfiguration configuration)
		{
			return new DnxProjectExecutionCommand (
				BaseDirectory,
				DnxServices.ProjectService.CurrentDnxRuntime
			);
		}

		protected override bool OnGetSupportsTarget (string target)
		{
			return false;
		}

		protected override bool OnGetCanExecute (ExecutionContext context, ConfigurationSelector configuration)
		{
			return true;
		}

		protected override bool OnGetNeedsBuilding (ConfigurationSelector configuration)
		{
			return false;
		}

		protected override BuildResult OnRunTarget (IProgressMonitor monitor, string target, ConfigurationSelector configuration)
		{
			return new BuildResult ();
		}

		protected override void DoExecute (IProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configuration)
		{
			var config = GetConfiguration (configuration) as DotNetProjectConfiguration;
			monitor.Log.WriteLine (GettextCatalog.GetString ("Running {0} ...", Name));

			IConsole console = CreateConsole (config, context);
			var aggregatedOperationMonitor = new AggregatedOperationMonitor (monitor);

			try {
				try {
					ExecutionCommand executionCommand = CreateExecutionCommand (configuration, config);
					if (context.ExecutionTarget != null)
						executionCommand.Target = context.ExecutionTarget;

					IProcessAsyncOperation asyncOp = new DnxExecutionHandler ().Execute (executionCommand, console);
					aggregatedOperationMonitor.AddOperation (asyncOp);
					asyncOp.WaitForCompleted ();

					monitor.Log.WriteLine (GettextCatalog.GetString ("The application exited with code: {0}", asyncOp.ExitCode));
				} finally {
					console.Dispose ();
					aggregatedOperationMonitor.Dispose ();
				}
			} catch (Exception ex) {
				LoggingService.LogError (string.Format ("Cannot execute \"{0}\"", Name), ex);
				monitor.ReportError (GettextCatalog.GetString ("Cannot execute \"{0}\"", Name), ex);
			}
		}

		IConsole CreateConsole (DotNetProjectConfiguration config, ExecutionContext context)
		{
			if (config.ExternalConsole)
				return context.ExternalConsoleFactory.CreateConsole (!config.PauseConsoleOutput);
			return context.ConsoleFactory.CreateConsole (!config.PauseConsoleOutput);
		}

		public override FilePath GetOutputFileName (ConfigurationSelector configuration)
		{
			return null;
		}

		/// <summary>
		/// Have to override the SolutionEntityItem otherwise the FileFormat 
		/// changes the file extension back to .csproj when GetValidFileName is called.
		/// This is because the FileFormat finds the DotNetProjectNode for csproj files
		/// when looking at the /MonoDevelop/ProjectModel/MSBuildItemTypes extension.
		/// There does not seem to be a way to insert the DotNetProjectNode for DnxProjects
		/// since these extensions do not have an id.
		/// </summary>
		public override FilePath FileName {
			get {
				return fileName;
			}
			set {
				fileName = value;
				fileName = fileName.ChangeExtension (".xproj");
				if (ItemHandler.SyncFileName)
					Name = fileName.FileNameWithoutExtension;
				NotifyModified ("FileName");
			}
		}

		public override string Name {
			get {
				return name ?? string.Empty;
			}
			set {
				if (name == value)
					return;
				string oldName = name;
				name = value;
				if (!Loading && ItemHandler.SyncFileName) {
					if (string.IsNullOrEmpty (fileName))
						FileName = value;
					else {
						string ext = fileName.Extension;
						FileName = fileName.ParentDirectory.Combine (value) + ext;
					}
				}
				OnNameChanged (new SolutionItemRenamedEventArgs(this, oldName, name));
			}
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

		public event EventHandler PackageRestoreStarted;

		public void OnPackageRestoreStarted ()
		{
			var handler = PackageRestoreStarted;
			if (handler != null)
				handler (this, new EventArgs ());
		}

		public event EventHandler PackageRestoreFinished;

		public void OnPackageRestoreFinished ()
		{
			var handler = PackageRestoreFinished;
			if (handler != null)
				handler (this, new EventArgs ());
		}

		internal bool IsDirty { get; set; }

		protected override void OnSave (IProgressMonitor monitor)
		{
			if (IsDirty) {
				defaultNamespace = GetDefaultNamespace (FileName);
				base.OnSave (monitor);
			}
			IsDirty = false;
		}

		public override bool SupportsConfigurations ()
		{
			return true;
		}

		public void GenerateNewProjectFileName (Solution solution, string projectName)
		{
			FileName = solution.BaseDirectory
				.Combine ("src", projectName, projectName)
				.ChangeExtension (".xproj");
		}

		public void CreateProjectDirectory ()
		{
			CreateDirectory (BaseDirectory);
		}

		static void CreateDirectory (FilePath directory)
		{
			if (!Directory.Exists (directory)) {
				Directory.CreateDirectory (directory);
			}
		}

		public SolutionFolder GetSrcSolutionFolder ()
		{
			return ParentSolution.RootFolder.Items.OfType<SolutionFolder> ()
				.FirstOrDefault (item => item.Name == "src");
		}

		public void CreateWebRootDirectory ()
		{
			FilePath webRootDirectory = BaseDirectory.Combine ("wwwroot");
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
	}
}

