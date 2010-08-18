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
 * Portions created by the Initial Developer are Copyright (C) 2009 the Initial
 * Developer. All Rights Reserved.
 *
 * Contributor(s):
 *		Jason Simeone (jay@classless.net)
 * 
 * ***** END LICENSE BLOCK ***** */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace Classless.Hasher.Methods {
	/// <summary>An implementation of a Hash Chain structure.</summary>
	public class HashChain : IEnumerable<byte[]> {
		/// <summary>The default number of hash iterations that will be calculated upon instantiation.</summary>
		public static readonly int DefaultInitialization = 10;
	

		private System.Security.Cryptography.HashAlgorithm hashAlgorithm;
		private List<byte[]> cache = new List<byte[]>();


		/// <summary>Gets the hash iteration at the specified index.</summary>
		/// <param name="index">The zero-based index of the hash iteration to get.</param>
		/// <returns>The specified hash iteration.</returns>
		/// <exception cref="ArgumentOutOfRangeException">When the specified index is less than zero.</exception>
		public byte[] this[int index] {
			get {
				if (index < 0) {
					throw new ArgumentOutOfRangeException(Hasher.Properties.Resources.indexOutOfRange);
				}

				if (index >= cache.Count) {
					CalculateTo(index + 1);
				}
				return (byte[])cache[index].Clone();
			}
		}


		/// <summary>Initializes a new instance of the HashChain class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the chain iterations with.</param>
		/// <param name="data">The value to initialize the chain with.</param>
		/// <exception cref="ArgumentNullException">When the specified HashALgorithm is null.</exception>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid or the initialization value is less than 1.</exception>
		public HashChain(System.Security.Cryptography.HashAlgorithm hashAlgorithm, byte[] data) : this(hashAlgorithm, data, 0, data.Length, DefaultInitialization) { }

		/// <summary>Initializes a new instance of the HashChain class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the chain iterations with.</param>
		/// <param name="data">The value to initialize the chain with.</param>
		/// <param name="initialize">How many iterations to calculate during instantiation.</param>
		/// <exception cref="ArgumentNullException">When the specified HashALgorithm is null.</exception>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid or the initialization value is less than 1.</exception>
		public HashChain(System.Security.Cryptography.HashAlgorithm hashAlgorithm, byte[] data, int initialize) : this(hashAlgorithm, data, 0, data.Length, initialize) { }

		/// <summary>Initializes a new instance of the HashChain class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the chain iterations with.</param>
		/// <param name="data">The value to initialize the chain with.</param>
		/// <param name="offset">Position in the byte array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to use.</param>
		/// <exception cref="ArgumentNullException">When the specified HashALgorithm is null.</exception>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid or the initialization value is less than 1.</exception>
		public HashChain(System.Security.Cryptography.HashAlgorithm hashAlgorithm, byte[] data, int offset, int length) : this(hashAlgorithm, data, offset, length, DefaultInitialization) { }

		/// <summary>Initializes a new instance of the HashChain class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the chain iterations with.</param>
		/// <param name="data">The value to initialize the chain with.</param>
		/// <param name="offset">Position in the byte array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to use.</param>
		/// <param name="initialize">How many iterations to calculate during instantiation.</param>
		/// <exception cref="ArgumentNullException">When the specified HashALgorithm is null.</exception>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid or the initialization value is less than 1.</exception>
		public HashChain(System.Security.Cryptography.HashAlgorithm hashAlgorithm, byte[] data, int offset, int length, int initialize) {
			if (hashAlgorithm == null) {
				throw new ArgumentNullException("hashAlgorithm", Hasher.Properties.Resources.hashCantBeNull);
			}
			if ((length + offset) > data.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if (initialize < 1) {
				throw new ArgumentException(Hasher.Properties.Resources.invalidHashChainInit, "initialize");
			}

			this.hashAlgorithm = hashAlgorithm;
			cache.Add(this.hashAlgorithm.ComputeHash(data, offset, length));
			CalculateTo(initialize);
		}


		/// <summary>Generates the hash chain up to the specified iteration.</summary>
		/// <param name="iteration">The maximum interation to calculate.</param>
		protected void CalculateTo(int iteration) {
			while (cache.Count < iteration) {
				cache.Add(hashAlgorithm.ComputeHash(cache[cache.Count - 1]));
			}
		}


		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<byte[]> GetEnumerator() {
			return cache.GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		IEnumerator<byte[]> IEnumerable<byte[]>.GetEnumerator() {
			return cache.GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator() {
			return cache.GetEnumerator();
		}
	}
}
