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
