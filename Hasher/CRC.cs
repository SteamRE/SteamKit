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
using System.Collections;

namespace Classless.Hasher {
	/// <summary>Computes the CRC hash for the input data using the managed library.</summary>
	public class Crc : HashAlgorithm, IParametrizedHashAlgorithm {
		private readonly object syncLock = new object();

		static private Hashtable lookupTables;

		private CrcParameters parameters;
		private long[] lookup;
		private long checksum;
		private long registerMask;


		/// <summary>Gets the HashAlgorithmParameters being used by this HashAlgorithm.</summary>
		public HashAlgorithmParameters Parameters {
			get { return parameters; }
		}


		/// <summary>Initializes a new instance of the CRC class.</summary>
		/// <remarks>This constructor implements the default parameters of Crc32Bit.</remarks>
		public Crc() : this(CrcParameters.GetParameters(CrcStandard.Crc32Bit)) { }

		/// <summary>Initializes a new instance of the CRC class.</summary>
		/// <param name="parameters">The parameters to utilize in the CRC calculation.</param>
		/// <exception cref="ArgumentNullException">When the specified parameters are null.</exception>
		public Crc(CrcParameters parameters) : base() {
			lock (syncLock) {
				if (parameters == null) { throw new ArgumentNullException("parameters", Hasher.Properties.Resources.paramCantBeNull); }
				this.parameters = parameters;
				HashSizeValue = this.parameters.Order;

				Crc.BuildLookup(this.parameters);
				lookup = (long[])lookupTables[this.parameters];
				if (this.parameters.Order == 64) {
					registerMask = 0x00FFFFFFFFFFFFFF;
				} else {
					registerMask = (long)(Math.Pow(2, (this.parameters.Order - 8)) - 1);
				}

				checksum = this.parameters.InitialValue;
				if (this.parameters.ReflectInput) {
					checksum = Reflect(checksum, this.parameters.Order);
				}
			}
		}


		// Pre-build the more popular lookup tables.
		static Crc() {
			lookupTables = new Hashtable();
			BuildLookup(CrcParameters.GetParameters(CrcStandard.Crc32Bit));
		}


		/// <summary>Build the CRC lookup table for a given polynomial.</summary>
		static private void BuildLookup(CrcParameters param) {
			if (lookupTables.Contains(param)) {
				// No sense in creating the table twice.
				return;
			}

			long[] table = new long[256];
			long topBit = (long)1 << (param.Order - 1);
			long widthMask = (((1 << (param.Order - 1)) - 1) << 1) | 1;

			// Build the table.
			for (int i = 0; i < table.Length; i++) {
				table[i] = i;

				if (param.ReflectInput) { table[i] = Reflect((long)i, 8); }
				
				table[i] = table[i] << (param.Order - 8);

				for (int j = 0; j < 8; j++) {
					if ((table[i] & topBit) != 0) {
						table[i] = (table[i] << 1) ^ param.Polynomial;
					} else {
						table[i] <<= 1;
					}
				}

				if (param.ReflectInput) { table[i] = Reflect(table[i], param.Order); }

				table[i] &= widthMask;
			}

			// Add the new lookup table.
			lookupTables.Add(param, table);
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				checksum = parameters.InitialValue;
				if (parameters.ReflectInput) {
					checksum = Reflect(checksum, parameters.Order);
				}
			}
		}


		/// <summary>Drives the hashing function.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="ibStart">The position in the array to begin reading from.</param>
		/// <param name="cbSize">How many bytes in the array to read.</param>
		override protected void HashCore(byte[] array, int ibStart, int cbSize) {
			lock (syncLock) {
				for (int i = ibStart; i < (cbSize + ibStart); i++) {
					if (parameters.ReflectInput) {
						checksum = ((checksum >> 8) & registerMask) ^ lookup[(checksum ^ array[i]) & 0xFF];
					} else {
						checksum = (checksum << 8) ^ lookup[((checksum >> (parameters.Order - 8)) ^ array[i]) & 0xFF];
					}
				}
			}
		}


		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				int i, shift, numBytes;
				byte[] temp;

				checksum ^= parameters.FinalXorValue;

				numBytes = (int)parameters.Order / 8;
				temp = new byte[numBytes];
				for (i = (numBytes - 1), shift = 0; i >= 0; i--, shift += 8) {
					temp[i] = (byte)((checksum >> shift) & 0xFF);
				}

				return temp;
			}
		}


		/// <summary>Reflects the lower bits of the value provided.</summary>
		/// <param name="data">The value to reflect.</param>
		/// <param name="numBits">The number of bits to reflect.</param>
		/// <returns>The reflected value.</returns>
		static private long Reflect(long data, int numBits) {
			long temp = data;

			for (int i = 0; i < numBits; i++) {
				long bitMask = (long)1 << ((numBits - 1) - i);

				if ((temp & (long)1) != 0) {
					data |= bitMask;
				} else {
					data &= ~bitMask;
				}

				temp >>= 1;
			}

			return data;
		}
	}
}
