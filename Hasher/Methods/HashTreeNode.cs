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

namespace Classless.Hasher.Methods {
	/// <summary>Represents a node in a HashTree.</summary>
	public class HashTreeNode : HashNode {
		private HashTreeNode parent;
		private HashTreeNode left;
		private HashTreeNode right;


		/// <summary>Gets or sets parent HashTreeNode of this node.</summary>
		public HashTreeNode Parent {
			get { return parent; }
			set {
				if ((value != null) && ((value == Left) || (value == Right))) {
					throw new ArgumentException(Hasher.Properties.Resources.nodeCantLoop, "value");
				}
				parent = value;
			}
		}

		/// <summary>Gets or sets the left child HashTreeNode of this node.</summary>
		public HashTreeNode Left {
			get { return left; }
			set {
				if (value != null) {
					if ((value == Right) || (value == Parent)) {
						throw new ArgumentException(Hasher.Properties.Resources.nodeCantLoop, "value");
					}
					value.Parent = this;
				}
				left = value;
			}
		}

		/// <summary>Gets or sets the right child HashTreeNode of this node.</summary>
		public HashTreeNode Right {
			get { return right; }
			set {
				if (value != null) {
					if ((value == Left) || (value == Parent)) {
						throw new ArgumentException(Hasher.Properties.Resources.nodeCantLoop, "value");
					}
					value.Parent = this;
				}
				right = value;
			}
		}


		/// <summary>Initializes a new instance of the HashTreeNode class.</summary>
		public HashTreeNode() : this(null, null) { }

		/// <summary>Initializes a new instance of the HashTreeNode class.</summary>
		/// <param name="parent">The parent HashTreeNode of this node.</param>
		public HashTreeNode(HashTreeNode parent) : this(null, parent) { }

		/// <summary>Initializes a new instance of the HashTreeNode class.</summary>
		/// <param name="hash">The hash of the data represented by this node.</param>
		public HashTreeNode(byte[] hash) : this(hash, null) { }

		/// <summary>Initializes a new instance of the HashTreeNode class.</summary>
		/// <param name="hash">The hash of the data represented by this node.</param>
		/// <param name="parent">The parent HashTreeNode of this node.</param>
		public HashTreeNode(byte[] hash, HashTreeNode parent) : base(hash) {
			Parent = parent;
		}

		/// <summary>Initializes a new instance of the HashTreeNode class.</summary>
		/// <param name="hash">The hash of the data represented by this node.</param>
		/// <param name="rangeStart">The index of the beginning of the range of data represented by this node.</param>
		/// <param name="rangeEnd">The index of the end of the range of data represented by this node.</param>
		public HashTreeNode(byte[] hash, long rangeStart, long rangeEnd) : this(hash, rangeStart, rangeEnd, null) { }

		/// <summary>Initializes a new instance of the HashTreeNode class.</summary>
		/// <param name="hash">The hash of the data represented by this node.</param>
		/// <param name="rangeStart">The index of the beginning of the range of data represented by this node.</param>
		/// <param name="rangeEnd">The index of the end of the range of data represented by this node.</param>
		/// <param name="parent">The parent HashTreeNode of this node.</param>
		public HashTreeNode(byte[] hash, long rangeStart, long rangeEnd, HashTreeNode parent) : base(hash, rangeStart, rangeEnd) {
			Parent = parent;
		}
	}
}
