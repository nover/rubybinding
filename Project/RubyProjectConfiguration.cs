
using System;

using MonoDevelop.Projects;
using MonoDevelop.Core.Serialization;

namespace MonoDevelop.RubyBinding
{
	public class RubyProjectConfiguration: ProjectConfiguration
	{
		[ItemProperty("MainFile")]
		public string MainFile{ get; set; }
	}
}
