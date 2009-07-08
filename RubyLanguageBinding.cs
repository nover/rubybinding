//
// RubyLanguageBinding.cs
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

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Projects.CodeGeneration;

namespace MonoDevelop.RubyBinding
{
	public class RubyLanguageBinding: ILanguageBinding
	{
		public static readonly string RubyLanguage = "Ruby";
		public static readonly string RubyExtension = ".rb";
		public static readonly string RubyInterpreter = "ruby";

		#region ILanguageBinding implementation
		
		public string BlockCommentEndTag {
			get { return "=end"; }
		}
		
		public string BlockCommentStartTag {
			get { return "=begin"; }
		}
		
		public string Language {
			get { return RubyLanguage; }
		}
		
		public IParser Parser {
			get { return null; }
		}
		
		public IRefactorer Refactorer {
			get { return null; }
		}
		
		public string SingleLineCommentTag {
			get { return "#"; }
		}
		
		public string GetFileName (string fileNameWithoutExtension)
		{
			return string.Format ("{0}{1}", fileNameWithoutExtension, RubyExtension);
		}
		
		public bool IsSourceCodeFile (string fileName)
		{
			return IsRubyFile (fileName);
		}
		
		public static bool IsRubyFile (string fileName)
		{
			return Path.GetExtension (fileName).Equals (RubyExtension, StringComparison.OrdinalIgnoreCase);
		}
		
		#endregion
	}
}
