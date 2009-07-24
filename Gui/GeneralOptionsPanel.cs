//
// GeneralOptionsPanel.cs
//
// Authors:
//  Levi Bard <taktaktaktaktaktaktaktaktaktak@gmail.com> 
//
// Copyright (C) 2009 Levi Bard
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 


using System;
using Gtk;

using MonoDevelop.Projects;
using MonoDevelop.Projects.Gui.Dialogs;
using MonoDevelop.Core.Gui.Dialogs;

namespace MonoDevelop.RubyBinding
{
	/// <summary>
	/// Panel for Ruby-specific options
	/// </summary>
	/// <remarks>
	/// Currently limited to startup selection
	/// </remarks>
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
