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
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;

namespace MonoDevelop.RubyBinding
{
	public class RubyDocumentParser: AbstractParser
	{
		/// <summary>
		/// Keywords that increase the scope / require an 'end'
		/// </summary>
		static string[] scopeBeginners = new string[] {
			"begin", "case", "do", "for", "if",  "module", "while", "unless", "until"
		};
		static Regex methodDefinition = new Regex (@"^\s*def\s+([\w:][\w\d:]*\.)?(?<name>[\w\*\+=></^&\|%~\?\!\]\[-][\w\d\*\+=></^&\|%~\?\!\]\[-]*)", RegexOptions.Compiled);
		static Regex classDefinition = new Regex (@"^\s*class\s+([A-Z][\w\d]*::)?(?<name>[A-Z][\w\d]*)", RegexOptions.Compiled);
		static Regex doEndBlock = new Regex (@"[^\w\d]do\s*\|[^\|]+\|(?<end>[^\w\d]end(\s|$))?", RegexOptions.Compiled);
		
		Dictionary<string,ParsedDocument> successfulParses;
		Dictionary<int,RubyDeclaration> methods;
		Dictionary<int,RubyDeclaration> classes;
		
		public RubyDocumentParser (): base (RubyLanguageBinding.RubyLanguage, "text/x-ruby")
		{
			successfulParses = new Dictionary<string, ParsedDocument> ();
		}
		
		public override bool CanParse (string fileName)
		{
			return RubyLanguageBinding.IsRubyFile (fileName);
		}
		
		public override ParsedDocument Parse (MonoDevelop.Projects.Dom.Parser.ProjectDom dom, string fileName, string content)
		{
			string basepath = (null == dom || null == dom.Project)?
				Path.GetDirectoryName (fileName): 
				(string)dom.Project.BaseDirectory.FullPath;
			List<Error> errors = RubyCompletion.CheckForErrors (basepath, content);
			
			if (null != errors && 0 < errors.Count) {
				ParsedDocument doc = successfulParses.ContainsKey (fileName)? 
				    successfulParses[fileName]: 
				    new ParsedDocument (fileName);
				foreach (Error err in errors) {
					Console.WriteLine ("RubyDocumentParser: Error {0}:{1} {2}", fileName, err.Region.Start.Line, err.Message);
					doc.Add (err); 
				}
				return doc;
			}
				
			lock (this) {
				string[] lines = content.Split (new string[]{Environment.NewLine}, StringSplitOptions.None);
				
				methods = new Dictionary<int, RubyDeclaration> ();
				classes = new Dictionary<int, RubyDeclaration> ();
				
				if (!RunStack (lines)) {
					return successfulParses.ContainsKey (fileName)? successfulParses[fileName]: new ParsedDocument (fileName);
				}
				
				ParsedDocument doc = new ParsedDocument (fileName);
				if(null == doc.CompilationUnit){ doc.CompilationUnit = new CompilationUnit (fileName); }
				CompilationUnit cu = (CompilationUnit)doc.CompilationUnit;
				
				PopulateClasses (cu);
				
				if (0 < methods.Count) {
					DomType glob = new DomType (cu, ClassType.Unknown, GettextCatalog.GetString ("(Global Methods)"), new DomLocation (1, 1), string.Empty, new DomRegion (1, lines.Length), new List<IMember> ());
					PopulateMethods (glob);
					cu.Add (glob);
				}// Add global methods
				
				return successfulParses[fileName] = doc;
			}
		}
		
		/// <summary>
		/// Populate methods and classes dictionaries from content lines
		/// </summary>
		/// <param name="contentLines">
		/// A <see cref="System.String[]"/>: Each line of file content
		/// </param>
		/// <remarks>
		/// Maintain a stack of lines that increase the scope, 
		/// then pop as the scope is decreased.
		/// Use scope begin/end for region/fold population.
		/// </remarks>
		/// <returns>
		/// A <see cref="System.Boolean"/>: Success/failure
		/// </returns>
		bool RunStack (string[] contentLines)
		{
			Stack<KeyValuePair<int,RubyDeclaration>> stack = new Stack<KeyValuePair<int,RubyDeclaration>> ();
			int i = 1;
			Match match;
			
			foreach (string aline in contentLines) {
				string line = aline.Trim ();
				if (line.StartsWith ("end", StringComparison.Ordinal) && 
				    (3 == line.Length || !char.IsLetterOrDigit (line[3]))) { 
					if (0 == stack.Count){ 
						Console.WriteLine ("RubyDocumentParser: Popping empty stack at {0}", i);
						return false; 
					}// stack imbalance
					
					KeyValuePair<int,RubyDeclaration> poppedPair = stack.Pop ();
					if (!string.IsNullOrEmpty (poppedPair.Value.name)) {
						// Console.WriteLine ("Got {0} {1} from {2} to {3}", methods.ContainsKey(poppedPair.Key)? "method": "class", poppedPair.Value, poppedPair.Key, i);
						if (methods.ContainsKey (poppedPair.Key)) {
							methods[poppedPair.Key].end = i;
						}// end of method definition
						if (classes.ContainsKey (poppedPair.Key)) {
							classes[poppedPair.Key].end = i;
						}// end of class definition
					}// only care about method and class definitions
				}// Scope decrease
				
				foreach (string sb in scopeBeginners) {
					if (line.StartsWith (sb, StringComparison.Ordinal) && !line.EndsWith ("end", StringComparison.Ordinal) &&
					    (line.Length == sb.Length || char.IsWhiteSpace (line[sb.Length]) || char.IsPunctuation (line[sb.Length]))) {
						stack.Push (new KeyValuePair<int,RubyDeclaration> (i, new RubyDeclaration (i, i, string.Empty, line)));
						break;
					}
				}// check for unimportant scope increase
				
				if (null != (match = doEndBlock.Match (line)) && match.Success && !match.Groups["end"].Success) {
					stack.Push (new KeyValuePair<int,RubyDeclaration> (i, new RubyDeclaration (i, i, string.Empty, line)));
				}// check for do/end-scoped block with inline do
				else if (null != (match = methodDefinition.Match (line)) && match.Success) {
					RubyDeclaration method = methods[i] = new RubyDeclaration (i, i, match.Groups["name"].Value, line);
					stack.Push (new KeyValuePair<int,RubyDeclaration> (i, method));
				}// begin method definition
				else if (null != (match = classDefinition.Match (line)) && match.Success) {
					RubyDeclaration klass = classes[i] = new RubyDeclaration (i, i, match.Groups["name"].Value, line);
					stack.Push (new KeyValuePair<int,RubyDeclaration> (i, klass));
				}// begin class definition
				++i;
			}// parse line
			
			if (0 < stack.Count) { 
				Console.WriteLine ("RubyDocumentParser: {0} extra items on stack", stack.Count);
			}// stack imbalance
			
			return (0 == stack.Count);
		}
		
		/// <summary>
		/// Populate a compilation unit with classes
		/// </summary>
		void PopulateClasses (CompilationUnit cu)
		{
			foreach (KeyValuePair<int,RubyDeclaration> pair in classes) {
				DomType myclass = new DomType (cu, ClassType.Class, pair.Value.name, new DomLocation (pair.Value.begin, 1), string.Empty, 
				                               new DomRegion (pair.Value.begin+1, pair.Value.end+1), new List<IMember> ());
				PopulateMethods (myclass);
				cu.Add (myclass);
			}// Add classes and member methods
		}
		
		/// <summary>
		/// Populate a DomType with methods
		/// </summary>
		void PopulateMethods (DomType parent)
		{
			List<int> removal = new List<int> ();
			
			foreach (KeyValuePair<int,RubyDeclaration> mpair in methods) {
				if (mpair.Key > parent.Location.Line && mpair.Key < parent.BodyRegion.End.Line) {
					parent.Add (new DomMethod (mpair.Value.name, Modifiers.None, MethodModifier.None, new DomLocation (mpair.Value.begin, 1), 
					                           new DomRegion (mpair.Value.begin+1, mpair.Value.end+1)));
					removal.Add (mpair.Key);
				}// Add methods that are declared within the parent's scope
			}// Check detected methods
			
			// Remove used methods from map
			foreach (int key in removal){ methods.Remove (key); }
		}
		
		/// <summary>
		/// Helper class for declarations
		/// </summary>
		class RubyDeclaration {
			public int begin;
			public int end;
			public string name;
			public string declaration;
			
			public RubyDeclaration (int begin, int end, string name, string declaration) {
				this.begin = begin;
				this.end = end;
				this.name = name;
				this.declaration = declaration;
			}
		}
	}
}
