//
// RubyProjectConfiguration.cs: configuration for a RubyProject
//
// Authors:
//  Levi Bard <taktaktaktaktaktaktaktaktaktak@gmail.com> 
//
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
using System.Collections;

using MonoDevelop.Core.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Core.Serialization;

namespace MonoDevelop.RubyBinding
{
	public class RubyProjectConfiguration: ProjectConfiguration
	{
		[ItemProperty("MainFile")]
		public string MainFile{ get; set; }
		
		[ItemProperty ("LoadPaths")]
		[ItemProperty ("LoadPath", Scope = "*", ValueType = typeof(string))]
		public ArrayList LoadPaths{ get; set; }
		
		public RubyProjectConfiguration (): base()
		{
			LoadPaths = new ArrayList ();
		}
		
		public override void CopyFrom (ItemConfiguration configuration)
		{
			try {
				base.CopyFrom (configuration);
				RubyProjectConfiguration conf = (RubyProjectConfiguration)configuration;
				MainFile = conf.MainFile;
				LoadPaths = new ArrayList ();
				if (null != conf.LoadPaths) {
					foreach (object path in conf.LoadPaths) {
						if (!string.IsNullOrEmpty ((string)path)) {
							LoadPaths.Add (path);
						}
					}
				}
			} catch (Exception e) {
				MessageService.ShowException (e);
			}
		}// CopyFrom
	}// RubyProjectConfiguration
}
