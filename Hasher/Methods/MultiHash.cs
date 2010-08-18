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
	/// <summary>Computes multiple hashes/checksums at once.</summary>
	public class MultiHash : HashAlgorithm {
		private readonly object syncLock = new object();

		private HashAlgorithmCollection hashAlgorithms = new HashAlgorithmCollection();


		/// <summary>Gets the size of the computed hash code in bits.</summary>
		/// <remarks>This value will be the largest of all of the inner HashAlgorithm.HashSize values.</remarks>
		public override int HashSize {
			get {
				int hashSize = 0;
				foreach (System.Security.Cryptography.HashAlgorithm hasher in HashAlgorithms) {
					if (hasher.HashSize > hashSize) {
						hashSize = hasher.HashSize;
					}
				}
				return hashSize;
			}
		}


		/// <summary>Gets or sets the list of HashAlgorithms that are being used.</summary>
		public HashAlgorithmCollection HashAlgorithms {
			get { return hashAlgorithms; }
		}


		/// <summary>Initializes an instance of MultiHash.</summary>
		public MultiHash() : base() {
			hashAlgorithms.Changed += new EventHandler<ChangedEventArgs>(HashAlgorithmsChanged);
		}

		/// <summary>Initializes an instance of MultiHash.</summary>
		/// <param name="hashAlgorithms">The list of HashAlgorithms to use in the calculations.</param>
		/// <exception cref="ArgumentNullException">When the specified collection is null.</exception>
		public MultiHash(HashAlgorithmCollection hashAlgorithms) : base() {
			if (hashAlgorithms == null) {
				throw new ArgumentNullException("hashAlgorithms", Properties.Resources.hashListCantBeNull);
			}
			this.hashAlgorithms = hashAlgorithms;
			this.hashAlgorithms.Changed += new EventHandler<ChangedEventArgs>(HashAlgorithmsChanged);
		}

		/// <summary>Initializes an instance of MultiHash.</summary>
		/// <param name="hashAlgorithms">The list of HashAlgorithms to use in the calculations.</param>
		public MultiHash(params System.Security.Cryptography.HashAlgorithm[] hashAlgorithms) : base() {
			this.hashAlgorithms.AddRange(hashAlgorithms);
			this.hashAlgorithms.Changed += new EventHandler<ChangedEventArgs>(HashAlgorithmsChanged);
		}


		/// <summary>The delegate that handles the Changed event of the HashAlgorithms property.</summary>
		/// <param name="sender">The HashAlgorithmList object that triggered the event.</param>
		/// <param name="e">Data about the event.</param>
		/// <exception cref="CryptographicException">When the type of change that triggered this event will invalidate the hash calculation that is in progress.</exception>
		protected virtual void HashAlgorithmsChanged(object sender, ChangedEventArgs e) {
			if ((e != null) && (e.ChangeType == ChangedEventType.Element) && (State != 0)) {
				throw new CryptographicException(Properties.Resources.cantChangeHasherList);
			}
		}


		/// <summary>Initializes the algorithm(s).</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				foreach (System.Security.Cryptography.HashAlgorithm hasher in hashAlgorithms) {
					hasher.Initialize();
				}
			}
		}


		/// <summary>Performs the hash algorithm(s) on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="ibStart">The position in the array to begin reading from.</param>
		/// <param name="cbSize">How many bytes in the array to read.</param>
		override protected void HashCore(byte[] array, int ibStart, int cbSize) {
			lock (syncLock) {
				byte[] temp = new byte[array.Length];

				foreach (System.Security.Cryptography.HashAlgorithm hasher in hashAlgorithms) {
					hasher.TransformBlock(array, ibStart, cbSize, temp, 0);
				}
			}
		}


		/// <summary>Performs any final activities required by the hash algorithm(s).</summary>
		/// <returns>Null. The individual computed hashes/checksums should be retrieved from the Hashers list.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				byte[] dummy = new byte[1];

				foreach (System.Security.Cryptography.HashAlgorithm hasher in hashAlgorithms) {
					hasher.TransformFinalBlock(dummy, 0, 0);
				}

				if (hashAlgorithms.Count > 0) {
					return hashAlgorithms[0].Hash;
				} else {
					return new byte[0];
				}
			}
		}
	}
}
