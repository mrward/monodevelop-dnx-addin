//
// DnxMessageService.cs
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

using MonoDevelop.Components.Extensions;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.Dnx
{
	public static class DnxMessageService
	{
		public static void ShowDotNetCoreNotInstalledError (DotNetCoreNotFoundException ex)
		{
			var message = new GenericMessage (ex.Message, ex.Details);
			var downloadButton = new AlertButton (GettextCatalog.GetString ("Go to download"));
			message.Icon = Stock.Error;
			message.Buttons.Add (downloadButton);
			message.Buttons.Add (AlertButton.Ok);
			message.DefaultButton = 1;
			var dialog = new AlertDialog (message);
			dialog.TransientFor = IdeApp.Workbench.RootWindow;
			var result = dialog.Run ();

			if (result == downloadButton) {
				DesktopService.ShowUrl ("https://www.microsoft.com/net/core");
			}
		}
	}
}

