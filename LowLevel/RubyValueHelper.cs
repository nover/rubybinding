// 
//  RubyValueHelper.cs
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
using System.Runtime.InteropServices;

namespace MonoDevelop.RubyBinding.LowLevel
{
	public static class RubyValueHelper
	{
		internal static RubyValueType GetType(IntPtr input)
		{
			try
			{
				object obj = Marshal.PtrToStructure(input, typeof(RBasic));
				
				RBasic val = (RBasic)obj;
				return GetType(val);	
			}
			catch (Exception ex)
			{
				return RubyValueType.RUBY_T_NONE;
			}
		}
		
		/// <summary>
		/// Gets the type of the RBasic input.
		/// </summary>
		/// <returns>
		/// The type.
		/// </returns>
		/// <param name='input'>
		/// Input.
		/// </param>
		internal static RubyValueType GetType(RBasic input)
		{
			return (RubyValueType)(input.flags & 0x1f);
		}
	}
}

