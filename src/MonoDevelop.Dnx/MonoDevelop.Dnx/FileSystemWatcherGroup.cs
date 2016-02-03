//
// FileSystemWatcherGroup.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
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
using MonoDevelop.Core;
using MonoDevelop.Dnx.Omnisharp;
using MonoDevelop.Projects;
using OmniSharp.Services;

namespace MonoDevelop.Dnx
{
	public class FileSystemWatcherGroup : IFileSystemWatcher
	{
		List<IFileSystemWatcher> watchers = new List<IFileSystemWatcher> ();

		public FileSystemWatcherGroup (Solution solution)
		{
			CreateWatchers (solution);
		}

		void CreateWatchers (Solution solution)
		{
			AddWatchersForDirectoriesDefinedInGlobalJson (solution);

			if (!watchers.Any ()) {
				AddDefaultWatcher (solution);
			}
		}

		void AddWatchersForDirectoriesDefinedInGlobalJson (Solution solution)
		{
			foreach (string projectSubDirectory in GetDirectoriesDefinedInGlobalJson (solution)) {
				FilePath projectDirectory = solution.BaseDirectory.Combine (projectSubDirectory);
				if (Directory.Exists (projectDirectory)) {
					AddWatcher (projectDirectory);
				}
			}
		}

		IEnumerable<string> GetDirectoriesDefinedInGlobalJson (Solution solution)
		{
			try {
				var globalJson = GlobalJsonFile.Read (solution);
				return globalJson.ProjectDirectories;
			} catch (Exception ex) {
				LoggingService.LogError ("Unable to read directories defined in global.json.", ex);
			}

			return Enumerable.Empty<string> ();
		}

		void AddWatcher (FilePath directory)
		{
			var env = new OmnisharpEnvironment (directory);
			var watcher = new FileSystemWatcherWrapper (env);
			watchers.Add (watcher);
		}

		void AddDefaultWatcher (Solution solution)
		{
			FilePath directoryToWatch = solution.BaseDirectory.Combine ("src");
			if (!Directory.Exists (directoryToWatch)) {
				directoryToWatch = solution.BaseDirectory;
				LoggingService.LogWarning ("src directory not found. Watching solution directory");
			}

			AddWatcher (directoryToWatch);
		}

		public void Watch (string path, Action<string> callback)
		{
			foreach (IFileSystemWatcher watcher in watchers) {
				watcher.Watch (path, callback);
			}
		}

		public void TriggerChange (string path)
		{
			watchers.First ().TriggerChange (path);
		}

		public void Dispose ()
		{
			foreach (IFileSystemWatcher watcher in watchers) {
				watcher.Dispose ();
			}
		}
	}
}

