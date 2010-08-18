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
	/// <summary>Predefined standards for CRC algorithms.</summary>
	public enum CrcStandard {
		/// <summary>8bit CRC</summary>
		Crc8Bit,

		/// <summary>8bit CRC</summary>
		Crc8BitIcode,

		/// <summary>8bit CRC</summary>
		Crc8BitItu,

		/// <summary>8bit CRC</summary>
		Crc8BitMaxim,

		/// <summary>8bit CRC</summary>
		Crc8BitWcdma,

		/// <summary>16bit CRC</summary>
		Crc16Bit,

		/// <summary>16bit CRC; Alias for Crc16Bit</summary>
		Crc16BitArc,

		/// <summary>16bit CRC; Alias for Crc16Bit</summary>
		Crc16BitIbm,

		/// <summary>16bit CRC; Alias for Crc16Bit</summary>
		Crc16BitLha,

		/// <summary>16bit CRC</summary>
		Crc16BitCcitt,

		/// <summary>16bit CRC; Alias for Crc16BitCcitt</summary>
		Crc16BitKermit,

		/// <summary>16bit CRC</summary>
		Crc16BitCcittFalse,

		/// <summary>16bit CRC</summary>
		Crc16BitMaxim,

		/// <summary>16bit CRC</summary>
		Crc16BitUsb,

		/// <summary>16bit CRC</summary>
		Crc16BitX25,

		/// <summary>16bit CRC</summary>
		Crc16BitXmodem,

		/// <summary>16bit CRC; Alias for Crc16BitXmodem</summary>
		Crc16BitZmodem,

		/// <summary>16bit CRC</summary>
		Crc16BitXkermit,

		/// <summary>24bit CRC</summary>
		Crc24Bit,

		/// <summary>24bit CRC; Alias for Crc24Bit</summary>
		Crc24BitOpenPgp,

		/// <summary>32bit CRC</summary>
		Crc32Bit,

		/// <summary>32bit CRC; Alias for Crc32Bit</summary>
		Crc32BitPkzip,

		/// <summary>32bit CRC; Alias for Crc32Bit</summary>
		Crc32BitItu,

		/// <summary>32bit CRC</summary>
		Crc32BitBzip2,

		/// <summary>32bit CRC</summary>
		Crc32BitIscsi,

		/// <summary>32bit CRC</summary>
		Crc32BitJam,

		/// <summary>32bit CRC</summary>
		Crc32BitPosix,

		/// <summary>32bit CRC; Alias for Crc32BitPosix</summary>
		Crc32BitCksum,

		/// <summary>32bit CRC</summary>
		Crc32BitMpeg2,

		/// <summary>64bit CRC</summary>
		Crc64Bit,

		/// <summary>64bit CRC</summary>
		Crc64BitWE,

		/// <summary>64bit CRC</summary>
		Crc64BitIso,

		/// <summary>64bit CRC</summary>
		Crc64BitJones,
	}
}
