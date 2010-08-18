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

namespace Classless.Hasher.Utilities {
	/// <summary>A container of static type conversion functions.</summary>
	sealed public class Conversions {
		/// <summary>Prevent the compiler from making an unneeded default public constructor.</summary>
		private Conversions() {}


		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public ushort[] ByteToUShort(byte[] array) { return ByteToUShort(array, 0, array.Length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="endian">The order in which to read the bytes.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public ushort[] ByteToUShort(byte[] array, EndianType endian) { return ByteToUShort(array, 0, array.Length, endian); }
		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to convert.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public ushort[] ByteToUShort(byte[] array, int offset, int length) { return ByteToUShort(array, offset, length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to convert.</param>
		/// <param name="endian">The order in which to read the bytes.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid, or if the length specified does not represent full UShort values.</exception>
		[CLSCompliant(false)]
		static public ushort[] ByteToUShort(byte[] array, int offset, int length, EndianType endian) {
			if ((length + offset) > array.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if ((length % 2) != 0) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthMultipleOfTypeSize, "length");
			}
			
			ushort[] temp = new ushort[length / 2];

			for (int i = 0, j = offset; i < temp.Length; i++) {
				if (endian == EndianType.LittleEndian) {
					temp[i] = (ushort)((((uint)array[j++] & 0xFF)      ) |
									   (((uint)array[j++] & 0xFF) <<  8));
				} else {
					temp[i] = (ushort)((((uint)array[j++] & 0xFF) <<  8) |
									   (((uint)array[j++] & 0xFF)      ));
				}
			}

			return temp;
		}


		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public uint[] ByteToUInt(byte[] array) { return ByteToUInt(array, 0, array.Length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="endian">The order in which to read the bytes.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public uint[] ByteToUInt(byte[] array, EndianType endian) { return ByteToUInt(array, 0, array.Length, endian); }
		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to convert.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public uint[] ByteToUInt(byte[] array, int offset, int length) { return ByteToUInt(array, offset, length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to convert.</param>
		/// <param name="endian">The order in which to read the bytes.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid, or if the length specified does not represent full UInt values.</exception>
		[CLSCompliant(false)]
		static public uint[] ByteToUInt(byte[] array, int offset, int length, EndianType endian) {
			if ((length + offset) > array.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if ((length % 4) != 0) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthMultipleOfTypeSize, "length");
			}

			uint[] temp = new uint[length / 4];

			for (int i = 0, j = offset; i < temp.Length; i++) {
				if (endian == EndianType.LittleEndian) {
					temp[i] = (((uint)array[j++] & 0xFF)      ) |
							  (((uint)array[j++] & 0xFF) <<  8) |
							  (((uint)array[j++] & 0xFF) << 16) |
							  (((uint)array[j++] & 0xFF) << 24);
				} else {
					temp[i] = (((uint)array[j++] & 0xFF) << 24) |
							  (((uint)array[j++] & 0xFF) << 16) |
							  (((uint)array[j++] & 0xFF) <<  8) |
							  (((uint)array[j++] & 0xFF)      );
				}
			}

			return temp;
		}


		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public ulong[] ByteToULong(byte[] array) { return ByteToULong(array, 0, array.Length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="endian">The order in which to read the bytes.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public ulong[] ByteToULong(byte[] array, EndianType endian) { return ByteToULong(array, 0, array.Length, endian); }
		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to convert.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public ulong[] ByteToULong(byte[] array, int offset, int length) { return ByteToULong(array, offset, length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to convert.</param>
		/// <param name="endian">The order in which to read the bytes.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid, or if the length specified does not represent full ULong values.</exception>
		[CLSCompliant(false)]
		static public ulong[] ByteToULong(byte[] array, int offset, int length, EndianType endian) {
			if ((length + offset) > array.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if ((length % 8) != 0) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthMultipleOfTypeSize, "length");
			}

			ulong[] temp = new ulong[length / 8];

			for (int i = 0, j = offset; i < temp.Length; i++) {
				if (endian == EndianType.LittleEndian) {
					temp[i] = (((ulong)array[j++] & 0xFF)      ) |
							  (((ulong)array[j++] & 0xFF) <<  8) |
							  (((ulong)array[j++] & 0xFF) << 16) |
							  (((ulong)array[j++] & 0xFF) << 24) |
							  (((ulong)array[j++] & 0xFF) << 32) |
							  (((ulong)array[j++] & 0xFF) << 40) |
							  (((ulong)array[j++] & 0xFF) << 48) |
							  (((ulong)array[j++] & 0xFF) << 56);
				} else {
					temp[i] = (((ulong)array[j++] & 0xFF) << 56) |
							  (((ulong)array[j++] & 0xFF) << 48) |
							  (((ulong)array[j++] & 0xFF) << 40) |
							  (((ulong)array[j++] & 0xFF) << 32) |
							  (((ulong)array[j++] & 0xFF) << 24) |
							  (((ulong)array[j++] & 0xFF) << 16) |
							  (((ulong)array[j++] & 0xFF) <<  8) |
							  (((ulong)array[j++] & 0xFF)      );
				}
			}

			return temp;
		}


		/// <summary>Converts an unsigned short integer to an array of bytes.</summary>
		/// <param name="data">The unsigned short integer to convert.</param>
		/// <returns>The unsigned short integer represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] UShortToByte(ushort data) { return UShortToByte(new ushort[1]{data}, 0, 1, EndianType.LittleEndian); }
		/// <summary>Converts an unsigned short integer to an array of bytes.</summary>
		/// <param name="data">The unsigned short integer to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned short integer represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public byte[] UShortToByte(ushort data, EndianType endian) { return UShortToByte(new ushort[1]{data}, 0, 1, endian); }
		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] UShortToByte(ushort[] array) { return UShortToByte(array, 0, array.Length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public byte[] UShortToByte(ushort[] array, EndianType endian) { return UShortToByte(array, 0, array.Length, endian); }
		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many unsigned short integers in the array to convert.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] UShortToByte(ushort[] array, int offset, int length) { return UShortToByte(array, offset, length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned short integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many unsigned short integers in the array to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned short integers represented as a byte array.</returns>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When the length specified is longer than this implementation supports.</exception>
		[CLSCompliant(false)]
		static public byte[] UShortToByte(ushort[] array, int offset, int length, EndianType endian) {
			if ((length + offset) > array.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if (length > (int.MaxValue / 2)) {
				throw new ArgumentOutOfRangeException("length", length, Properties.Resources.lengthWayTooLong);
			}

			byte[] temp = new byte[length * 2];

			for (int i = offset; i < (offset + length); i++) {
				for (int j = 0; j < 2; j++) {
					if (endian == EndianType.LittleEndian) {
						temp[(i - offset) * 2 + j] = (byte)(array[i] >> (j * 8));
					} else {
						temp[(i - offset) * 2 + (1 - j)] = (byte)(array[i] >> (j * 8));
					}
				}
			}

			return temp;
		}


		/// <summary>Converts an unsigned integer to an array of bytes.</summary>
		/// <param name="data">The unsigned integer to convert.</param>
		/// <returns>The unsigned integer represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] UIntToByte(uint data) { return UIntToByte(new uint[1]{data}, 0, 1, EndianType.LittleEndian); }
		/// <summary>Converts an unsigned integer to an array of bytes.</summary>
		/// <param name="data">The unsigned integer to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned integer represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public byte[] UIntToByte(uint data, EndianType endian) { return UIntToByte(new uint[1]{data}, 0, 1, endian); }
		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] UIntToByte(uint[] array) { return UIntToByte(array, 0, array.Length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public byte[] UIntToByte(uint[] array, EndianType endian) { return UIntToByte(array, 0, array.Length, endian); }
		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many unsigned integers in the array to convert.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] UIntToByte(uint[] array, int offset, int length) { return UIntToByte(array, offset, length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many unsigned integers in the array to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned integers represented as a byte array.</returns>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When the length specified is longer than this implementation supports.</exception>
		[CLSCompliant(false)]
		static public byte[] UIntToByte(uint[] array, int offset, int length, EndianType endian) {
			if ((length + offset) > array.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if (length > (int.MaxValue / 4)) {
				throw new ArgumentOutOfRangeException("length", length, Properties.Resources.lengthWayTooLong);
			}

			byte[] temp = new byte[length * 4];

			for (int i = offset; i < (offset + length); i++) {
				for (int j = 0; j < 4; j++) {
					if (endian == EndianType.LittleEndian) {
						temp[(i - offset) * 4 + j] = (byte)(array[i] >> (j * 8));
					} else {
						temp[(i - offset) * 4 + (3 - j)] = (byte)(array[i] >> (j * 8));
					}
				}
			}

			return temp;
		}


		/// <summary>Converts an unsigned long integer to an array of bytes.</summary>
		/// <param name="data">The unsigned long integer to convert.</param>
		/// <returns>The unsigned long integer represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] ULongToByte(ulong data) { return ULongToByte(new ulong[1]{data}, 0, 1, EndianType.LittleEndian); }
		/// <summary>Converts an unsigned long integer to an array of bytes.</summary>
		/// <param name="data">The unsigned long integer to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned long integer represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public byte[] ULongToByte(ulong data, EndianType endian) { return ULongToByte(new ulong[1]{data}, 0, 1, endian); }
		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] ULongToByte(ulong[] array) { return ULongToByte(array, 0, array.Length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		[CLSCompliant(false)]
		static public byte[] ULongToByte(ulong[] array, EndianType endian) { return ULongToByte(array, 0, array.Length, endian); }
		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many unsigned long integers in the array to convert.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		/// <remarks>Assumes Little Endian.</remarks>
		[CLSCompliant(false)]
		static public byte[] ULongToByte(ulong[] array, int offset, int length) { return ULongToByte(array, offset, length, EndianType.LittleEndian); }
		/// <summary>Converts an array of unsigned long integers to an array of bytes.</summary>
		/// <param name="array">The array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many unsigned long integers in the array to convert.</param>
		/// <param name="endian">The order in which to store the bytes.</param>
		/// <returns>The unsigned long integers represented as a byte array.</returns>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When the length specified is longer than this implementation supports.</exception>
		[CLSCompliant(false)]
		static public byte[] ULongToByte(ulong[] array, int offset, int length, EndianType endian) {
			if ((length + offset) > array.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if (length > (int.MaxValue / 8)) {
				throw new ArgumentOutOfRangeException("length", length, Properties.Resources.lengthWayTooLong);
			}

			byte[] temp = new byte[length * 8];

			for (int i = offset; i < (offset + length); i++) {
				for (int j = 0; j < 8; j++) {
					if (endian == EndianType.LittleEndian) {
						temp[(i - offset) * 8 + j] = (byte)(array[i] >> (j * 8));
					} else {
						temp[(i - offset) * 8 + (7 - j)] = (byte)(array[i] >> (j * 8));
					}
				}
			}

			return temp;
		}


		/// <summary>Convert a byte into a hexadecimal string.</summary>
		/// <param name="data">The byte to convert.</param>
		/// <returns>A string containing the hexadecimal equivelent of the byte.</returns>
		static public string ByteToHexadecimal(byte data) { return ByteToHexadecimal(new byte[1]{data}, 0, 1); }
		/// <summary>Convert a byte array into a hexadecimal string.</summary>
		/// <param name="array">The byte array to convert.</param>
		/// <returns>A string containing the hexadecimal equivelent of the byte array.</returns>
		static public string ByteToHexadecimal(byte[] array) { return ByteToHexadecimal(array, 0, array.Length); }
		/// <summary>Convert a byte array into a hexadecimal string.</summary>
		/// <param name="array">The byte array to convert.</param>
		/// <param name="offset">Position in the array to begin the conversion.</param>
		/// <param name="length">How many bytes in the array to convert.</param>
		/// <returns>A string containing the hexadecimal equivelent of the byte array.</returns>
		/// <exception cref="ArgumentException">When the range specified for the array is invalid.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When the length specified is longer than this implementation supports.</exception>
		static public string ByteToHexadecimal(byte[] array, int offset, int length) {
			if ((length + offset) > array.Length) {
				throw new ArgumentException(Hasher.Properties.Resources.lengthOffsetOutOfBounds);
			}
			if (length > (int.MaxValue / 2)) {
				throw new ArgumentOutOfRangeException("length", length, Properties.Resources.lengthWayTooLong);
			}

			System.Text.StringBuilder temp = new System.Text.StringBuilder(2 * length);

			for (int i = offset; i < (offset + length); i++) {
				temp.Append(array[i].ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
			}

			return temp.ToString();
		}


		/// <summary>Convert a hexadecimal string into a byte array.</summary>
		/// <param name="data">The hexadecimal string to convert.</param>
		/// <returns>The hexadecimal string represented as a byte array.</returns>
		static public byte[] HexadecimalToByte(string data) {
			byte[] temp = new byte[data.Length / 2];

			for (int i = 0; i < temp.Length; i++) {
				temp[i] = byte.Parse(data.Substring((i * 2), 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
			}

			return temp;
		}
	}
}
