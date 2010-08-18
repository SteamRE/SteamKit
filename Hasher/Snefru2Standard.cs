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
	/// <summary>Predefined standard parameters for Snefru2 algorithms.</summary>
	public enum Snefru2Standard {
		/// <summary>Four passes with a 128bit result hash.</summary>
		Snefru128Bit4Pass,

		/// <summary>Four passes with a 256bit result hash.</summary>
		Snefru256Bit4Pass,

		/// <summary>Eight passes with a 128bit result hash.</summary>
		Snefru128Bit8Pass,

		/// <summary>Eight passes with a 256bit result hash.</summary>
		Snefru256Bit8Pass
	}
}
