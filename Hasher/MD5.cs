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
	/// <summary>Computes the MD5 hash for the input data using the managed library.</summary>
	public class MD5 : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		internal uint[] accumulator = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476 };


		/// <summary>Initializes a new instance of the MD5 class.</summary>
		public MD5() : base(64) {
			HashSizeValue = 128;
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				accumulator = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476 };
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				uint[] workBuffer;
				uint a = accumulator[0];
				uint b = accumulator[1];
				uint c = accumulator[2];
				uint d = accumulator[3];

				workBuffer = Conversions.ByteToUInt(inputBuffer, inputOffset, BlockSize);

				#region Round 1
				a = FF(a, b, c, d, workBuffer[ 0],  7, 0xD76AA478);
				d = FF(d, a, b, c, workBuffer[ 1], 12, 0xE8C7B756);
				c = FF(c, d, a, b, workBuffer[ 2], 17, 0x242070DB);
				b = FF(b, c, d, a, workBuffer[ 3], 22, 0xC1BDCEEE);
				a = FF(a, b, c, d, workBuffer[ 4],  7, 0xF57C0FAF);
				d = FF(d, a, b, c, workBuffer[ 5], 12, 0x4787C62A);
				c = FF(c, d, a, b, workBuffer[ 6], 17, 0xA8304613);
				b = FF(b, c, d, a, workBuffer[ 7], 22, 0xFD469501);
				a = FF(a, b, c, d, workBuffer[ 8],  7, 0x698098D8);
				d = FF(d, a, b, c, workBuffer[ 9], 12, 0x8B44F7AF);
				c = FF(c, d, a, b, workBuffer[10], 17, 0xFFFF5BB1);
				b = FF(b, c, d, a, workBuffer[11], 22, 0x895CD7BE);
				a = FF(a, b, c, d, workBuffer[12],  7, 0x6B901122);
				d = FF(d, a, b, c, workBuffer[13], 12, 0xFD987193);
				c = FF(c, d, a, b, workBuffer[14], 17, 0xA679438E);
				b = FF(b, c, d, a, workBuffer[15], 22, 0x49B40821);
				#endregion

				#region Round 2
				a = GG(a, b, c, d, workBuffer[ 1],  5, 0xF61E2562);
				d = GG(d, a, b, c, workBuffer[ 6],  9, 0xC040B340);
				c = GG(c, d, a, b, workBuffer[11], 14, 0x265E5A51);
				b = GG(b, c, d, a, workBuffer[ 0], 20, 0xE9B6C7AA);
				a = GG(a, b, c, d, workBuffer[ 5],  5, 0xD62F105D);
				d = GG(d, a, b, c, workBuffer[10],  9, 0x02441453);
				c = GG(c, d, a, b, workBuffer[15], 14, 0xD8A1E681);
				b = GG(b, c, d, a, workBuffer[ 4], 20, 0xE7D3FBC8);
				a = GG(a, b, c, d, workBuffer[ 9],  5, 0x21E1CDE6);
				d = GG(d, a, b, c, workBuffer[14],  9, 0xC33707D6);
				c = GG(c, d, a, b, workBuffer[ 3], 14, 0xF4D50D87);
				b = GG(b, c, d, a, workBuffer[ 8], 20, 0x455A14ED);
				a = GG(a, b, c, d, workBuffer[13],  5, 0xA9E3E905);
				d = GG(d, a, b, c, workBuffer[ 2],  9, 0xFCEFA3F8);
				c = GG(c, d, a, b, workBuffer[ 7], 14, 0x676F02D9);
				b = GG(b, c, d, a, workBuffer[12], 20, 0x8D2A4C8A);
				#endregion

				#region Round 3
				a = HH(a, b, c, d, workBuffer[ 5],  4, 0xFFFA3942);
				d = HH(d, a, b, c, workBuffer[ 8], 11, 0x8771F681);
				c = HH(c, d, a, b, workBuffer[11], 16, 0x6D9D6122);
				b = HH(b, c, d, a, workBuffer[14], 23, 0xFDE5380C);
				a = HH(a, b, c, d, workBuffer[ 1],  4, 0xA4BEEA44);
				d = HH(d, a, b, c, workBuffer[ 4], 11, 0x4BDECFA9);
				c = HH(c, d, a, b, workBuffer[ 7], 16, 0xF6BB4B60);
				b = HH(b, c, d, a, workBuffer[10], 23, 0xBEBFBC70);
				a = HH(a, b, c, d, workBuffer[13],  4, 0x289B7EC6);
				d = HH(d, a, b, c, workBuffer[ 0], 11, 0xEAA127FA);
				c = HH(c, d, a, b, workBuffer[ 3], 16, 0xD4EF3085);
				b = HH(b, c, d, a, workBuffer[ 6], 23, 0x04881D05);
				a = HH(a, b, c, d, workBuffer[ 9],  4, 0xD9D4D039);
				d = HH(d, a, b, c, workBuffer[12], 11, 0xE6DB99E5);
				c = HH(c, d, a, b, workBuffer[15], 16, 0x1FA27CF8);
				b = HH(b, c, d, a, workBuffer[ 2], 23, 0xC4AC5665);
				#endregion

				#region Round 4
				a = II(a, b, c, d, workBuffer[ 0],  6, 0xF4292244);
				d = II(d, a, b, c, workBuffer[ 7], 10, 0x432AFF97);
				c = II(c, d, a, b, workBuffer[14], 15, 0xAB9423A7);
				b = II(b, c, d, a, workBuffer[ 5], 21, 0xFC93A039);
				a = II(a, b, c, d, workBuffer[12],  6, 0x655B59C3);
				d = II(d, a, b, c, workBuffer[ 3], 10, 0x8F0CCC92);
				c = II(c, d, a, b, workBuffer[10], 15, 0xFFEFF47D);
				b = II(b, c, d, a, workBuffer[ 1], 21, 0x85845DD1);
				a = II(a, b, c, d, workBuffer[ 8],  6, 0x6FA87E4F);
				d = II(d, a, b, c, workBuffer[15], 10, 0xFE2CE6E0);
				c = II(c, d, a, b, workBuffer[ 6], 15, 0xA3014314);
				b = II(b, c, d, a, workBuffer[13], 21, 0x4E0811A1);
				a = II(a, b, c, d, workBuffer[ 4],  6, 0xF7537E82);
				d = II(d, a, b, c, workBuffer[11], 10, 0xBD3AF235);
				c = II(c, d, a, b, workBuffer[ 2], 15, 0x2AD7D2BB);
				b = II(b, c, d, a, workBuffer[ 9], 21, 0xEB86D391);
				#endregion

				accumulator[0] += a;
				accumulator[1] += b;
				accumulator[2] += c;
				accumulator[3] += d;
			}
		}


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many blocks have been processed so far.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		override protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			lock (syncLock) {
				byte[] temp;
				int paddingSize;
				ulong size;

				// Figure out how much padding is needed between the last byte and the size.
				paddingSize = (int)((inputCount + Count) % BlockSize);
				paddingSize = (BlockSize - 8) - paddingSize;
				if (paddingSize < 1) { paddingSize += BlockSize; }

				// Create the final, padded block(s).
				temp = new byte[inputCount + paddingSize + 8];
				Array.Copy(inputBuffer, inputOffset, temp, 0, inputCount);
				temp[inputCount] = 0x80;
				size = ((ulong)Count + (ulong)inputCount) * 8;
				Array.Copy(Conversions.ULongToByte(size), 0, temp, (inputCount + paddingSize), 8);

				// Push the final block(s) into the calculation.
				ProcessBlock(temp, 0);
				if (temp.Length == (BlockSize * 2)) {
					ProcessBlock(temp, BlockSize);
				}

				return Conversions.UIntToByte(accumulator);
			}
		}


		static private uint FF(uint a, uint b, uint c, uint d, uint k, int s, uint t) {
			a += k + t + (d ^ (b & (c ^ d)));
			a = BitTools.RotateLeft(a, s);
			return a + b;
		}

		static private uint GG(uint a, uint b, uint c, uint d, uint k, int s, uint t) {
			a += k + t + (c ^ (d & (b ^ c)));
			a = BitTools.RotateLeft(a, s);
			return a + b;
		}

		static private uint HH(uint a, uint b, uint c, uint d, uint k, int s, uint t) {
			a += k + t + (b ^ c ^ d);
			a = BitTools.RotateLeft(a, s);
			return a + b;
		}

		static private uint II(uint a, uint b, uint c, uint d, uint k, int s, uint t) {
			a += k + t + (c  ^ (b | ~d));
			a = BitTools.RotateLeft(a, s);
			return a + b;
		}
	}
}
