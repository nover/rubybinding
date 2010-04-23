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
using System.IO;
using System.Collections;
using Gtk;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;
using MonoDevelop.Projects;

namespace MonoDevelop.RubyBinding
{
	/// <summary>
	/// Panel for Ruby-specific runtime options
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeneralOptionsPanel : Gtk.Bin
	{
		RubyProjectConfiguration config;
		Gtk.ListStore loadpathStore = new Gtk.ListStore (typeof(string));
		
		public GeneralOptionsPanel()
		{
			this.Build();
			
			Gtk.CellRendererText textRenderer = new Gtk.CellRendererText ();
			
			loadpathTreeView.Model = loadpathStore;
			loadpathTreeView.HeadersVisible = false;
			loadpathTreeView.AppendColumn ("Load Path", textRenderer, "text", 0);
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
			foreach (object path in config.LoadPaths) {
				if (!string.IsNullOrEmpty ((string)path)) {
					loadpathStore.AppendValues (path);
				}
			}
			
			return true;
		}
		
		public bool Store ()
		{
			if (null == config) { return false; }
			
			TreeIter iter;
			string line;
			
			config.MainFile = projectFilesCB.ActiveText;
			config.LoadPaths.Clear ();
			for (loadpathStore.GetIterFirst (out iter);
			     loadpathStore.IterIsValid (iter);
			     loadpathStore.IterNext (ref iter)) {
				line = (string)loadpathStore.GetValue (iter, 0);
				if (!string.IsNullOrEmpty (line)){ config.LoadPaths.Add (line); }
			}
			return true;
		}

		protected virtual void loadpathAddEntryChanged (object sender, System.EventArgs e)
		{
			addLoadpathButton.Sensitive = Directory.Exists (loadpathAddEntry.Text);
		}

		protected virtual void loadpathAddButtonClicked (object sender, System.EventArgs e)
		{
			string path = loadpathAddEntry.Text;
			if (!string.IsNullOrEmpty (path)) {
				loadpathStore.AppendValues (path);
				loadpathAddEntry.Text = string.Empty;
			}
		}

		protected virtual void browseButtonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fcd = new FileChooserDialog (GettextCatalog.GetString ("Choose Load Path"), null, FileChooserAction.SelectFolder, 
			                                               Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Open, Gtk.ResponseType.Ok);
			try {
				if (fcd.Run() == (int)ResponseType.Ok) {
					if (!string.IsNullOrEmpty (fcd.Filename)) {
						loadpathStore.AppendValues (fcd.Filename);
					}
				}
			} finally {
				fcd.Destroy ();
			}
		}

		protected virtual void removeLoadpathButtonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			if (loadpathTreeView.Selection.GetSelected (out iter)) {
				loadpathStore.Remove (ref iter);
			}
			loadpathTreeViewCursorChanged (sender, e);
		}

		protected virtual void loadpathTreeViewCursorChanged (object sender, System.EventArgs e)
		{
			removeLoadpathButton.Sensitive = (null != loadpathTreeView.Selection && 
			                                  0 < loadpathTreeView.Selection.CountSelectedRows ());
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
