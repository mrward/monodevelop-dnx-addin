//
// ConsoleWrapper.cs
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
using System.IO;
using MonoDevelop.Core.Execution;

namespace MonoDevelop.Dnx
{
	/// <summary>
	/// Prevents a disposed error when building .NET Core projects since the
	/// LogViewProgressMonitor's console will be disposed when the dotnet process
	/// exits and then again by the MonoDevelop build system.
	/// </summary>
	class ConsoleWrapper : IConsole
	{
		IConsole console;

		public ConsoleWrapper (IConsole console)
		{
			this.console = console;
		}

		public event EventHandler CancelRequested {
			add { console.CancelRequested += value; }
			remove { console.CancelRequested -= value; }
		}

		public TextReader In {
			get { return console.In; }
		}

		public TextWriter Out {
			get { return console.Out; }
		}

		public TextWriter Error {
			get { return console.Error; }
		}

		public TextWriter Log {
			get { return console.Log; }
		}

		public bool CloseOnDispose {
			get { return console.CloseOnDispose; }
		}

		public void Dispose ()
		{
		}
	}
}

