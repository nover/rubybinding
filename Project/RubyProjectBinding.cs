//
// RubyProjectBinding.cs: binding for a RubyProject
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

using MonoDevelop.Projects;

namespace MonoDevelop.RubyBinding
{
	public class RubyProjectBinding: IProjectBinding
	{
		#region IProjectBinding implementation
		
		public string Name {
			get { return RubyLanguageBinding.RubyLanguage; }
		}
		
		public bool CanCreateSingleFileProject (string sourceFile)
		{
			return RubyLanguageBinding.IsRubyFile (sourceFile);
		}
		
		public Project CreateProject (ProjectCreateInformation info, System.Xml.XmlElement projectOptions)
		{
			return new RubyProject (info, projectOptions, RubyLanguageBinding.RubyLanguage);
		}
		
		public Project CreateSingleFileProject (string sourceFile)
		{
			ProjectCreateInformation info = new ProjectCreateInformation ();
			info.ProjectName = Path.GetFileNameWithoutExtension (sourceFile);
			info.CombinePath = Path.GetDirectoryName (sourceFile);
			info.ProjectBasePath = Path.GetDirectoryName (sourceFile);
			
			Project project =  new RubyProject (info, null, RubyLanguageBinding.RubyLanguage);
			project.Files.Add (new ProjectFile (sourceFile));
			return project;
		}
		
		#endregion
	}
}
