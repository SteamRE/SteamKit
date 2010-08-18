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
using System.Security.Cryptography;
using Classless.Hasher.Utilities;

namespace Classless.Hasher.Mac {
	/// <summary>Implements the HMAC keyed message authentication code algorithm.</summary>
	public class Hmac : KeyedHashAlgorithm {
		private readonly object syncLock = new object();

		private BlockHashAlgorithm hashAlgorithm;
		private byte[] keyBuffer;
		private byte[] innerPadding;
		private byte[] outerPadding;
		private bool isHashing;


		/// <summary>Gets the hash algorithm used in the computation.</summary>
		/// <exception cref="CryptographicException">When an attempt to change the value of the HashAlgorithm occurs during the execution of a hash calculation.</exception>
		public BlockHashAlgorithm HashAlgorithm {
			get {
					return hashAlgorithm;
			}
			set {
				if (isHashing) {
					throw new CryptographicException(Properties.Resources.cantChangeHasher);
				}
				hashAlgorithm = value;
				InitializeKey(KeyValue);
			}
		}


		/// <summary>Gets the size of the computed hash code in bits.</summary>
		override public int HashSize {
			get { return hashAlgorithm.HashSize; }
		}


		/// <summary>Gets or sets the key to use in the hash algorithm.</summary>
		/// <exception cref="CryptographicException">When an attempt to change the value of the Key occurs during the execution of a hash calculation.</exception>
		override public byte[] Key {
			get {
				return (byte[])KeyValue.Clone();
			}
			set {
				if (isHashing) {
					throw new CryptographicException(Properties.Resources.cantChangeKey);
				}
				InitializeKey(value);
			}
		}


		/// <summary>Initializes a new instance of the HMAC class.</summary>
		/// <remarks>The default HashAlgorithm will be used, and a random key will be generated.</remarks>
		public Hmac() : this((BlockHashAlgorithm)Classless.Hasher.HashAlgorithm.Create(), null) { }

		/// <summary>Initializes a new instance of the HMAC class.</summary>
		/// <param name="hashAlgorithm">The base hash algorithm to use.</param>
		/// <remarks>A random key will be generated.</remarks>
		public Hmac(BlockHashAlgorithm hashAlgorithm) : this(hashAlgorithm, null) { }

		/// <summary>Initializes a new instance of the HMAC class.</summary>
		/// <param name="hashAlgorithm">The base hash algorithm to use.</param>
		/// <param name="key">The key to use for the HMAC.</param>
		/// <exception cref="ArgumentNullException">When the specified HashAlgorithm is null.</exception>
		/// <remarks>If the specified key is null, a random key will be generated and used.</remarks>
		public Hmac(BlockHashAlgorithm hashAlgorithm, byte[] key) {
			lock (syncLock) {
				if (hashAlgorithm == null) { throw new ArgumentNullException("hashAlgorithm", Properties.Resources.hashCantBeNull); }
				this.hashAlgorithm = hashAlgorithm;

				if (key == null) {
					Random r = new Random((int)System.DateTime.Now.Ticks);
					byte[] temp = new byte[HashAlgorithm.BlockSize];
					r.NextBytes(temp);
					InitializeKey(temp);
				} else {
					InitializeKey(key);
				}
			}
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Hmac-{0}", hashAlgorithm.ToString());
		}


		/// <summary>Initializes the MAC.</summary>
		override public void Initialize() {
			lock (syncLock) {
				State = 0;
				hashAlgorithm.Initialize();
				isHashing = false;
			}
		}


		/// <summary>Prepares the Key for use.</summary>
		/// <param name="key">The key to use.</param>
		private void InitializeKey(byte[] key) {
			KeyValue = (byte[])key.Clone();

			if (KeyValue.Length > HashAlgorithm.BlockSize) {
				keyBuffer = HashAlgorithm.ComputeHash(key);
			} else {
				keyBuffer = (byte[])key.Clone();
			}

			innerPadding = new byte[HashAlgorithm.BlockSize];
			outerPadding = new byte[HashAlgorithm.BlockSize];
			for (int i = 0; i < HashAlgorithm.BlockSize; i++) {
				innerPadding[i] = 0x36;
				outerPadding[i] = 0x5C;
			}

			for (int i = 0; i < keyBuffer.Length; i++) {
				innerPadding[i] ^= keyBuffer[i];
				outerPadding[i] ^= keyBuffer[i];
			}
		}


		/// <summary>Routes data written to the object into the hash algorithm for computing the hash.</summary>
		/// <param name="array">The input for which to compute the hash code.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data. </param>
		/// <param name="cbSize">The number of bytes in the byte array to use as data. </param>
		override protected void HashCore(byte[] array, int ibStart, int cbSize) {
			lock (syncLock) {
				if (!isHashing) {
					HashAlgorithm.TransformBlock(innerPadding, 0, innerPadding.Length, null, 0);
					isHashing = true;
				}
				hashAlgorithm.TransformBlock(array, ibStart, cbSize, null, 0);
			}
		}


		/// <summary>Finalizes the hash computation after the last data is processed by the cryptographic stream object.</summary>
		/// <returns>The computed hash code.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				if (!isHashing) {
					HashAlgorithm.TransformBlock(innerPadding, 0, innerPadding.Length, null, 0);
					isHashing = true;
				}
				HashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
				byte[] data = HashAlgorithm.Hash;

				hashAlgorithm.Initialize();
				hashAlgorithm.TransformBlock(outerPadding, 0, outerPadding.Length, null, 0);
				hashAlgorithm.TransformBlock(data, 0, data.Length, null, 0);
				HashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);

				isHashing = false;
				return HashAlgorithm.Hash;
			}
		}
	}
}
