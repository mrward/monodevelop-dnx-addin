//
// DnxGlobalOptionsPanel.cs
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
using Microsoft.Framework.Logging;
using MonoDevelop.Components;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;

namespace MonoDevelop.Dnx.Gui
{
	public class DnxGlobalOptionsPanel : OptionsPanel
	{
		bool originalRestoreDependenciesSetting = DnxServices.ProjectService.RestoreDependencies;
		Gtk.CheckButton restoreCheckBox;

		Gtk.ComboBox logLevelsComboBox;
		readonly List<LogLevel> logLevels = new List<LogLevel> {
			LogLevel.Debug,
			LogLevel.Verbose,
			LogLevel.Information,
			LogLevel.Warning,
			LogLevel.Error,
			LogLevel.Critical
		};

		public override Control CreatePanelWidget()
		{
			var vbox = new Gtk.VBox ();
			vbox.Spacing = 6;

			var restoreSectionLabel = new Gtk.Label (GetBoldMarkup (".NET Core Restore"));
			restoreSectionLabel.UseMarkup = true;
			restoreSectionLabel.Xalign = 0;
			vbox.PackStart (restoreSectionLabel, false, false, 0);

			restoreCheckBox = new Gtk.CheckButton (GettextCatalog.GetString ("Automatically restore dependencies."));
			restoreCheckBox.Active = originalRestoreDependenciesSetting;
			restoreCheckBox.BorderWidth = 10;
			vbox.PackStart (restoreCheckBox, false, false, 0);

			var outputSectionLabel = new Gtk.Label (GetBoldMarkup (".NET Core Output"));
			outputSectionLabel.UseMarkup = true;
			outputSectionLabel.Xalign = 0;
			vbox.PackStart (outputSectionLabel, false, false, 0);

			var outputVerbosityHBox = new Gtk.HBox ();
			outputVerbosityHBox.BorderWidth = 10;
			outputVerbosityHBox.Spacing = 6;
			var outputVerbosityLabel = new Gtk.Label (GettextCatalog.GetString ("Verbosity:"));
			outputVerbosityLabel.Xalign = 0;
			outputVerbosityHBox.PackStart (outputVerbosityLabel, false, false, 0);

			logLevelsComboBox = Gtk.ComboBox.NewText ();
			outputVerbosityHBox.PackStart (logLevelsComboBox, true, true, 0);

			AddLogLevelsToComboBox ();

			vbox.PackStart (outputVerbosityHBox, false, false, 0);
			vbox.ShowAll ();
			return vbox;
		}

		static string GetBoldMarkup (string phrase)
		{
			return "<b>" + GettextCatalog.GetString (phrase) + "</b>";
		}

		void AddLogLevelsToComboBox ()
		{
			foreach (LogLevel logLevel in logLevels) {
				logLevelsComboBox.AppendText (logLevel.ToString ());
			}

			logLevelsComboBox.Active = logLevels.IndexOf (DnxLoggerService.LogLevel);
		}

		public override void ApplyChanges()
		{
			bool restore = restoreCheckBox.Active;
			if (restore != originalRestoreDependenciesSetting) {
				DnxServices.ProjectService.RestoreDependencies = restore;
			}

			DnxLoggerService.LogLevel = logLevels [logLevelsComboBox.Active];
		}
	}
}

