// RubyCompletion.cs
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MonoDevelop.Core.Gui;
using MonoDevelop.Projects.Dom;
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
			// TODO: Does this always happen on the main thread?
			DispatchService.GuiDispatch (delegate () {
				Console.WriteLine ("Initializing RubyCompletion");
				string scriptname = "monodevelop_ruby_parser";
				ruby_init ();
				ruby_script (scriptname);
				ruby_set_argv (1, new string[]{scriptname});
				ruby_init_loadpath ();
				Console.WriteLine ("Done initializing RubyCompletion");
			});
		}
		
		static Dictionary<Regex,CompleteFunction> symbolTypes = new Dictionary<Regex,CompleteFunction> {
			{ new Regex ("^'[^']*'$", RegexOptions.Compiled), delegate(string basepath, string contents, string s, int line){ return CompleteSymbol(basepath, contents, "\"\"", line, instance_completors); } },
//			{ new Regex ("^\"(\\\\\"|[^\"])*\"$", RegexOptions.Compiled), delegate(string contents, string s, int line){ return CompleteSymbol(contents, "String", line); } },
//			{ new Regex (@"^([\d\.]+|0x[a-fA-F\d]+)$", RegexOptions.Compiled), delegate(string contents, string s, int line){ return CompleteSymbol(contents, "Numeric", line); } },
//			{ new Regex (@"^\[.*\]$", RegexOptions.Compiled), delegate(string contents, string s, int line){ return CompleteSymbol(contents, "Array", line); } },
//			{ new Regex (@"^{.*}$", RegexOptions.Compiled), delegate(string contents, string s, int line){ return CompleteSymbol(contents, "Hash", line); } },
//			{ new Regex (@"^/(\\/|[^/])*/$", RegexOptions.Compiled), delegate(string contents, string s, int line){ return CompleteSymbol(contents, "Regexp", line); } },
//			{ new Regex (@"^:\w[\w\d]*$", RegexOptions.Compiled), delegate(string contents, string s, int line){ return CompleteSymbol(contents, "Symbol", line); } },
		};
		
		internal static readonly string[] reservedWords = new string[] {
			"alias", "and", "BEGIN", "begin", "break", "case", "class", "def", "defined?", "do", "else", "elsif", "END", "end", "ensure", "false", "for", "if", "in", "module", "next", "nil", "not", "or", "redo", "rescue", "retry", "return", "self", "super", "then", "true", "undef", "unless", "until", "when", "while", "yield"
		};
		
		static string[] operators = new string[] {
			"::", ".", "[", "**", "!", "~", "*",  "/",  "%", "+",  "-", "<<",  ">>", "&", "|",  "^", ">",  ">=",  "<",  "<=", "<=>", "==", "===", "!=", "=~", "!~", "&&", "||", "..", "...", "=", "**=", "!=", "~=", "*=",  "/=",  "%=", "+=",  "-=", "<<=",  ">>=", "&=", "|=",  "^=", "&&=", "||="
		};

		static string[,] instance_completors = new string[,] {
			{".class.instance_methods", Stock.Method},
			{".class.instance_variables", Stock.Field },
//			{".class.constants", Stock.Literal },
//			{".class.class_variables", Stock.Field }
		};
		
		static string[,] class_completors = new string[,] {
			{".methods", Stock.Method },
			{".constants", Stock.Literal },
			{".class_variables", Stock.Field }
		};
		
		// Don't complete operators
		static Regex completionResult = new Regex (@"^[@\w:]", RegexOptions.Compiled);
		static Regex errorMessage = new Regex (@"^[^:]*:(?<line>\d+):\s*(?<message>.*)", RegexOptions.Compiled);
		
		public static ICompletionData[] Complete (string basepath, string contents, string symbol, int line)
		{
			return GuiThreadSync<ICompletionData[]> (delegate() {
				if (0 > Array.IndexOf (reservedWords, symbol) && 0 > Array.IndexOf (operators, symbol)) {
					foreach (KeyValuePair<Regex,CompleteFunction> pair in symbolTypes) {
						if (pair.Key.IsMatch (symbol)){ return pair.Value (basepath, contents, symbol, line); }
					}
					return CompleteSymbol (basepath, contents, symbol, line, char.IsUpper (symbol[0])? class_completors: instance_completors);
				}
				
				return new ICompletionData[0];
			});
		}
		
		public static List<Error> CheckForErrors (string basepath, string contents)
		{
			return GuiThreadSync<List<Error>> (delegate() {
				int runstatus = 0;
				StringBuilder sb = new StringBuilder ();
				sb.AppendFormat ("$LOAD_PATH << '{0}'{1}", basepath, Environment.NewLine);
				sb.AppendLine ("(__LINE__-1).to_s");
				int baseline = 0;
				int.TryParse (FromRubyString (rb_eval_string_wrap (sb.ToString (), ref runstatus)), out baseline);
				if (0 != runstatus) {
					rb_eval_string_wrap ("puts($!)", ref runstatus);
				}
				rb_eval_string_wrap (contents, ref runstatus);
				Match match;
				List<Error> errors = new List<Error> ();
				
				if (0 != runstatus) {
					string[] messages = FromRubyString (rb_eval_string_wrap ("$!.message", ref runstatus)).Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
					foreach (string error in messages) {
						// Console.WriteLine ("RubyCompletion: Error {0}", error);
						if (null != (match = errorMessage.Match (error)) && match.Success) {
							errors.Add (new Error (int.Parse (match.Groups["line"].Value)-baseline, 1, match.Groups["message"].Value));
						}
					}
				}
				
				return errors;
			});
		}
		
		public static string[] GetMethodArguments (string basepath, string contents, int line, string owner, string method)
		{
			Console.WriteLine ("GetMethodArguments({0},contents,{1},{2},{3})", basepath, line, owner, method);
			int runstatus = 0;
			List<string> lines = new List<string> (contents.Split ('\n'));
			string joiner = string.IsNullOrEmpty (owner)? string.Empty: ".";
			lines[line] = string.Empty;
			
			IntPtr arityval = EvaluateInContext (basepath, lines, string.Format ("{0}{1}method('{2}').arity.to_s", owner, joiner, method), line, ref runstatus);
			
			if (0 != runstatus) {
				Console.WriteLine ("Evaluation failed: {0}", runstatus);
				rb_eval_string_wrap ("puts($!)", ref runstatus);
				return new string[0];
			}
			
			int arity = int.Parse (FromRubyString (arityval));
			// Console.WriteLine ("Got arity {0}", arity);
			
			return ParamsFromArity (arity);
		}// GetMethodArguments
		
		
		public static readonly char[] wordBreakChars = new char[]{ ' ', '\t', '\r', '\n', '\\', '`', '>', '<', '=', ';', '|', '&', '(', '.' };
		internal static string GetSymbol (string contents, int offset)
		{
			if (string.IsNullOrEmpty (contents) || 0 == offset) { 
				Console.WriteLine ("RubyBinding: Empty contents or zero trigger offset {0}", offset);
				return string.Empty; 
			}
			
			int start = contents.LastIndexOfAny (wordBreakChars, offset-1)+1,
			    end = contents.IndexOfAny (wordBreakChars, offset-1);
			
			Console.WriteLine ("RubyBinding: Start {0}, End {1}", start, end);
			
			if (0 > start){ start = 0; }
			if (0 > end){ end = contents.Length; }
			if (end < start){ end = start; }
			
			Console.WriteLine ("RubyBinding: Start {0}, End {1}", start, end);
			
			return contents.Substring (start, end-start);
		}
		
		static string[] ParamsFromArity (int arity)
		{
			List<string> theparams = new List<string> ();
			bool varargs = (0 > arity);
			
			if (varargs){ arity = -arity - 1; }
			for (int i=0; i<arity; ++i) {
				theparams.Add (string.Format ("arg{0}", i));
			}
			if (varargs){ theparams.Add ("*args"); }
			
			return theparams.ToArray ();
		}// ParamsFromArity
		
		static ICompletionData[] CompleteSymbol (string basepath, string contents, string symbol, int line, string[,] completors)
		{
			ICompletionData[] rv = null;
			int runstatus = 0;
			StringBuilder sb = new StringBuilder ();
			List<string> lines = new List<string> (contents.Split ('\n'));
			
			lines[line] = symbol;
			
			symbol = symbol.Replace ("'", "\\'");
			sb.Append ("eval('[$!");
			for (int i=0; i < completors.GetLength (0); ++i) {
				sb.AppendFormat (", {0}{1}", symbol, completors[i, 0]);
			}
			sb.AppendLine (string.Format ("]', $monodevelop_bindings[{0}])", line + 2));

			IntPtr raw_completions = EvaluateInContext (basepath, lines, sb.ToString (), line, ref runstatus);
			if (0 != runstatus) {
				Console.WriteLine ("Evaluation failed: {0}", runstatus);
				rb_eval_string_wrap ("puts($!)", ref runstatus);
				return new ICompletionData[0];
			}
			
			completions = new List<ICompletionData> ();
			for (int i=0; i<completors.GetLength(0); ++i) {
				rb_iterate (IterateCompletions, rb_ary_entry (raw_completions, i+1), delegate(IntPtr completion, IntPtr z){
					AddCompletion (completion, completors[i,1]);
					return IntPtr.Zero;
				}, IntPtr.Zero);
			}
			rv = completions.ToArray ();
			// Console.WriteLine ("RubyCompletion: Returning {0} completions", completions.Count);
			
			return rv;
		}// CompleteSymbol

		static IntPtr EvaluateInContext (string basepath, IList<string> lines, string expression, int line, ref int runstatus) {
			StringBuilder sb = new StringBuilder ();
			sb.AppendLine (string.Format ("$LOAD_PATH << '{0}'", basepath));
			sb.AppendLine ("$baseline = __LINE__-1");
			sb.AppendLine ("$monodevelop_bindings = {}");
			sb.AppendLine ("set_trace_func(proc{|event,file,line,id,binding,klass| if('line'==event && __FILE__==file) then $monodevelop_bindings[line-$baseline]=binding; end})");
			
			foreach (string linestr in lines) {
				sb.AppendLine (linestr);
			}
			sb.AppendLine (expression);
			
			// Console.WriteLine (sb.ToString ());
			IntPtr result = rb_eval_string_wrap (sb.ToString (), ref runstatus);
			int localstatus = 0;
			rb_eval_string_wrap ("$LOAD_PATH.slice!(-1)", ref localstatus);
			return result;
		}
		
		public static List<ICompletionData> completions;
		
		static void AddCompletion (IntPtr completion, string icon)
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
			if (IntPtr.Zero == rubyval || Qnil == rubyval){ return string.Empty; }
			return Marshal.PtrToStringAuto (rb_string_value_cstr (ref rubyval));
		}
		
		/// <summary>
		/// Performs an operation on the gui thread, 
		/// blocking until completion.
		/// </summary>
		/// <param name="realfunction">
		/// A <see cref="Func<T>"/>: The real function to execute
		/// </param>
		/// <returns>
		/// A <see cref="T"/>: The result of realfunction, or default(T)
		/// </returns>
		static T GuiThreadSync<T> (Func<T> realfunction)
		{
			T result = default (T);
			
			DispatchService.GuiSyncDispatch (delegate () {
				result = realfunction ();
			});
			
			return result;
		}// GuiThreadSync
		
		public delegate IntPtr RubyFunction (IntPtr arguments);
		public delegate IntPtr YieldFunction (IntPtr yield_value, IntPtr extra);
		public delegate ICompletionData[] CompleteFunction (string basepath, string contents, string symbol, int line);
		
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
		
		public static readonly IntPtr Qnil = new IntPtr (4); // ruby.h
	}// RubyCompletion
}
