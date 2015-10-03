//
// GlobalJsonFile.cs
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

using System.IO;
using System.Text;
using MonoDevelop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Dnx
{
	public class GlobalJsonFile
	{
		FilePath filePath;
		Encoding encoding = Encoding.UTF8;
		JObject jsonObject;
		JObject sdkObject;

		GlobalJsonFile (FilePath filePath)
		{
			this.filePath = filePath;
		}

		public static GlobalJsonFile Read (DnxProject project)
		{
			var jsonFile = new GlobalJsonFile (project.ParentSolution.BaseDirectory.Combine ("global.json"));
			jsonFile.Read ();
			return jsonFile;
		}

		public bool Exists { get; private set; }

		public string DnxRuntimeVersion { get; set; }

		void Read ()
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

			ReadPropertiesFromJsonObject ();
		}

		bool JsonFileExists ()
		{
			return File.Exists (filePath);
		}

		void ReadPropertiesFromJsonObject ()
		{
			JToken sdkToken;
			if (!jsonObject.TryGetValue ("sdk", out sdkToken))
				return;

			sdkObject = sdkToken as JObject;
			if (sdkObject == null)
				return;

			JToken version;
			if (!sdkObject.TryGetValue ("version", out version))
				return;

			DnxRuntimeVersion = version.ToString ();
		}

		public void Save ()
		{
			sdkObject["version"] = new JValue (DnxRuntimeVersion);
			using (var writer = new StreamWriter (filePath, false, encoding)) {
				using (var jsonWriter = new JsonTextWriter (writer)) {
					jsonWriter.Formatting = Formatting.Indented;
					jsonWriter.Indentation = 1;
					jsonWriter.IndentChar = '\t';
					jsonObject.WriteTo (jsonWriter);
				}
			}
		}

		public FilePath Path {
			get { return filePath; }
		}
	}
}

