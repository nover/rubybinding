// 
//  Copyright (C) 2009 Levi Bard <taktaktaktaktaktaktaktaktaktak@gmail.com> 
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;
using System.Text;

using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects.Gui.Completion;

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
		
		public ParameterDataProvider (Document doc, ICodeCompletionContext context)
		{
			this.doc = doc;
			
			string basepath = (null == doc.Project)? 
				doc.FileName.FullPath.ParentDirectory: 
				doc.Project.BaseDirectory.FullPath;
			string contents = doc.TextEditor.Text;
			string line = doc.TextEditor.GetLineText (context.TriggerLine);
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
		}// constructor

		#region IParameterDataProvider implementation
		
		public int GetCurrentParameterIndex (ICodeCompletionContext ctx)
		{
			TextEditor editor = doc.TextEditor;
			int cursor = editor.CursorPosition;
			int i = ctx.TriggerOffset;
			
			if (i > cursor)
				return -1;
			else if (i == cursor)
				return 1;
			
			int parameterIndex = 1;
			
			while (i++ < cursor) {
				char ch = editor.GetCharAt (i-1);
				if (ch == ',')
					parameterIndex++;
				else if (ch == ')')
					return -1;
			}
			
			return (methodParams.Length >= parameterIndex)? parameterIndex: -1;
		}// GetCurrentParameterIndex
		
		public string GetMethodMarkup (int overload, string[] parameterMarkup)
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
