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
using System.IO;
using System.Collections;
using System.Collections.Generic;

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
			if (string.IsNullOrEmpty (conf.MainFile) && 0 < Files.Count) {
				foreach (ProjectFile file in Files) {
					if (RubyLanguageBinding.IsRubyFile (file.Name)) {
						conf.MainFile = file.FilePath.FullPath;
					}
				}
			}
			return conf;
		}
		
		public override bool IsCompileable (string fileName)
		{
			return false;
		}
		
		protected override bool OnGetCanExecute (MonoDevelop.Projects.ExecutionContext context, string solutionConfiguration)
		{
			RubyProjectConfiguration conf = GetConfiguration (solutionConfiguration) as RubyProjectConfiguration;
			return (null != conf && !string.IsNullOrEmpty (conf.MainFile));
		}
		
		protected override void DoExecute (IProgressMonitor monitor,
		                                   ExecutionContext context,
		                                   string configuration)
		{
			RubyProjectConfiguration conf = (RubyProjectConfiguration)GetConfiguration (configuration);
			bool pause = conf.PauseConsoleOutput;
			IConsole console = (conf.ExternalConsole? context.ExternalConsoleFactory: context.ConsoleFactory).CreateConsole (!pause);
			List<string> loadPaths = new List<string> ();
			loadPaths.Add (BaseDirectory.FullPath);
			foreach (object path in conf.LoadPaths) {
				if (!string.IsNullOrEmpty ((string)path)){ loadPaths.Add ((string)path); }
			}
 			
			ExecutionCommand cmd = new NativeExecutionCommand (RubyLanguageBinding.RubyInterpreter, conf.MainFile, BaseDirectory, 
			                                                   new Dictionary<string,string>(){{"RUBYLIB", string.Join (Path.DirectorySeparatorChar.ToString(), loadPaths.ToArray ()) }});
			
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
