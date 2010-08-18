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
using System.Security.Cryptography;

namespace Classless.Hasher.Methods {
	/// <summary>Computes the Parallel hash for the input data using the managed libraries.</summary>
	public class ParallelHash : MultiHash {
		private readonly object syncLock = new object();


		/// <summary>Gets the size of the computed hash code in bits.</summary>
		/// <remarks>This value will be the sum of all of the inner HashAlgorithm.HashSize values.</remarks>
		public override int HashSize {
			get {
				int hashSize = 0;
				foreach (System.Security.Cryptography.HashAlgorithm hasher in HashAlgorithms) {
					hashSize += hasher.HashSize;
				}
				return hashSize;
			}
		}


		/// <summary>Initializes an instance of ParallelHash.</summary>
		public ParallelHash() : base() { }

		/// <summary>Initializes an instance of ParallelHash.</summary>
		/// <param name="hashAlgorithms">The list of HashAlgorithms to use in the calculations.</param>
		public ParallelHash(HashAlgorithmCollection hashAlgorithms) : base(hashAlgorithms) { }

		/// <summary>Initializes an instance of ParallelHash.</summary>
		/// <param name="hashAlgorithms">The list of HashAlgorithms to use in the calculations.</param>
		public ParallelHash(params System.Security.Cryptography.HashAlgorithm[] hashAlgorithms) : base(hashAlgorithms) { }


		/// <summary>The delegate that handles the Changed event of the HashAlgorithms property.</summary>
		/// <param name="sender">The HashAlgorithmList object that triggered the event.</param>
		/// <param name="e">Data about the event.</param>
		/// <exception cref="CryptographicException">When the type of change that triggered this event will invalidate the hash calculation that is in progress.</exception>
		override protected void HashAlgorithmsChanged(object sender, ChangedEventArgs e) {
			if (State != 0) {
				throw new CryptographicException(Properties.Resources.cantChangeHasherList);
			}
		}


		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				base.HashFinal();

				byte[] hash = new byte[HashSize / 8];
				byte[] temp;
				int position = 0;

				foreach (System.Security.Cryptography.HashAlgorithm hasher in HashAlgorithms) {
					temp = hasher.Hash;
					Array.Copy(temp, 0, hash, position, temp.Length);
					position += temp.Length;
				}

				return hash;
			}
		}
	}
}
