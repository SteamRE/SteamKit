#region License
/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is Classless.Hasher - C#/.NET Hash and Checksum Algorithm Library.
 *
 * The Initial Developer of the Original Code is Classless.net.
 * Portions created by the Initial Developer are Copyright (C) 2004 the Initial
 * Developer. All Rights Reserved.
 *
 * Contributor(s):
 *		Jason Simeone (jay@classless.net)
 * 
 * ***** END LICENSE BLOCK ***** */
#endregion

using System;

namespace Classless.Hasher {
	/// <summary>Predefined standard parameters for HAVAL algorithms.</summary>
	public enum HavalStandard {
		/// <summary>Three passes with a 128bit result hash.</summary>
		Haval128Bit3Pass,

		/// <summary>Three passes with a 160bit result hash.</summary>
		Haval160Bit3Pass,

		/// <summary>Three passes with a 192bit result hash.</summary>
		Haval192Bit3Pass,
		
		/// <summary>Three passes with a 224bit result hash.</summary>
		Haval224Bit3Pass,
		
		/// <summary>Three passes with a 256bit result hash.</summary>
		Haval256Bit3Pass,

		/// <summary>Four passes with a 128bit result hash.</summary>
		Haval128Bit4Pass,

		/// <summary>Four passes with a 160bit result hash.</summary>
		Haval160Bit4Pass,

		/// <summary>Four passes with a 192bit result hash.</summary>
		Haval192Bit4Pass,
		
		/// <summary>Four passes with a 224bit result hash.</summary>
		Haval224Bit4Pass,
		
		/// <summary>Four passes with a 256bit result hash.</summary>
		Haval256Bit4Pass,
	
		/// <summary>Five passes with a 128bit result hash.</summary>
		Haval128Bit5Pass,

		/// <summary>Five passes with a 160bit result hash.</summary>
		Haval160Bit5Pass,

		/// <summary>Five passes with a 192bit result hash.</summary>
		Haval192Bit5Pass,
		
		/// <summary>Five passes with a 224bit result hash.</summary>
		Haval224Bit5Pass,
		
		/// <summary>Five passes with a 256bit result hash.</summary>
		Haval256Bit5Pass
	}
}
