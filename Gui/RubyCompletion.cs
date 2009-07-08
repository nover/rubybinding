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
using System.Text.RegularExpressions;
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

		static string[,] completors = new string[,] {
			{".methods", Stock.Method},
			{".class.instance_variables", Stock.Field },
			{".class.constants", Stock.Literal },
			{".class.class_variables", Stock.Field }
		};
		
		// Don't complete operators
		static Regex completionResult = new Regex (@"^[@\w:]", RegexOptions.Compiled);
		
		public static ICompletionData[] CompleteSymbol (string contents, string symbol, int line)
		{
			ICompletionData[] rv = null;
			
			int runstatus = 0;
			StringBuilder sb = new StringBuilder ();
			List<string> lines = new List<string> (contents.Split ('\n'));
			
			completions = new List<ICompletionData> ();
			
			lines[line] = symbol;
			lines.Insert (0, "set_trace_func(proc{|event,file,line,id,binding,klass| if('line'==event && __FILE__==file) then $monodevelop_bindings[line-$baseline]=binding; end})");
			lines.Insert (0, "$monodevelop_bindings = {}");
			lines.Insert (0, "$baseline = __LINE__-1");
			
			foreach (string linestr in lines) {
				sb.AppendLine (linestr);
			}
			sb.AppendLine ("$monodevelop_bindings.each_key{|k| puts(k)}");
			
			sb.Append ("eval('[$!");
			for (int i=0; i<completors.GetLength(0); ++i) {
				sb.AppendFormat (", {0}{1}", symbol, completors[i,0]);
			}
			sb.AppendLine (string.Format("]', $monodevelop_bindings[{0}])", line+3));
		
//			Console.WriteLine (sb.ToString ());
			IntPtr raw_completions = rb_eval_string_wrap (sb.ToString (), ref runstatus);
			if (0 != runstatus) {
				Console.WriteLine ("Evaluation failed: {0}", runstatus);
				rb_eval_string_wrap ("puts($!)", ref runstatus);
				return new ICompletionData[0];
			}
			
			for (int i=0; i<completors.GetLength(0); ++i) {
				rb_iterate (IterateCompletions, rb_ary_entry (raw_completions, i+1), delegate(IntPtr completion, IntPtr z){
					AddCompletion (completion, completors[i,1]);
					return IntPtr.Zero;
				}, IntPtr.Zero);
			}
			rv = completions.ToArray ();
			Console.WriteLine ("RubyCompletion: Returning {0} completions", completions.Count);
			
			return rv;
		}// CompleteSymbol
		
		public static List<ICompletionData> completions;
		
		public static void AddCompletion (IntPtr completion, string icon)
		{
			string name = FromRubyString (completion);
			// Console.WriteLine ("Adding {0} {1}", icon, name);
			if (completionResult.IsMatch (name)) {
				completions.Add (new CompletionData (name, icon, name, name));
			}
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
		public static extern void ruby_finalize ();
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_funcall (IntPtr owner, IntPtr id, int dunno);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_intern (string symbol);
		
		[DllImport("ruby1.8")]
		public static extern IntPtr rb_ary_entry (IntPtr array, int index);
		
	}// RubyCompletion
}
