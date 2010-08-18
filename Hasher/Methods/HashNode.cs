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
	/// <summary>Represents a node in a HashList.</summary>
	public class HashNode {
		private byte[] hash;
		private long rangeStart;
		private long rangeEnd;


		/// <summary>Gets or sets the hash of the data represented by this node.</summary>
		public byte[] Hash {
			get { return hash; }
			set { hash = value; }
		}

		/// <summary>Gets or sets the index of the beginning of the range of data represented by this node.</summary>
		public long RangeStart {
			get { return rangeStart; }
			set {
				if (value < 0) {
					throw new ArgumentException(Hasher.Properties.Resources.invalidNodeRange, "value");
				}
				rangeStart = value;
			}
		}

		/// <summary>Gets or sets the index of the end of the range of data represented by this node.</summary>
		public long RangeEnd {
			get { return rangeEnd; }
			set {
				if (value < 0) {
					throw new ArgumentException(Hasher.Properties.Resources.invalidNodeRange, "value");
				}
				rangeEnd = value;
			}
		}

		/// <summary>Gets the number of bytes included in the range of data represented by this node.</summary>
		public long Count {
			get { return (RangeEnd - RangeStart) + 1;  }
		}


		/// <summary>Initializes a new instance of the HashNode class.</summary>
		public HashNode() : this(null, 0, 0) { }

		/// <summary>Initializes a new instance of the HashNode class.</summary>
		/// <param name="hash">The hash of the data represented by this node.</param>
		public HashNode(byte[] hash) : this(hash, 0, 0) { }

		/// <summary>Initializes a new instance of the HashNode class.</summary>
		/// <param name="hash">The hash of the data represented by this node.</param>
		/// <param name="rangeStart">The index of the beginning of the range of data represented by this node.</param>
		/// <param name="rangeEnd">The index of the end of the range of data represented by this node.</param>
		public HashNode(byte[] hash, long rangeStart, long rangeEnd) {
			Hash = hash;
			RangeStart = rangeStart;
			RangeEnd = rangeEnd;
		}
	}
}
