//
// GeneralOptionsPanel.cs
//
// Authors:
//  Levi Bard <taktaktaktaktaktaktaktaktaktak@gmail.com> 
//
// Copyright (C) 2009 Levi Bard
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
using Gtk;

using MonoDevelop.Projects;
using MonoDevelop.Projects.Gui.Dialogs;
using MonoDevelop.Core.Gui.Dialogs;

namespace MonoDevelop.RubyBinding
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeneralOptionsPanel : Gtk.Bin
	{
		RubyProjectConfiguration config;
		
		public GeneralOptionsPanel()
		{
			this.Build();
		}
		
		public GeneralOptionsPanel (RubyProject project, RubyProjectConfiguration config): this ()
		{
			Load (project, config);
		}
		
		public bool Load (RubyProject project, RubyProjectConfiguration config)
		{
			int found = 0,
			    count = 0;
			if (null == config || null == project){ return false; }
			this.config = config;
			
			foreach (ProjectFile pf in project.Files) {
				projectFilesCB.AppendText (pf.Name);
				if (pf.Name.Equals (config.MainFile, StringComparison.OrdinalIgnoreCase)) { 
					found = count;
				}
				++count;
			}
			projectFilesCB.Active = found;
			
			return true;
		}
		
		public bool Store ()
		{
			if (null != config) {
				config.MainFile = projectFilesCB.ActiveText;
				return true;
			}
			return false;
		}
	}
    
	public class GeneralOptionsPanelBinding : MultiConfigItemOptionsPanel
	{
		private GeneralOptionsPanel panel;
		
		public override Gtk.Widget CreatePanelWidget ()
		{
			return panel = new GeneralOptionsPanel ();
		}
		
		public override void LoadConfigData ()
		{
			panel.Load ((RubyProject)ConfiguredProject, (RubyProjectConfiguration)CurrentConfiguration);
			panel.ShowAll ();
		}
		
		public override void ApplyChanges ()
		{
			panel.Store ();
		}
	}
}
