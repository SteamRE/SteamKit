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
	/// <summary>A class that contains the parameters necessary to initialize a FNV algorithm.</summary>
	public class FnvParameters : HashAlgorithmParameters {
		private int order;
		private long prime;
		private long offsetBasis;
		private FnvAlgorithmType algorithmType;


		/// <summary>Gets or sets the order of the FNV (e.g., how many bits).</summary>
		/// <exception cref="ArgumentOutOfRangeException">When the specified value is not either 32 or 64.</exception>
		public int Order {
			get { return order; }
			set {
				if ((value != 32) && (value != 64)) {
					throw new ArgumentOutOfRangeException("value", Hasher.Properties.Resources.invalidFnvOrder);
				} else {
					order = value;
				}
			}
		}

		/// <summary>Gets or sets the prime number to use in the FNV calculations.</summary>
		public long Prime {
			get { return prime; }
			set { prime = value; }
		}

		/// <summary>Gets or sets the offset basis of the FNV.</summary>
		public long OffsetBasis {
			get { return offsetBasis; }
			set { offsetBasis = value; }
		}

		/// <summary>Gets or sets the FNV algorithm variation.</summary>
		public FnvAlgorithmType AlgorithmType {
			get { return algorithmType; }
			set { algorithmType = value; }
		}


		/// <summary>Initializes a new instance of the FNVParamters class.</summary>
		/// <param name="order">The order of the FNV (e.g., how many bits).</param>
		/// <param name="prime">The prime number to use in the FNV calculations.</param>
		/// <param name="offsetBasis">The offset basis of the FNV.</param>
		/// <param name="type">The FNV algorithm variation.</param>
		public FnvParameters(int order, long prime, long offsetBasis, FnvAlgorithmType type) {
			this.Order = order;
			this.Prime = prime;
			this.OffsetBasis = offsetBasis;
			this.AlgorithmType = type;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:d}:{2:d}:{3:d}:{4})", this.GetType().Name, Order, Prime, OffsetBasis, AlgorithmType);
		}


		/// <summary>Retrieves a standard set of FNV parameters.</summary>
		/// <param name="standard">The name of the standard parameter set to retrieve.</param>
		/// <returns>The FNV Parameters for the given standard.</returns>
		public static FnvParameters GetParameters(FnvStandard standard) {
			FnvParameters temp = null;

			switch (standard) {
				case FnvStandard.Fnv32BitType0:	temp = new FnvParameters(32, 0x01000193,     0x00000000, FnvAlgorithmType.Fnv1); break;
				case FnvStandard.Fnv64BitType0:	temp = new FnvParameters(64, 0x0100000001B3, 0x00000000, FnvAlgorithmType.Fnv1); break;
				case FnvStandard.Fnv32BitType1:	temp = new FnvParameters(32, 0x01000193,     0x811C9DC5, FnvAlgorithmType.Fnv1); break;
				case FnvStandard.Fnv64BitType1:	temp = new FnvParameters(64, 0x0100000001B3, unchecked((long)0xCBF29CE484222325), FnvAlgorithmType.Fnv1); break;
				case FnvStandard.Fnv32BitType1A:	temp = new FnvParameters(32, 0x01000193,     0x811C9DC5, FnvAlgorithmType.Fnv1A); break;
				case FnvStandard.Fnv64BitType1A:	temp = new FnvParameters(64, 0x0100000001B3, unchecked((long)0xCBF29CE484222325), FnvAlgorithmType.Fnv1A); break;
			}

			return temp;
		}
	}
}
