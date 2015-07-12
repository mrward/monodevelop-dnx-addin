//
// AspNet5ProjectSystemFactory.cs
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

using Microsoft.AspNet.Hosting;
using MonoDevelop.Dnx.Omnisharp;
using MonoDevelop.Projects;
using OmniSharp;
using OmniSharp.AspNet5;
using OmniSharp.Services;

namespace MonoDevelop.Dnx
{
	public class AspNet5ProjectSystemFactory
	{
		public AspNet5ProjectSystem CreateProjectSystem (
			Solution solution,
			IApplicationLifetime appLifetime,
			AspNet5Context context)
		{
			var workspace = new OmnisharpWorkspace ();
			var env = new OmnisharpEnvironment (solution.BaseDirectory);
			var options = new OmniSharpOptionsWrapper ();
			var loggerFactory = new LoggerFactory ();
			var cache = new MetadataFileReferenceCache ();
			var emitter = new EventEmitter ();
			var watcher = new FileSystemWatcherWrapper (env);

			return new AspNet5ProjectSystem (
				workspace,
				env,
				options,
				loggerFactory,
				cache,
				appLifetime,
				watcher,
				emitter,
				context);
		}
	}
}


