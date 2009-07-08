//
// RubyTextEditorExtension.cs
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
			Console.WriteLine ("RubyBinding: HandleCodeCompletion: '{0}'", completionChar);
			
			CompletionDataList cdl = new CompletionDataList ();
			if ('.' == completionChar) {
				string contents = Editor.Text,
				       symbol = GetSymbol (contents, completionContext);
				Console.WriteLine ("RubyBinding: Completing {0}", symbol);
				if (!string.IsNullOrEmpty (symbol)) {
					ICompletionData[] completions = RubyCompletion.CompleteSymbol (contents, symbol, completionContext.TriggerLine-1);
					if (null != completions) {
						Console.WriteLine ("RubyBinding: Got {0} completions", completions.Length);
						cdl.AddRange (completions);
					}
				}
			}
			Console.WriteLine ("RubyBinding: Returning {0} completions", cdl.Count);
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
