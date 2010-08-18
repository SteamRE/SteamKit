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
	/// <summary>Predefined standards for FNV algorithms.</summary>
	public enum FnvStandard {
		/// <summary>A 32bit FNV-0 algorithm.</summary>
		Fnv32BitType0,

		/// <summary>A 64bit FNV-0 algorithm.</summary>
		Fnv64BitType0,

		/// <summary>A 32bit FNV-1 algorithm.</summary>
		Fnv32BitType1,

		/// <summary>A 64bit FNV-1 algorithm.</summary>
		Fnv64BitType1,

		/// <summary>A 32bit FNV-1a algorithm.</summary>
		Fnv32BitType1A,

		/// <summary>A 64bit FNV-1a algorithm.</summary>
		Fnv64BitType1A
	}
}
