////
//// Workspace.cs
////
//// Author:
////       Matt Ward <ward.matt@gmail.com>
////
//// Copyright (c) 2015 Matthew Ward
////
//// Permission is hereby granted, free of charge, to any person obtaining a copy
//// of this software and associated documentation files (the "Software"), to deal
//// in the Software without restriction, including without limitation the rights
//// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//// copies of the Software, and to permit persons to whom the Software is
//// furnished to do so, subject to the following conditions:
////
//// The above copyright notice and this permission notice shall be included in
//// all copies or substantial portions of the Software.
////
//// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//// THE SOFTWARE.
////
//
//using MonoDevelop.Dnx;
//using MonoDevelop.Dnx.Omnisharp;
//using OmniSharp.Dnx;
//
//namespace Microsoft.CodeAnalysis
//{
//	/// <summary>
//	/// Dummy Roslyn workspace.
//	/// </summary>
//	public class Workspace
//	{
//		public Workspace()
//		{
//			CurrentSolution = new RoslynSolution ();
//		}
//
//		public RoslynSolution CurrentSolution { get; private set; }
//
//		protected internal void OnProjectAdded (ProjectInfo projectInfo)
//		{
//			CurrentSolution.AddProject (projectInfo);
//		}
//
//		protected internal void OnProjectRemoved (ProjectId projectId)
//		{
//			CurrentSolution.RemoveProject (projectId);
//		}
//
//		protected internal void OnProjectReferenceAdded (ProjectId projectId, ProjectReference projectReference)
//		{
//		}
//
//		protected internal void OnProjectReferenceRemoved (ProjectId projectId, ProjectReference projectReference)
//		{
//		}
//
//		protected internal void OnMetadataReferenceAdded (ProjectId projectId, MetadataReference metadataReference)
//		{
//		}
//
//		protected internal void OnMetadataReferenceRemoved (ProjectId projectId, MetadataReference metadataReference)
//		{
//		}
//
//		public void ReferencesUpdated (ProjectId projectId, FrameworkProject frameworkProject)
//		{
//			DnxServices.ProjectService.OnReferencesUpdated (projectId, frameworkProject);
//		}
//
//		protected internal void OnParseOptionsChanged (ProjectId projectId, ParseOptions options)
//		{
//			DnxServices.ProjectService.OnParseOptionsChanged (projectId, options);
//		}
//	}
//}
