// RubyCompletion.cs
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
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MonoDevelop.Core.Gui;
using MonoDevelop.Projects.Gui.Completion;

namespace MonoDevelop.RubyBinding
{
	public class RubyCompletion
	{
#if false
		 * Types of symbols:
		 * String literal ("string" or 'string') - ^(("(\\"|[^"])*")|('[^']*'))$
		 * Numeric literal (10, 0x10) - ^([\d\.]+|0x[a-fA-F\d]+)$
		 * Array literal (['array','literal']) - ^\[.*\]$
		 * Proc - (blah(){ |foo| blah }) - ^{.*}$
		 * Hash literal ({'hash'=>'literal'}) (could also be proc) - ^{.*}$
		 * Regex literal (/regex.literal/) - ^/(\\/|[^/])*/$
		 * Constant/class/module (StartsWithCapitalLetter) - ^[A-Z][\w\d]*$
		 * Symbol (:symbol) - ^:\w[\w\d]*
		 * Instance field (@instance_field) - ^@\w[\w\d]*$
		 * Reserved word - (list)
		 * Operator - (list)
		 * Variables/method calls/everything elsse
#endif
		static RubyCompletion () {
			Console.WriteLine ("Initializing RubyCompletion");
			string scriptname = "monodevelop_ruby_parser";
			ruby_init ();
			ruby_script (scriptname);
			ruby_set_argv (1, new string[]{scriptname});
			ruby_init_loadpath ();
			Console.WriteLine ("Done initializing RubyCompletion");
		}

		static Dictionary<string,string> icons = new Dictionary<string,string> {
			{ "method", Stock.Method },
			{ "constant", Stock.Literal }
		};
		
		public static ICompletionData[] Complete (string contents, string symbol, int line)
		{
			ICompletionData[] rv = null;
			
			int runstatus = 0;
			StringBuilder sb = new StringBuilder ();
			List<string> lines = new List<string> (contents.Split ('\n'));
			
			completions = new List<ICompletionData> ();
			lines.Insert (line, string.Format ("$md_completions = {0}.methods.collect{{|m| [m,{1}]}} + {0}.class.constants.collect{{|c| [c,{2}]}}", symbol, "'method'", "'constant'"));
			Console.WriteLine ("Replacing {0} with {1}", lines[line+1], lines[line]);
			lines.RemoveAt (line+1);
			lines.Insert (0, "$md_completions = []");
			
			foreach (string linestr in lines) {
				sb.AppendLine (linestr);
			}
			sb.AppendLine ("$md_completions");
		
			IntPtr raw_completions = rb_eval_string_wrap (sb.ToString (), ref runstatus);
			if (0 != runstatus) {
				Console.WriteLine ("Evaluation failed: {0}", runstatus);
				rb_eval_string_wrap ("puts($!)", ref runstatus);
				return new ICompletionData[0];
			}
			
			rb_iterate (IterateCompletions, raw_completions, AddCompletion, IntPtr.Zero);
			rv = completions.ToArray ();
			Console.WriteLine ("RubyCompletion: Returning {0} completions", completions.Count);
			
			return rv;
		}// Complete
		
		public static List<ICompletionData> completions;
		
		public static IntPtr AddCompletion (IntPtr completion, IntPtr extra)
		{
			string name = FromRubyString (rb_ary_entry (completion, 0));
			string itemtype = FromRubyString (rb_ary_entry (completion, 1));
			completions.Add (new CompletionData (name, icons[itemtype], name, name));
			return IntPtr.Zero;
		}// AddCompletion
		
		public static IntPtr IterateCompletions (IntPtr collection)
		{
			return rb_funcall(collection, rb_intern("each"), 0);
		}// IterateCompletions
		
		static string FromRubyString (IntPtr rubyval)
		{
			return Marshal.PtrToStringAuto (rb_string_value_cstr (ref rubyval));
		}
		
		public delegate IntPtr RubyFunction (IntPtr arguments);
		public delegate IntPtr YieldFunction (IntPtr yield_value, IntPtr extra);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_iterate (RubyFunction iterate_function, IntPtr iterate_arguments, YieldFunction yield_function, IntPtr extra_yield_arguments);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_each (IntPtr collection);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_rescue (RubyFunction function, IntPtr arguments, RubyFunction exception_handler, IntPtr handler_arguments);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_gv_get (string variable_name);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_eval_string_wrap (string eval_text, ref int status);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_eval_string_protect (string eval_text, ref int status);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_string_value_cstr (ref IntPtr rb_string);
		
		[DllImport("ruby1.8")]
		public static extern void ruby_init ();
		
		[DllImport("ruby1.8")]
		public static extern void ruby_init_loadpath ();
		
		[DllImport("ruby1.8")]
		public static extern void ruby_init_stack ();
		
		[DllImport("ruby1.8")]
		public static extern void ruby_set_argv (int argc, string[] argv);
		
		[DllImport("ruby1.8")]
		public static extern void ruby_script (string scriptname);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_funcall (IntPtr owner, IntPtr id, int dunno);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_intern (string symbol);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_ary_entry (IntPtr array, int index);
		
	}// RubyCompletion
}
