//
// JsonFile.cs
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
using System.Text;
using MonoDevelop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Dnx
{
	public class JsonFile
	{
		FilePath filePath;
		Encoding encoding = Encoding.UTF8;

		protected JObject jsonObject;

		protected JsonFile (FilePath filePath)
		{
			this.filePath = filePath;
		}

		public bool Exists { get; private set; }

		protected void Read ()
		{
			Exists = JsonFileExists ();
			if (!Exists)
				return;

			using (var fileStream = File.OpenRead (filePath)) {
				using (var reader = new StreamReader (fileStream)) {
					using (var jsonReader = new JsonTextReader (reader)) {
						jsonObject = JObject.Load (jsonReader);
						if (reader.CurrentEncoding != null) {
							encoding = reader.CurrentEncoding;
						}
					}
				}
			}

			AfterRead ();
		}

		protected virtual void AfterRead ()
		{
		}

		bool JsonFileExists ()
		{
			return File.Exists (filePath);
		}

		public void Save ()
		{
			BeforeSave ();
			using (var writer = new StreamWriter (filePath, false, encoding)) {
				using (var jsonWriter = new JsonTextWriter (writer)) {
					jsonWriter.Formatting = Formatting.Indented;
					jsonWriter.Indentation = 1;
					jsonWriter.IndentChar = '\t';
					jsonObject.WriteTo (jsonWriter);
				}
			}
		}

		protected virtual void BeforeSave ()
		{
		}

		public FilePath Path {
			get { return filePath; }
		}

		protected static void InsertSorted (JObject parent, JProperty propertyToAdd)
		{
			List<JToken> children = parent.Children ().ToList ();
			foreach (JToken child in children) {
				child.Remove ();
			}

			bool added = false;

			foreach (JToken child in children) {
				if (!added) {
					int result = Compare (propertyToAdd, child);
					if (result <= 0) {
						parent.Add (propertyToAdd);
						added = true;
						if (result == 0) {
							continue;
						}
					}
				}
				parent.Add (child);
			}

			if (!added)
				parent.Add (propertyToAdd);
		}

		static int Compare (JProperty a, JToken b)
		{
			var propertyB = b as JProperty;
			if (propertyB != null) {
				return String.Compare (a.Name, propertyB.Name, StringComparison.OrdinalIgnoreCase);
			}

			return 1;
		}
	}
}

