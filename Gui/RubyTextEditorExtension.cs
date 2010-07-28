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

using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Ide.CodeCompletion;

namespace MonoDevelop.RubyBinding
{
	public class RubyTextEditorExtension: CompletionTextEditorExtension
	{
		protected virtual string BasePath
		{
			get {
				return (null == Document.Project)? 
				       Document.FileName.FullPath.ParentDirectory: 
				       Document.Project.BaseDirectory.FullPath;
			}
		}
		
		public override bool ExtendsEditor (Document doc, IEditableTextBuffer editor)
		{
			return (Path.GetExtension (doc.FileName).Equals (RubyLanguageBinding.RubyExtension, StringComparison.OrdinalIgnoreCase));
		}
		
		public override ICompletionDataList HandleCodeCompletion (CodeCompletionContext completionContext, char completionChar)
		{
			int triggerWordLength = 0;
			return HandleCodeCompletion (completionContext, completionChar, false, ref triggerWordLength);
		}
		
		public override ICompletionDataList HandleCodeCompletion (CodeCompletionContext completionContext, char completionChar, ref int triggerWordLength)
		{
			return HandleCodeCompletion (completionContext, completionChar, false, ref triggerWordLength);
		}
		
		protected virtual ICompletionDataList HandleCodeCompletion (CodeCompletionContext completionContext, char completionChar, bool forced, ref int triggerWordLength)
		{
			CompletionDataList cdl = new CompletionDataList ();
			string contents = null,
			       symbol = null;
			
			switch (completionChar) {
			case '.':
				// Dot operator
				contents = Editor.Text;
				symbol = RubyCompletion.GetSymbol (contents, completionContext.TriggerOffset-1);
				// Console.WriteLine ("RubyBinding: Completing {0}", symbol);
				if (!string.IsNullOrEmpty (symbol)) {
					CompletionData[] completions = RubyCompletion.Complete (BasePath, contents, symbol, completionContext.TriggerLine-1);
					if (null != completions) {
						// Console.WriteLine ("RubyBinding: Got {0} completions", completions.Length);
						cdl.AddRange (completions);
					}
				}
				break;
			case ':':
				// Scope operator
				if (1 < completionContext.TriggerOffset && ':' == Editor.GetCharAt (completionContext.TriggerOffset-2)) {
					contents = Editor.Text;
					symbol = RubyCompletion.GetSymbol (contents, completionContext.TriggerOffset-2);
					string[] tokens = symbol.Split (new string[]{"::"}, StringSplitOptions.None);
					symbol = (1 < tokens.Length)? string.Join ("::", tokens, 0, tokens.Length-1): tokens[0];
					// Console.WriteLine ("RubyBinding: Completing {0}", symbol);
					if (RubyCompletion.IsConstant (symbol)) {
						CompletionData[] completions = RubyCompletion.Complete (BasePath, contents, symbol, completionContext.TriggerLine-1);
						if (null != completions) {
							// Console.WriteLine ("RubyBinding: Got {0} completions", completions.Length);
							cdl.AddRange (completions);
						}
					}
				}
				break;
			case ' ':
			case '\t':
				string line = Editor.GetLineText (completionContext.TriggerLine);
				
				if ((null == line || (string.IsNullOrEmpty (line.Trim ()))) && !forced) {
					// Don't complete on spacing unless requested
					break;
				}
				
				contents = Editor.Text;
				symbol = RubyCompletion.GetSymbol (contents, completionContext.TriggerOffset-2);
				
				if (0 > Array.IndexOf (RubyCompletion.declarors, symbol.Trim ())) {
					CompletionData[] completions = RubyCompletion.CompleteGlobal (BasePath, contents, completionContext.TriggerLine-1);
					if (null != completions) {
						cdl.AddRange (completions);
					}
				}
				break;
			default:
				// Aggressive completion
				if (char.IsLetter (completionChar)) {
					CompletionData[] completions = RubyCompletion.CompleteGlobal (BasePath, Editor.Text, completionContext.TriggerLine-1);
					cdl.AddRange (completions);
					triggerWordLength = ResetTriggerOffset (completionContext);
				}
				break;
			}
			
			// Zero-length list causes segfault
			return (0 < cdl.Count)? cdl: null;
		}// HandleCodeCompletion
		
		/// <summary>
		/// Move the completion trigger offset to the beginning of the current token
		/// </summary>
		protected virtual int ResetTriggerOffset (CodeCompletionContext completionContext)
		{
			int i = completionContext.TriggerOffset;
			int accumulator = 0;
			
			for (;
			     1 < i && char.IsLetterOrDigit (Editor.GetCharAt (i));
			     --i, ++accumulator);
			completionContext.TriggerOffset = i-1;
			return accumulator+1;
		}// ResetTriggerOffset
		
		public override ICompletionDataList CodeCompletionCommand (CodeCompletionContext completionContext)
		{
			ICompletionDataList completions = null;
			if (RubyLanguageBinding.IsRubyFile (Document.FileName)) {
				int pos = completionContext.TriggerOffset;
				completions = HandleCodeCompletion(completionContext, Editor.Document.GetTextBetween (pos - 1, pos)[0], true, ref pos);
			}
			return completions;
		}// CodeCompletionCommand
		
		public override  IParameterDataProvider HandleParameterCompletion (CodeCompletionContext completionContext, char completionChar)
		{
			ParameterDataProvider pdp = null;
			if (char.IsWhiteSpace (completionChar) || '(' == completionChar) {
				pdp = new ParameterDataProvider (Document, completionContext);
			}
			return (null != pdp && pdp.Valid)? pdp: null;
		}
	}// RubyTextEditorExtension
}
