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
using Classless.Hasher.Utilities;

namespace Classless.Hasher {
	/// <summary>A class that contains the parameters necessary to initialize a CRC algorithm.</summary>
	public class CrcParameters : HashAlgorithmParameters {
		private int order;
		private long polynomial;
		private long initial;
		private long finalXor;
		private bool reflectIn;


		/// <summary>Gets or sets the order of the CRC (e.g., how many bits).</summary>
		/// <exception cref="ArgumentOutOfRangeException">When the specified value is not a multiple of 8, is less than 8, or is greater than 64.</exception>
		public int Order {
			get { return order; }
			set {
				if (((value % 8) != 0) || (value < 8) || (value > 64)) {
					throw new ArgumentOutOfRangeException("value", value, Hasher.Properties.Resources.invalidCrcOrder);
				} else {
					order = value;
				}
			}
		}

		/// <summary>Gets or sets the polynomial to use in the CRC calculations.</summary>
		public long Polynomial {
			get { return polynomial; }
			set { polynomial = value; }
		}

		/// <summary>Gets or sets the initial value of the CRC.</summary>
		public long InitialValue {
			get { return initial; }
			set { initial = value; }
		}

		/// <summary>Gets or sets the final value to XOR with the CRC.</summary>
		public long FinalXorValue {
			get { return finalXor; }
			set { finalXor = value; }
		}

		/// <summary>Gets or sets the value dictating whether or not to reflect the incoming data before calculating. (UART)</summary>
		public bool ReflectInput {
			get { return reflectIn; }
			set { reflectIn = value; }
		}


		/// <summary>Initializes a new instance of the CRCParamters class.</summary>
		/// <param name="order">The order of the CRC (e.g., how many bits).</param>
		/// <param name="polynomial">The polynomial to use in the calculations.</param>
		/// <param name="initial">The initial value of the CRC.</param>
		/// <param name="finalXor">The final value to XOR with the CRC.</param>
		/// <param name="reflectIn">Whether or not to reflect the incoming data before calculating.</param>
		public CrcParameters(int order, long polynomial, long initial, long finalXor, bool reflectIn) {
			this.Order = order;
			this.Polynomial = polynomial;
			this.InitialValue = initial;
			this.FinalXorValue = finalXor;
			this.ReflectInput = reflectIn;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:d}:{2:d}:{3:d}:{4:d}:{5})", this.GetType().Name, Order, Polynomial, InitialValue, FinalXorValue, ReflectInput);
		}


		/// <summary>Retrieves a standard set of CRC parameters.</summary>
		/// <param name="standard">The name of the standard parameter set to retrieve.</param>
		/// <returns>The CRC Parameters for the given standard.</returns>
		public static CrcParameters GetParameters(CrcStandard standard) {
			CrcParameters param = null;

			switch (standard) {
				case CrcStandard.Crc8Bit: param = new CrcParameters(8, 0x07, 0, 0, false); break;
				case CrcStandard.Crc8BitIcode: param = new CrcParameters(8, 0x1D, 0xFD, 0, false); break;
				case CrcStandard.Crc8BitItu: param = new CrcParameters(8, 0x07, 0, 0x55, false); break;
				case CrcStandard.Crc8BitMaxim: param = new CrcParameters(8, 0x31, 0, 0, true); break;
				case CrcStandard.Crc8BitWcdma: param = new CrcParameters(8, 0x9B, 0, 0, true); break;
				case CrcStandard.Crc16Bit: param = new CrcParameters(16, 0x8005, 0, 0, true); break;
				case CrcStandard.Crc16BitArc: goto case CrcStandard.Crc16Bit;
				case CrcStandard.Crc16BitIbm: goto case CrcStandard.Crc16Bit;
				case CrcStandard.Crc16BitLha: goto case CrcStandard.Crc16Bit;
				case CrcStandard.Crc16BitCcitt: param = new CrcParameters(16, 0x1021, 0, 0, true); break;
				case CrcStandard.Crc16BitKermit: goto case CrcStandard.Crc16BitCcitt;
				case CrcStandard.Crc16BitCcittFalse: param = new CrcParameters(16, 0x1021, 0xFFFF, 0, false); break;
				case CrcStandard.Crc16BitMaxim: param = new CrcParameters(16, 0x8005, 0, 0xFFFF, true); break;
				case CrcStandard.Crc16BitUsb: param = new CrcParameters(16, 0x8005, 0xFFFF, 0xFFFF, true); break;
				case CrcStandard.Crc16BitX25: param = new CrcParameters(16, 0x1021, 0xFFFF, 0xFFFF, true); break;
				case CrcStandard.Crc16BitXmodem: param = new CrcParameters(16, 0x1021, 0, 0, false); break;
				case CrcStandard.Crc16BitZmodem: goto case CrcStandard.Crc16BitXmodem;
				case CrcStandard.Crc16BitXkermit: param = new CrcParameters(16, 0x8408, 0, 0, true); break;
				case CrcStandard.Crc24Bit: param = new CrcParameters(24, 0x864CFB, 0xB704CE, 0, false); break;
				case CrcStandard.Crc24BitOpenPgp: goto case CrcStandard.Crc24Bit;
				case CrcStandard.Crc32Bit: param = new CrcParameters(32, 0x04C11DB7, 0xFFFFFFFF, 0xFFFFFFFF, true); break;
				case CrcStandard.Crc32BitPkzip: goto case CrcStandard.Crc32Bit;
				case CrcStandard.Crc32BitItu: goto case CrcStandard.Crc32Bit;
				case CrcStandard.Crc32BitBzip2: param = new CrcParameters(32, 0x04C11DB7, 0xFFFFFFFF, 0xFFFFFFFF, false); break;
				case CrcStandard.Crc32BitIscsi: param = new CrcParameters(32, 0x1EDC6F41, 0xFFFFFFFF, 0xFFFFFFFF, true); break;
				case CrcStandard.Crc32BitJam: param = new CrcParameters(32, 0x04C11DB7, 0xFFFFFFFF, 0, true); break;
				case CrcStandard.Crc32BitPosix: param = new CrcParameters(32, 0x04C11DB7, 0, 0xFFFFFFFF, false); break;
				case CrcStandard.Crc32BitCksum: goto case CrcStandard.Crc32BitPosix;
				case CrcStandard.Crc32BitMpeg2: param = new CrcParameters(32, 0x04C11DB7, 0xFFFFFFFF, 0, false); break;
				case CrcStandard.Crc64Bit: param = new CrcParameters(64, 0x42F0E1EBA9EA3693, 0, 0, false); break;
				case CrcStandard.Crc64BitWE: param = new CrcParameters(64, (long)0x42F0E1EBA9EA3693, -1, -1, false); break;
				case CrcStandard.Crc64BitIso: param = new CrcParameters(64, 0x000000000000001B, 0, 0, true); break;
				case CrcStandard.Crc64BitJones: param = new CrcParameters(64, unchecked((long)0xAD93D23594C935A9), 0, 0, true); break;
			}

			return param;
		}
	}
}
