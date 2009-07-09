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
			"begin", "case", "do", "for", "while", "unless", "until", "module"
		};
		static Regex methodDefinition = new Regex (@"^\s*def\s+([\w:][\w\d:]*\.)?(?<name>\w[\w\d]*)", RegexOptions.Compiled);
		static Regex classDefinition = new Regex (@"^\s*class\s+([A-Z][\w\d]*::)?(?<name>[A-Z][\w\d]+)", RegexOptions.Compiled);
		
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
			int i = 1;
			Match match;
			
			foreach (string aline in content.Split (new string[]{Environment.NewLine}, StringSplitOptions.None)) {
				string line = aline.TrimStart ();
				if (line.StartsWith ("end", StringComparison.Ordinal) && 
				    (3 == line.Length ||  char.IsWhiteSpace (line[3]) || char.IsPunctuation (line[3]))) {
					if (0 == stack.Count){ return null; } // punt
					KeyValuePair<int,string> poppedPair = stack.Pop ();
					if (!string.IsNullOrEmpty (poppedPair.Value)) {
						Console.WriteLine ("Got {0} from {1} to {2}", poppedPair.Value, poppedPair.Key, i);
					}
				}
				foreach (string sb in scopeBeginners) {
					if (line.StartsWith (sb, StringComparison.Ordinal) && 
					    (line.Length == sb.Length || char.IsWhiteSpace (line[sb.Length]) || char.IsPunctuation (line[sb.Length]))) {
						stack.Push (new KeyValuePair<int,string> (i, string.Empty));
						break;
					}
				}
				if (null != (match = methodDefinition.Match (line)) && match.Success) {
					stack.Push (new KeyValuePair<int,string> (i, match.Groups["name"].Value));
				}
				if (null != (match = classDefinition.Match (line)) && match.Success) {
					stack.Push (new KeyValuePair<int,string> (i, match.Groups["name"].Value));
				}
				++i;
			}
		    throw new System.NotImplementedException ();
		}
	}
}
