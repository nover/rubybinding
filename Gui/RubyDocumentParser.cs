//
// RubyDocumentParser.cs
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;

namespace MonoDevelop.RubyBinding
{
	public class RubyDocumentParser: AbstractParser
	{
		static string[] scopeBeginners = new string[] {
			"begin", "case", "do", "for", "if",  "module", "while", "unless", "until"
		};
		static Regex methodDefinition = new Regex (@"^\s*def\s+([\w:][\w\d:]*\.)?(?<name>\w[\w\d]*)", RegexOptions.Compiled);
		static Regex classDefinition = new Regex (@"^\s*class\s+([A-Z][\w\d]*::)?(?<name>[A-Z][\w\d]*)", RegexOptions.Compiled);
		ParsedDocument lastSuccessfulParse;
		
		public RubyDocumentParser (): base (RubyLanguageBinding.RubyLanguage, "text/x-ruby")
		{
		}
		
		public override bool CanParse (string fileName)
		{
			return RubyLanguageBinding.IsRubyFile (fileName);
		}
		
		public override ParsedDocument Parse (MonoDevelop.Projects.Dom.Parser.ProjectDom dom, string fileName, string content)
		{
			Stack<KeyValuePair<int,string>> stack = new Stack<KeyValuePair<int,string>> ();
			Dictionary<int,string> methods = new Dictionary<int, string> ();
			Dictionary<int,string> classes = new Dictionary<int, string> ();
			int i = 1;
			Match match;
			string[] lines = content.Split (new string[]{Environment.NewLine}, StringSplitOptions.None);
			
			foreach (string aline in lines) {
				string line = aline.Trim ();
				if (line.StartsWith ("end", StringComparison.Ordinal) && 
				    (3 == line.Length ||  char.IsWhiteSpace (line[3]) || char.IsPunctuation (line[3]))) {
					if (0 == stack.Count){ 
						Console.WriteLine ("RubyDocumentParser: Popping empty stack at {0}:{1}", fileName, i);
						return lastSuccessfulParse; 
					}// punt
					
					KeyValuePair<int,string> poppedPair = stack.Pop ();
					if (!string.IsNullOrEmpty (poppedPair.Value)) {
						// Console.WriteLine ("Got {0} {1} from {2} to {3}", methods.ContainsKey(poppedPair.Key)? "method": "class", poppedPair.Value, poppedPair.Key, i);
						if (methods.ContainsKey (poppedPair.Key)) {
							methods[poppedPair.Key] = string.Format("{0}|{1}|{2}", poppedPair.Value, poppedPair.Key, i);
						}
						if (classes.ContainsKey (poppedPair.Key)) {
							classes[poppedPair.Key] = string.Format("{0}|{1}|{2}", poppedPair.Value, poppedPair.Key, i);
						}
					}
				}
				foreach (string sb in scopeBeginners) {
					if (line.StartsWith (sb, StringComparison.Ordinal) && !line.EndsWith ("end", StringComparison.Ordinal) &&
					    (line.Length == sb.Length || char.IsWhiteSpace (line[sb.Length]) || char.IsPunctuation (line[sb.Length]))) {
						stack.Push (new KeyValuePair<int,string> (i, string.Empty));
						break;
					}
				}
				if (null != (match = methodDefinition.Match (line)) && match.Success) {
					stack.Push (new KeyValuePair<int,string> (i, match.Groups["name"].Value));
					methods[i] = match.Groups["name"].Value;
				}
				if (null != (match = classDefinition.Match (line)) && match.Success) {
					stack.Push (new KeyValuePair<int,string> (i, match.Groups["name"].Value));
					classes[i] = match.Groups["name"].Value;
				}
				++i;
			}// actually parse document
			
			// Make sure the stack is empty
			if (0 < stack.Count) { 
				Console.WriteLine ("RubyDocumentParser: {0}: {1} extra items on stack", fileName, stack.Count);
				return lastSuccessfulParse;
			}
			
			ParsedDocument doc = new ParsedDocument (fileName);
			if(null == doc.CompilationUnit){ doc.CompilationUnit = new CompilationUnit (fileName); }
			CompilationUnit cu = (CompilationUnit)doc.CompilationUnit;
			
			foreach (KeyValuePair<int,string> pair in classes) {
				string[] tokens = pair.Value.Split ('|');
				string name = tokens[0];
				int start = int.Parse (tokens[1]);
				int end = int.Parse (tokens[2]);
				List<int> removal = new List<int> ();
				DomType myclass = new DomType (cu, ClassType.Class, name, new DomLocation (start, 1), string.Empty, new DomRegion (start+1, end+1), new List<IMember> ());
				
				foreach (KeyValuePair<int,string> mpair in methods) {
					if (mpair.Key > start && mpair.Key < end) {
						string[] mtokens = mpair.Value.Split ('|');
						string mname = mtokens[0];
						int mstart = int.Parse (mtokens[1]);
						int mend = int.Parse (mtokens[2]);
						myclass.Add (new DomMethod (mname, Modifiers.None, "initialize".Equals (mname, StringComparison.Ordinal)? MethodModifier.IsConstructor: MethodModifier.None, new DomLocation (mstart, 1), new DomRegion (mstart+1, mend+1)));
						removal.Add (mpair.Key);
					}
				}
				foreach (int key in removal){ methods.Remove (key); }
				
				cu.Add (myclass);
			}// Add classes and member methods first
			
			if (0 < methods.Count) {
				DomType glob = new DomType (cu, ClassType.Unknown, "(Global Methods)", new DomLocation (1, 1), string.Empty, new DomRegion (1, lines.Length), new List<IMember> ());
				
				foreach (KeyValuePair<int,string> mpair in methods) {
					string[] mtokens = mpair.Value.Split ('|');
					string mname = mtokens[0];
					int mstart = int.Parse (mtokens[1]);
					int mend = int.Parse (mtokens[2]);
					glob.Add (new DomMethod (mname, Modifiers.None, MethodModifier.None, new DomLocation (mstart, 1), new DomRegion (mstart+1, mend+1)));
				}
				cu.Add (glob);
			}// Add global methods
			
			return lastSuccessfulParse = doc;
		}
	}
}
