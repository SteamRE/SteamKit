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
	/// <summary>An implementation of a Hash Tree structure.</summary>
	public class HashTree : BlockHashAlgorithm {
		/// <summary>The default size of the blocks the source data will be split into.</summary>
		public static readonly int DefaultBlockSize = 1024;

		private readonly object syncLock = new object();

		private System.Security.Cryptography.HashAlgorithm hashAlgorithm;
		private List<HashTreeNode> workingNodes = new List<HashTreeNode>();
		private HashTreeNode root;


		/// <summary>Gets the root HashTreeNode of the HashTree.</summary>
		public HashTreeNode Root {
			get { return root; }
		}


		/// <summary>Initializes a new instance of the HashTree class.</summary>
		/// <remarks>This constructor uses the default Tiger implementation.</remarks>
		public HashTree() : this(new Tiger(), DefaultBlockSize) { }

		/// <summary>Initializes a new instance of the HashTree class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the HashTreeNodes with.</param>
		/// <exception cref="ArgumentNullException">When the specified HashAlgorithm is null.</exception>
		public HashTree(System.Security.Cryptography.HashAlgorithm hashAlgorithm) : this(hashAlgorithm, DefaultBlockSize) { }

		/// <summary>Initializes a new instance of the HashTree class.</summary>
		/// <param name="hashAlgorithm">The HashAlgorithm to calculate the HashTreeNodes with.</param>
		/// <param name="blockSize">The size of the blocks the source data will be split into.</param>
		/// <exception cref="ArgumentNullException">When the specified HashAlgorithm is null.</exception>
		public HashTree(System.Security.Cryptography.HashAlgorithm hashAlgorithm, int blockSize) : base(blockSize) {
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
				workingNodes = new List<HashTreeNode>();
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				root = null;
				workingNodes.Add(new HashTreeNode(LeafHash(inputBuffer, inputOffset, BlockSize), Count, (Count + BlockSize - 1)));
			}
		}


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		override protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			lock (syncLock) {
				if ((Count == 0) || (inputCount > 0)) {
					long len = Count + inputCount - 1;
					if (len < 0) { len = 0; }
					workingNodes.Add(new HashTreeNode(LeafHash(inputBuffer, inputOffset, inputCount), Count, len));
				}

				List<HashTreeNode> newNodes;
				HashTreeNode node;
				while (workingNodes.Count > 1) {
					newNodes = new List<HashTreeNode>();
					for (int i = 0; i < workingNodes.Count; i += 2) {
						if (i == (workingNodes.Count - 1)) {
							newNodes.Add(workingNodes[i]);
							break;
						}

						node = new HashTreeNode();
						node.Left = workingNodes[i];
						node.Right = workingNodes[i + 1];
						node.RangeStart = node.Left.RangeStart;
						node.RangeEnd = node.Right.RangeEnd;
						node.Hash = InternalHash(node.Left.Hash, node.Right.Hash);
						newNodes.Add(node);
					}
					workingNodes = newNodes;
				}
				root = workingNodes[0];

				return root.Hash;
			}
		}


		/// <summary>Calculates the hash for a Leaf node.</summary>
		/// <param name="data">The data to hash.</param>
		/// <param name="offset">Where in the data array to start hashing.</param>
		/// <param name="count">How many bytes in the data array to hash.</param>
		/// <returns>The resulting Leaf hash.</returns>
		protected byte[] LeafHash(byte[] data, int offset, int count) {
			hashAlgorithm.Initialize();
			hashAlgorithm.TransformBlock(new byte[1] { 0 }, 0, 1, null, 0);
			hashAlgorithm.TransformFinalBlock(data, offset, count);
			return hashAlgorithm.Hash;
		}


		/// <summary>Calculates the Internal hash of two Child hashes.</summary>
		/// <param name="left">The hash of the Left Child node.</param>
		/// <param name="right">The hash of the Right Child node.</param>
		/// <returns>The resulting Internal hash.</returns>
		protected byte[] InternalHash(byte[] left, byte[] right) {
			hashAlgorithm.Initialize();
			hashAlgorithm.TransformBlock(new byte[1] { 1 }, 0, 1, null, 0);
			hashAlgorithm.TransformBlock(left, 0, left.Length, null, 0);
			hashAlgorithm.TransformFinalBlock(right, 0, right.Length);
			return hashAlgorithm.Hash;
		}
	}
}