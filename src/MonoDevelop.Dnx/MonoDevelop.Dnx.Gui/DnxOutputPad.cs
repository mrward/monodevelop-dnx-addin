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
	public class DnxOutputPad : AbstractPadContent, ILogger
	{
		static DnxOutputPad instance;
		readonly LogView logView = new LogView ();

		public DnxOutputPad ()
		{
			instance = this;
			IdeApp.Workspace.SolutionLoaded += SolutionLoaded;
		}

		public static DnxOutputPad Instance {
			get { return instance; }
		}

		public override void Dispose ()
		{
			IdeApp.Workspace.SolutionLoaded -= SolutionLoaded;
		}

		public override void Initialize (IPadWindow container)
		{
			DockItemToolbar toolbar = container.GetToolbar (PositionType.Right);

			var clearButton = new Button (new Gtk.Image (Ide.Gui.Stock.Broom, IconSize.Menu));
			clearButton.Clicked += ButtonClearClick;
			clearButton.TooltipText = GettextCatalog.GetString ("Clear");
			toolbar.Add (clearButton);
			toolbar.ShowAll ();
			Control.ShowAll ();
		}

		public override Widget Control {
			get { return logView; }
		}

		public LogView LogView {
			get { return logView; }
		}

		void ButtonClearClick (object sender, EventArgs e)
		{
			logView.Clear ();
		}

		void SolutionLoaded (object sender, SolutionEventArgs e)
		{
			logView.Clear ();
		}

		public void Log (LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
		{
			if (!IsEnabled (logLevel))
				return;

			string message = formatter.Invoke (state, exception) + Environment.NewLine;
			switch (logLevel) {
				case LogLevel.Verbose:
					logView.WriteConsoleLogText (message);
				break;
				case LogLevel.Information:
				case LogLevel.Warning:
					logView.WriteText (message);
				break;
				case LogLevel.Critical:
				case LogLevel.Error:
					logView.WriteError (message);
				break;
			}
		}

		public bool IsEnabled (LogLevel logLevel)
		{
			return logLevel >= DnxLoggerService.LogLevel;
		}

		public IDisposable BeginScope (object state)
		{
			throw new NotImplementedException ();
		}
	}
}

