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
using Classless.Hasher.Utilities;

namespace Classless.Hasher {
	/// <summary>Computes the Sum checksum for the input data using the managed library.</summary>
	public class Sum : HashAlgorithm, IParametrizedHashAlgorithm {
		private readonly object syncLock = new object();

		private SumParameters parameters;
		private ulong checksum;


		/// <summary>Gets the HashAlgorithmParameters being used by this HashAlgorithm.</summary>
		public HashAlgorithmParameters Parameters {
			get { return parameters; }
		}


		/// <summary>Initializes a new instance of the Sum class.</summary>
		/// <remarks>This constructor implements the default parameters of Sum32Bit.</remarks>
		public Sum() : this(SumParameters.GetParameters(SumStandard.Sum32Bit)) { }

		/// <summary>Initializes a new instance of the Sum class.</summary>
		/// <param name="parameters">The parameters to utilize in the Sum calculation.</param>
		/// <exception cref="ArgumentNullException">When the specified parameters are null.</exception>
		public Sum(SumParameters parameters) : base() {
			lock (syncLock) {
				if (parameters == null) { throw new ArgumentNullException("parameters", Properties.Resources.paramCantBeNull); }
				this.parameters = parameters;
				HashSizeValue = this.parameters.Order;
			}
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				checksum = 0;
			}
		}


		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="ibStart">The position in the array to begin reading from.</param>
		/// <param name="cbSize">How many bytes in the array to read.</param>
		override protected void HashCore(byte[] array, int ibStart, int cbSize) {
			lock (syncLock) {
				for (int i = ibStart; i < (ibStart + cbSize); i++) {
					checksum += array[i];
				}
			}
		}


		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				byte[] temp = new byte[parameters.Order / 8];
				Array.Copy(Conversions.ULongToByte(checksum, EndianType.BigEndian), (8 - temp.Length), temp, 0, temp.Length);
				return temp;
			}
		}
	}
}
