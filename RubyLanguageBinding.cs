//
// RubyLanguageBinding.cs
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
