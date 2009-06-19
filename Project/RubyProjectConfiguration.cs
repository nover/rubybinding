
using System;

using MonoDevelop.Projects;
using MonoDevelop.Core.Serialization;

namespace MonoDevelop.RubyBinding
{
	public class RubyProjectConfiguration: ProjectConfiguration
	{
		[ItemProperty("MainFile")]
		public string MainFile{ get; set; }
		
		public override void CopyFrom (ItemConfiguration configuration)
		{
			base.CopyFrom (configuration);
			RubyProjectConfiguration conf = (RubyProjectConfiguration)configuration;
			MainFile = conf.MainFile;
		}
	}
}
