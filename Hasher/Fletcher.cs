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
	/// <summary>Computes the Fletcher checksum for the input data using the managed library.</summary>
	public class Fletcher : HashAlgorithm, IParametrizedHashAlgorithm {
		private readonly object syncLock = new object();

		private FletcherParameters parameters;
		private uint value1;
		private uint value2;
		private uint modulo;


		/// <summary>Gets the HashAlgorithmParameters being used by this HashAlgorithm.</summary>
		public HashAlgorithmParameters Parameters {
			get { return parameters; }
		}


		/// <summary>Initializes a new instance of the Fletcher class.</summary>
		/// <remarks>This constructor implements the default parameters of Fletcher32Bit.</remarks>
		public Fletcher() : this(FletcherParameters.GetParameters(FletcherStandard.Fletcher32Bit)) { }

		/// <summary>Initializes a new instance of the Fletcher class.</summary>
		/// <param name="parameters">The parameters to utilize in the Fletcher calculation.</param>
		/// <exception cref="ArgumentNullException">When the specified parameters are null.</exception>
		public Fletcher(FletcherParameters parameters) : base() {
			lock (syncLock) {
				if (parameters == null) { throw new ArgumentNullException("parameters", Properties.Resources.paramCantBeNull); }
				this.parameters = parameters;
				HashSizeValue = this.parameters.Order;
				modulo = (uint)(Math.Pow(2, (this.parameters.Order / 2)) - 1);
			}
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				value1 = 0;
				value2 = 0;
				modulo = (uint)(Math.Pow(2, (parameters.Order / 2)) - 1);
			}
		}


		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="ibStart">The position in the array to begin reading from.</param>
		/// <param name="cbSize">How many bytes in the array to read.</param>
		override protected void HashCore(byte[] array, int ibStart, int cbSize) {
			lock (syncLock) {
				for (int i = ibStart; i < (ibStart + cbSize); i++) {
					value1 = (value1 + array[i]) % modulo;
					value2 = (value2 + value1) % modulo;
				}
			}
		}


		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				byte[] temp = null;

				if (parameters.Order == 32) {
					temp = new byte[] {
						(byte)((value2 >> 8) & 0xFF),
						(byte)(value2 & 0xFF),
						(byte)((value1 >> 8) & 0xFF),
						(byte)(value1 & 0xFF)
					};
				} else if (parameters.Order == 16) {
					temp = new byte[] {
						(byte)((value2) & 0xFF),
						(byte)(value1 & 0xFF)
					};
				} else if (parameters.Order == 8) {
					temp = new byte[] {
						(byte)((value2 << 4 | value1) & 0xFF)
					};
				}
				
				return temp;
			}
		}
	}
}
