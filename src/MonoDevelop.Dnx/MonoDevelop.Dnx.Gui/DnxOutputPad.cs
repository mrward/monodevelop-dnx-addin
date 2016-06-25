//
// DnxOutputPad.cs
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

using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using Microsoft.Framework.Logging;
using System;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;

namespace MonoDevelop.Dnx
{
	public class DnxOutputPad : PadContent
	{
		static DnxOutputPad instance;
		static readonly LogView logView = new LogView ();

		public DnxOutputPad ()
		{
			instance = this;
		}

		public static DnxOutputPad Instance {
			get { return instance; }
		}

		protected override void Initialize (IPadWindow window)
		{
			DockItemToolbar toolbar = window.GetToolbar (DockPositionType.Right);

			var clearButton = new Button (new ImageView (Ide.Gui.Stock.Broom, IconSize.Menu));
			clearButton.Clicked += ButtonClearClick;
			clearButton.TooltipText = GettextCatalog.GetString ("Clear");
			toolbar.Add (clearButton);
			toolbar.ShowAll ();
			logView.ShowAll ();
		}

		public override Control Control {
			get { return logView; }
		}

		public static LogView LogView {
			get { return logView; }
		}

		void ButtonClearClick (object sender, EventArgs e)
		{
			logView.Clear ();
		}

		public static void WriteText (string message)
		{
			logView.WriteText (message + Environment.NewLine);
		}

		public static void WriteError (string message)
		{
			logView.WriteError (message + Environment.NewLine);
		}
	}
}

