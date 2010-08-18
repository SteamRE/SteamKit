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
using Classless.Hasher.Utilities;

namespace Classless.Hasher {
	/// <summary>Computes the ELF hash for the input data using the managed library.</summary>
	public class ElfHash : HashAlgorithm {
		private readonly object syncLock = new object();

		private uint hash;
		private uint ghash;


		/// <summary>Initializes a new instance of the ElfHash class.</summary>
		public ElfHash() : base() {
			HashSizeValue = 32;
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				hash = 0;
				ghash = 0;
			}
		}


		/// <summary>Drives the hashing function.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="ibStart">The position in the array to begin reading from.</param>
		/// <param name="cbSize">How many bytes in the array to read.</param>
		override protected void HashCore(byte[] array, int ibStart, int cbSize) {
			lock (syncLock) {
				for (int i = ibStart; i < (ibStart + cbSize); i++) {
					hash = (hash << 4) + array[i];
					ghash = hash & 0xF0000000;
					if (ghash != 0) {
						hash ^= ghash >> 24;
					}
					hash &= ~ghash;
				}
			}
		}


		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				return Conversions.UIntToByte(hash, EndianType.BigEndian);
			}
		}
	}
}
