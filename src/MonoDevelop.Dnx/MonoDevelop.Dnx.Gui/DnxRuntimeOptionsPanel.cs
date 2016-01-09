//
// DnxRuntimeOptionsPanel.cs
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
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;

namespace MonoDevelop.Dnx.Gui
{
	public class DnxRuntimeOptionsPanel : OptionsPanel
	{
		Gtk.ComboBox dnxRuntimeVersionComboBox;
		GlobalJsonFile globalJsonFile;
		List<string> dnxRuntimeVersions;
		string originalDnxRuntimeVersion;

		public override Gtk.Widget CreatePanelWidget ()
		{
			var vbox = new Gtk.VBox ();
			var hbox = new Gtk.HBox ();
			var label = new Gtk.Label (GettextCatalog.GetString ("DNX Runtime Version:"));
			label.Xalign = 0;
			hbox.PackStart (label, false, false, 5);

			dnxRuntimeVersionComboBox = Gtk.ComboBox.NewText ();
			hbox.PackStart (dnxRuntimeVersionComboBox, true, true, 5);

			PopulateOptionsPanel ();

			vbox.PackStart (hbox);
			vbox.ShowAll ();
			return vbox;
		}

		void PopulateOptionsPanel ()
		{
			dnxRuntimeVersions = GetDnxRuntimeVersions ().ToList ();
			globalJsonFile = GlobalJsonFile.Read ((DnxProject)DataObject);
			if (globalJsonFile.Exists) {
				originalDnxRuntimeVersion = globalJsonFile.DnxRuntimeVersion;
			}

			if (globalJsonFile == null)
				return;

			if (globalJsonFile.Exists) {
				AddDnxRuntimeVersionsToComboBox ();
				SelectDnxRuntimeVersionInComboBox (globalJsonFile.DnxRuntimeVersion);
			}
		}

		IEnumerable<string> GetDnxRuntimeVersions ()
		{
			return new string[] {
				"1.0.0-beta4",
				"1.0.0-beta5",
				"1.0.0-beta6",
				"1.0.0-beta7",
				"1.0.0-beta8",
				"1.0.0-rc1-final",
				"1.0.0-rc1-update1"
			};
		}

		void AddDnxRuntimeVersionsToComboBox ()
		{
			foreach (string version in dnxRuntimeVersions) {
				dnxRuntimeVersionComboBox.AppendText (version);
			}
		}

		public override void ApplyChanges ()
		{
			if (globalJsonFile == null)
				return;

			globalJsonFile.DnxRuntimeVersion = dnxRuntimeVersionComboBox.ActiveText;
			if (originalDnxRuntimeVersion != globalJsonFile.DnxRuntimeVersion) {
				globalJsonFile.Save ();
				FileService.NotifyFileChanged (globalJsonFile.Path);
			}
		}

		void SelectDnxRuntimeVersionInComboBox (string version)
		{
			dnxRuntimeVersionComboBox.Active = dnxRuntimeVersions.IndexOf (version);
		}
	}
}

