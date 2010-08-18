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
using System.Security.Cryptography;
using Classless.Hasher.Utilities;

namespace Classless.Hasher.Methods {
	/// <summary>An implementation of a Hash List structure.</summary>
	public class HashList : BlockHashAlgorithm, IEnumerable<HashNode> {
		/// <summary>The default size of the blocks the source data will be split into.</summary>
		public static readonly int DefaultBlockSize = 1024;

		private readonly object syncLock = new object();

		private System.Security.Cryptography.HashAlgorithm hashAlgorithm;
		private List<HashNode> finalNodes = new List<HashNode>();
		private List<HashNode> workingNodes = new List<HashNode>();


		/// <summary>Gets the HashNode at the specified index.</summary>
		/// <param name="index">The zero-based index of the HashNode to get.</param>
		/// <returns>The specified HashNode.</returns>
		public HashNode this[int index] {
			get {
				return finalNodes[index];
			}
		}


		/// <summary>Initializes a new instance of the HashList class.</summary>
		/// <remarks>This constructor implements the default HashAlgorithm.</remarks>
		public HashList() : this(HashAlgorithm.Create()) { }

		/// <summary>Initializes a new instance of the HashList class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the HashNodes with.</param>
		/// <exception cref="ArgumentNullException">When the specified HashAlgorithm is null.</exception>
		public HashList(System.Security.Cryptography.HashAlgorithm hashAlgorithm) : this(hashAlgorithm, DefaultBlockSize) { }

		/// <summary>Initializes a new instance of the HashList class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the HashNodes with.</param>
		/// <param name="blockSize">The size of the blocks the source data will be split into.</param>
		/// <exception cref="ArgumentNullException">When the specified HashAlgorithm is null.</exception>
		public HashList(System.Security.Cryptography.HashAlgorithm hashAlgorithm, int blockSize) : base(blockSize) {
			lock (syncLock) {
				if (hashAlgorithm == null) {
					throw new ArgumentNullException("hashAlgorithm", Hasher.Properties.Resources.hashCantBeNull);
				}
				this.hashAlgorithm = hashAlgorithm;
				HashSizeValue = hashAlgorithm.HashSize;
			}
		}


		/// <summary>Initializes the list.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				hashAlgorithm.Initialize();
				workingNodes = new List<HashNode>();
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				workingNodes.Add(new HashNode(hashAlgorithm.ComputeHash(inputBuffer, inputOffset, BlockSize), Count, (Count + BlockSize - 1)));
			}
		}


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		override protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			lock (syncLock) {
				if (inputCount > 0) {
					workingNodes.Add(new HashNode(hashAlgorithm.ComputeHash(inputBuffer, inputOffset, inputCount), Count, (Count + inputCount - 1)));
				}
				finalNodes = workingNodes;

				// Calculate the top hash.
				hashAlgorithm.Initialize();
				foreach (HashNode node in this) {
					hashAlgorithm.TransformBlock(node.Hash, 0, node.Hash.Length, null, 0);
				}
				hashAlgorithm.TransformFinalBlock(new byte[1], 0, 0);

				return hashAlgorithm.Hash;
			}
		}


		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<HashNode> GetEnumerator() {
			return finalNodes.GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		IEnumerator<HashNode> IEnumerable<HashNode>.GetEnumerator() {
			return finalNodes.GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator() {
			return finalNodes.GetEnumerator();
		}
	}
}
