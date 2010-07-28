// 
//  Copyright (C) 2009 Levi Bard <taktaktaktaktaktaktaktaktaktak@gmail.com> 
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
using System.Text;

using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.CodeCompletion;

namespace MonoDevelop.RubyBinding
{
	/// <summary>
	/// Provides method parameter data for tooltip
	/// </summary>
	public class ParameterDataProvider: IParameterDataProvider
	{
		Document doc;
		string method;
		string owner;
		string[] methodParams;
		
		public bool Valid{ get; protected set; }
		
		public ParameterDataProvider (Document doc, CodeCompletionContext context)
		{
			this.doc = doc;
			
			string basepath = (null == doc.Project)? 
				doc.FileName.FullPath.ParentDirectory: 
				doc.Project.BaseDirectory.FullPath;
			string contents = doc.Editor.Text;
			string line = doc.Editor.GetLineText (context.TriggerLine);
			method = RubyCompletion.GetSymbol (contents, context.TriggerOffset-1);
			int methodIndex = line.IndexOf (method, 0);
			
			// Console.WriteLine ("HandleParameterCompletion ({0} {1})", line, methodIndex);
			
			if (0 < methodIndex) {
				int end = line.LastIndexOfAny (RubyCompletion.wordBreakChars, methodIndex);
				int start = -1;
				if (0 < end) {
					start = line.LastIndexOfAny (RubyCompletion.wordBreakChars, end-1)+1;
					if (0 <= start) {
						owner = line.Substring (start, end-start);
					}
				}
				// Console.WriteLine ("Owner: {0}-{1}", start, end);
			}
			
			if ("def".Equals (owner, StringComparison.Ordinal) || 0 <= Array.IndexOf (RubyCompletion.reservedWords, method)) {
				method = owner = string.Empty;
				methodParams = new string[0];
				return;
			}
			
			methodParams = RubyCompletion.GetMethodArguments (basepath, contents, context.TriggerLine-1, owner, method);
			Valid = (null != methodParams);
		}// constructor

		#region IParameterDataProvider implementation
		
		public int GetCurrentParameterIndex (ICompletionWidget w, CodeCompletionContext ctx)
		{
			int cursor = doc.Editor.Caret.Offset;
			int i = ctx.TriggerOffset;
			
			if (i > cursor)
				return -1;
			else if (i == cursor)
				return 1;
			
			int parameterIndex = 1;
			
			while (i++ < cursor) {
				char ch = doc.Editor.Document.GetCharAt (i-1);
				if (ch == ',')
					parameterIndex++;
				else if (ch == ')')
					return -1;
			}
			
			return (methodParams.Length >= parameterIndex)? parameterIndex: -1;
		}// GetCurrentParameterIndex
		
		public string GetMethodMarkup (int overload, string[] parameterMarkup, int currentParameter)
		{
			return string.IsNullOrEmpty (method)? null: string.Format ("<b>{0}</b>({1})", GLib.Markup.EscapeText (method), string.Join (", ", parameterMarkup));
		}
		
		public int GetParameterCount (int overload)
		{
			return methodParams.Length;
		}
		
		public string GetParameterMarkup (int overload, int paramIndex)
		{
			return (paramIndex >= methodParams.Length)? string.Empty: methodParams[paramIndex];
		}
		
		public int OverloadCount {
			get { return 1; }
		}
		
		#endregion
	
	}// ParameterDataProvider
}
