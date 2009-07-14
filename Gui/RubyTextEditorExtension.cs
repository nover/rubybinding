//
// RubyTextEditorExtension.cs
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
using System.IO;

using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects.Gui.Completion;

namespace MonoDevelop.RubyBinding
{
	public class RubyTextEditorExtension: CompletionTextEditorExtension
	{
		public override bool ExtendsEditor (Document doc, IEditableTextBuffer editor)
		{
			return (Path.GetExtension (doc.FileName).Equals (RubyLanguageBinding.RubyExtension, StringComparison.OrdinalIgnoreCase));
		}
		
		public override ICompletionDataList HandleCodeCompletion (ICodeCompletionContext completionContext, char completionChar)
		{
			CompletionDataList cdl = new CompletionDataList ();
			if ('.' == completionChar) {
				string contents = Editor.Text,
				       symbol = GetSymbol (contents, completionContext);
				Console.WriteLine ("RubyBinding: Completing {0}", symbol);
				if (!string.IsNullOrEmpty (symbol)) {
					string basepath = (null == Document.Project)? 
						Document.FileName.FullPath.ParentDirectory: 
						Document.Project.BaseDirectory.FullPath;
					ICompletionData[] completions = RubyCompletion.Complete (basepath, contents, symbol, completionContext.TriggerLine-1);
					if (null != completions) {
						Console.WriteLine ("RubyBinding: Got {0} completions", completions.Length);
						cdl.AddRange (completions);
					}
				}
			}
			
			return cdl;
		}
		
		public override ICompletionDataList CodeCompletionCommand (ICodeCompletionContext completionContext)
		{
			if (RubyLanguageBinding.IsRubyFile (Document.FileName)) {
				int pos = completionContext.TriggerOffset;
				return HandleCodeCompletion(completionContext, Editor.GetText (pos - 1, pos)[0]);
			}
			return null;
		}
		
		public override  IParameterDataProvider HandleParameterCompletion (ICodeCompletionContext completionContext, char completionChar)
		{
			return null;
		}
		
		public static readonly char[] wordBreakChars = new char[]{ ' ', '\t', '\r', '\n', '\\', '`', '>', '<', '=', ';', '|', '&', '(' };
		private static string GetSymbol (string contents, ICodeCompletionContext context)
		{
			if (string.IsNullOrEmpty (contents) || 0 == context.TriggerOffset) { 
				Console.WriteLine ("RubyBinding: Empty contents or zero trigger offset {0}", context.TriggerOffset);
				return string.Empty; 
			}
			
			int start = contents.LastIndexOfAny (wordBreakChars, context.TriggerOffset-1)+1,
			    end = contents.IndexOfAny (wordBreakChars, context.TriggerOffset-1)-1;
			
			if (0 > start){ start = 0; }
			if (0 > end){ end = contents.Length; }
			if (end < start){ end = start; }
			
			Console.WriteLine ("RubyBinding: Start {0}, End {1}", start, end);
			
			return contents.Substring (start, end-start);
		}
	}
}
