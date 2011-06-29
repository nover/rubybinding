// 
//  RubyValueType.cs
//  
//  Author:
//       nicklas <nicklas@isharp.dk>
// 
//  Copyright (c) 2011 nicklas
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
using System;

namespace MonoDevelop.RubyBinding.LowLevel
{
	/// <summary>
	/// The possible ruby value types.
	/// 
	/// Taken from ruby.h
	/// </summary>
	public enum RubyValueType
	{
		RUBY_T_NONE   = 0x00,

		RUBY_T_OBJECT = 0x01,
		RUBY_T_CLASS  = 0x02,
		RUBY_T_MODULE = 0x03,
		RUBY_T_FLOAT  = 0x04,
		RUBY_T_STRING = 0x05,
		RUBY_T_REGEXP = 0x06,
		RUBY_T_ARRAY  = 0x07,
		RUBY_T_HASH   = 0x08,
		RUBY_T_STRUCT = 0x09,
		RUBY_T_BIGNUM = 0x0a,
		RUBY_T_FILE   = 0x0b,
		RUBY_T_DATA   = 0x0c,
		RUBY_T_MATCH  = 0x0d,
		RUBY_T_COMPLEX  = 0x0e,
		RUBY_T_RATIONAL = 0x0f,
		
		RUBY_T_NIL    = 0x11,
		RUBY_T_TRUE   = 0x12,
		RUBY_T_FALSE  = 0x13,
		RUBY_T_SYMBOL = 0x14,
		RUBY_T_FIXNUM = 0x15,
		
		RUBY_T_UNDEF  = 0x1b,
		RUBY_T_NODE   = 0x1c,
		RUBY_T_ICLASS = 0x1d,
		RUBY_T_ZOMBIE = 0x1e,
		
		RUBY_T_MASK   = 0x1f
	}
}

