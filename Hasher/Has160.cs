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
	/// <summary>Computes the HAS-160 hash for the input data using the managed library.</summary>
	public class Has160 : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		private uint[] accumulator = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0xC3D2E1F0 };


		/// <summary>Initializes a new instance of the HAS-160 class.</summary>
		public Has160() : base(64) {
			HashSizeValue = 160;
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				accumulator = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0xC3D2E1F0 };
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
			uint A = accumulator[0];
			uint B = accumulator[1];
			uint C = accumulator[2];
			uint D = accumulator[3];
			uint E = accumulator[4];
			uint T, i;
			uint[] w = new uint[20];

			for (i = 0; i < 16; i++) {
				w[i] = (uint)(inputBuffer[inputOffset + (i * 4)] | (inputBuffer[inputOffset + (i * 4) + 1] << 8) | (inputBuffer[inputOffset + (i * 4) + 2] << 16) | (inputBuffer[inputOffset + (i * 4) + 3] << 24));
			}

			w[16] = w[ 0] ^ w[ 1] ^ w[ 2] ^ w[ 3];
			w[17] = w[ 4] ^ w[ 5] ^ w[ 6] ^ w[ 7];
			w[18] = w[ 8] ^ w[ 9] ^ w[10] ^ w[11];
			w[19] = w[12] ^ w[13] ^ w[14] ^ w[15];
			for (i = 0; i < 20; i++) {
				T = BitTools.RotateLeft(A, rot[i]) + ((B & C) | (~B & D)) + E + w[ndx[i]];
				E = D;
				D = C;
				C = B << 10 | B >> 22;
				B = A;
				A = T;
			}

			w[16] = w[ 3] ^ w[ 6] ^ w[ 9] ^ w[12];
			w[17] = w[ 2] ^ w[ 5] ^ w[ 8] ^ w[15];
			w[18] = w[ 1] ^ w[ 4] ^ w[11] ^ w[14];
			w[19] = w[ 0] ^ w[ 7] ^ w[10] ^ w[13];
			for (i = 20; i < 40; i++) {
				T = BitTools.RotateLeft(A, rot[i - 20]) + (B ^ C ^ D) + E + w[ndx[i]] + 0x5A827999;
				E = D;
				D = C;
				C = B << 17 | B >> 15;
				B = A;
				A = T;
			}

			w[16] = w[ 5] ^ w[ 7] ^ w[12] ^ w[14];
			w[17] = w[ 0] ^ w[ 2] ^ w[ 9] ^ w[11];
			w[18] = w[ 4] ^ w[ 6] ^ w[13] ^ w[15];
			w[19] = w[ 1] ^ w[ 3] ^ w[ 8] ^ w[10];
			for (i = 40; i < 60; i++) {
				T = BitTools.RotateLeft(A, rot[i - 40]) + (C ^ (B | ~D)) + E + w[ndx[i]] + 0x6ED9EBA1;
				E = D;
				D = C;
				C = B << 25 | B >> 7;
				B = A;
				A = T;
			}

			w[16] = w[ 2] ^ w[ 7] ^ w[ 8] ^ w[13];
			w[17] = w[ 3] ^ w[ 4] ^ w[ 9] ^ w[14];
			w[18] = w[ 0] ^ w[ 5] ^ w[10] ^ w[15];
			w[19] = w[ 1] ^ w[ 6] ^ w[11] ^ w[12];
			for (i = 60; i < 80; i++) {
				T = BitTools.RotateLeft(A, rot[i - 60]) + (B ^ C ^ D) + E + w[ndx[i]] + 0x8F1BBCDC;
				E = D;
				D = C;
				C = B << 30 | B >> 2;
				B = A;
				A = T;
			}

			accumulator[0] += A;
			accumulator[1] += B;
			accumulator[2] += C;
			accumulator[3] += D;
			accumulator[4] += E;
			}
		}


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		override protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			lock (syncLock) {
				byte[] temp;
				int paddingSize;
				ulong size;

				// Figure out how much padding is needed between the last byte and the size.
				paddingSize = (int)(((ulong)inputCount + (ulong)Count) % (ulong)BlockSize);
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

		#region Tables
		private static int[] rot = new int[] {
			5, 11,  7, 15,  6, 13,  8, 14,  7, 12,  9, 11,  8, 15,  6, 12,  9, 14,  5, 13
		};

		private static int[] ndx = new int[] {
			18,  0,  1,  2,  3, 19,  4,  5, 6,  7, 16,  8,  9, 10, 11, 17, 12, 13, 14, 15,
			18,  3,  6,  9, 12, 19, 15,  2, 5,  8, 16, 11, 14,  1,  4, 17,  7, 10, 13,  0,
			18, 12,  5, 14,  7, 19,  0,  9, 2, 11, 16,  4, 13,  6, 15, 17,  8,  1, 10,  3,
			18,  7,  2, 13,  8, 19,  3, 14, 9,  4, 16, 15, 10,  5,  0, 17, 11,  6,  1, 12
		};
		#endregion
	}
}
