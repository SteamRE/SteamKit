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
	/// <summary>An abstract class that represents the parameters necessary to initialize a hashing algorithm.</summary>
	abstract public class HashAlgorithmParameters {
		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		abstract override public string ToString();


		/// <summary>Serves as a hash function for a particular type, suitable for use in hashing algorithms and data structures like a hash table.</summary>
		/// <returns>A hash code for the current Object.</returns>
		override public int GetHashCode() {
			return this.ToString().GetHashCode();
		}


		/// <summary>Determines whether two HashAlgorithmParameters instances are equal.</summary>
		/// <param name="obj">The HashAlgorithmParameters to compare with the current Object.</param>
		/// <returns>true if the specified HashAlgorithmParameters is equal to the current HashAlgorithmParameters; otherwise, false.</returns>
		override public bool Equals(object obj) {
			if (obj == null) { return false; }
			return (
				(this.GetHashCode() == obj.GetHashCode())
				&& (this.GetType() == obj.GetType())
			);
		}
	}
}
