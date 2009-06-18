//
// RubyProject.cs: Ruby Project
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
		}
		
		public override bool IsCompileable (string fileName)
		{
			return false;
		}
		
		protected override bool OnGetCanExecute (MonoDevelop.Projects.ExecutionContext context, string solutionConfiguration)
		{
			return true;
//			System.Diagnostics.Process p = System.Diagnostics.Process.Start (RubyLanguageBinding.RubyInterpreter, "--version");
//			p.WaitForExit (5000);
//			return (0 == p.ExitCode);
		}
		
		protected override void DoExecute (IProgressMonitor monitor,
										   ExecutionContext context,
		                                   string configuration)
		{
			RubyProjectConfiguration conf = (RubyProjectConfiguration)GetConfiguration (configuration);
			bool pause = conf.PauseConsoleOutput;
			IConsole console = (conf.ExternalConsole? context.ExternalConsoleFactory: context.ConsoleFactory).CreateConsole (!pause);
			
			ExecutionCommand cmd = new NativeExecutionCommand (RubyLanguageBinding.RubyInterpreter, conf.MainFile);
			
			monitor.Log.WriteLine ("Running project...");
			
			if (conf.ExternalConsole)
				console = context.ExternalConsoleFactory.CreateConsole (!pause);
			else
				console = context.ConsoleFactory.CreateConsole (!pause);
			
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
		}
	}
}
