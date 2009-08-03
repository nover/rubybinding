// 
//  Copyright (C) 2009 Levi Bard
// 
// This source code is licenced under The MIT License:
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.IO;
using System.Collections.Generic;

using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.RubyBinding.Navigation
{
	/// <summary>
	/// Node builder extension for Ruby classpad support.
	/// </summary>
	public class ProjectNodeBuilderExtension: NodeBuilderExtension
	{
		public ClassPadEventHandler finishedBuildingTreeHandler;
		
		static Dictionary<ProjectFile,ParsedDocument> docs = new Dictionary<ProjectFile, ParsedDocument> ();
		
		public override bool CanBuildNode (Type dataType)
		{
			return typeof(RubyProject).IsAssignableFrom (dataType);
		}
		
		public override Type CommandHandlerType {
			get { return typeof(ProjectNodeBuilderExtensionHandler); }
		}
		
		protected override void Initialize ()
		{
			finishedBuildingTreeHandler = (ClassPadEventHandler)DispatchService.GuiDispatch (new ClassPadEventHandler (OnFinishedBuildingTree));
		}
		
		public override void Dispose ()
		{
		}
		
		/// <summary>
		/// Parse project files.
		/// </summary>
		public static void CreatePadTree (object o)
		{
			RubyProject p = o as RubyProject;
			if (o == null) return;
			
			foreach (ProjectFile file in p.Files) {
				if (RubyLanguageBinding.IsRubyFile (file.FilePath)) {
					docs[file] = ProjectDomService.Parse (file.FilePath.FullPath, file.ContentType,
					                                              delegate{ return File.ReadAllText (file.FilePath.FullPath); });
				}
			}
		}
		
		public override void BuildNode (ITreeBuilder treeBuilder,
		                                object dataObject,
		                                ref string label,
		                                ref Gdk.Pixbuf icon,
		                                ref Gdk.Pixbuf closedIcon)
		{
		}

		
		/// <summary>
		/// Push types from parsed documents into the class pad.
		/// </summary>
		public override void BuildChildNodes (ITreeBuilder builder, object dataObject)
		{			
			RubyProject p = dataObject as RubyProject;
			if (p == null) return;
			
			DispatchService.GuiDispatch (delegate () {
				foreach (ProjectFile file in p.Files) {
					foreach (DomType type in docs[file].CompilationUnit.Types) {
						builder.AddChild (type);
					}
				}
			});
		}// BuildChildNodes
		
		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			return true;
		}
		
		private void OnFinishedBuildingTree (ClassPadEventArgs e)
		{
			ITreeBuilder builder = Context.GetTreeBuilder (e.Project);
			if (null != builder)
				builder.UpdateChildren ();
		}
	}// ProjectNodeBuilderExtension
	
	public enum RubyProjectCommands
	{
		UpdateClassPad
	}// RubyProjectCommands
	
	public class ProjectNodeBuilderExtensionHandler : NodeCommandHandler
	{
		[CommandHandler (RubyProjectCommands.UpdateClassPad)]
		public void UpdateClassPad ()
		{
			ProjectNodeBuilderExtension.CreatePadTree (CurrentNode.DataItem);
		}
	}// ProjectNodeBuilderExtensionHandler
	
	public delegate void ClassPadEventHandler (ClassPadEventArgs e);
	
	public class ClassPadEventArgs : EventArgs
	{
		private Project project;
		
		public ClassPadEventArgs (Project project)
		{
			this.project = project;
		}
		
		public Project Project {
			get { return project; }
		}
	}// ClassPadEventArgs
}
