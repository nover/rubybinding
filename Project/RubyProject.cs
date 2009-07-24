//
// RubyProject.cs: Ruby Project
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
using System.Xml;

using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;

namespace MonoDevelop.RubyBinding
{
	public class RubyProject: Project
	{
		public override string ProjectType {
			get { return RubyLanguageBinding.RubyLanguage; }
		}
		
		public override string[] SupportedLanguages {
			get { return new string[] { RubyLanguageBinding.RubyLanguage }; }
		}
		
		public RubyProject () {}
		
		public RubyProject (ProjectCreateInformation info,
						 XmlElement projectOptions, string language)
		{
			if (info != null) {
				Name = info.ProjectName;
			}
			Configurations.Add (CreateConfiguration ("Default"));
			
		}
		
		public override SolutionItemConfiguration CreateConfiguration (string name)
		{
			RubyProjectConfiguration conf = new RubyProjectConfiguration ();
			conf.Name = name;
			return conf;
		}
		
		public override bool IsCompileable (string fileName)
		{
			return false;
		}
		
		protected override bool OnGetCanExecute (MonoDevelop.Projects.ExecutionContext context, string solutionConfiguration)
		{
			return true;
		}
		
		protected override void DoExecute (IProgressMonitor monitor,
		                                   ExecutionContext context,
		                                   string configuration)
		{
			RubyProjectConfiguration conf = (RubyProjectConfiguration)GetConfiguration (configuration);
			bool pause = conf.PauseConsoleOutput;
			IConsole console = (conf.ExternalConsole? context.ExternalConsoleFactory: context.ConsoleFactory).CreateConsole (!pause);
			
			ExecutionCommand cmd = new NativeExecutionCommand (RubyLanguageBinding.RubyInterpreter, conf.MainFile);
			
			monitor.Log.WriteLine ("Running {0} {1}", RubyLanguageBinding.RubyInterpreter, conf.MainFile);
			
			AggregatedOperationMonitor operationMonitor = new AggregatedOperationMonitor (monitor);
			
			try {
				if (!context.ExecutionHandler.CanExecute (cmd)) {
					monitor.ReportError (string.Format ("Cannot execute {0}.", conf.MainFile), null);
					return;
				}
				
				IProcessAsyncOperation op = context.ExecutionHandler.Execute (cmd, console);
				
				operationMonitor.AddOperation (op);
				op.WaitForCompleted ();
				
				monitor.Log.WriteLine ("The operation exited with code: {0}", op.ExitCode);
			} catch (Exception ex) {
				monitor.ReportError (string.Format ("Cannot execute {0}.", conf.MainFile), ex);
			} finally {			
				operationMonitor.Dispose ();			
				console.Dispose ();
			}
		}// DoExecute
	}// RubyProject
}
