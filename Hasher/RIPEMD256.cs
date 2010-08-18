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
	/// <summary>Computes the RIPEMD256 hash for the input data using the managed library.</summary>
	public class RipeMD256 : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		private uint[] accumulator = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0x76543210, 0xFEDCBA98, 0x89ABCDEF, 0x01234567 };


		#region Tables
		private static int[] R = new int[] {
			0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15,
			7,  4, 13,  1, 10,  6, 15,  3, 12,  0,  9,  5,  2, 14, 11,  8,
			3, 10, 14,  4,  9, 15,  8,  1,  2,  7,  0,  6, 13, 11,  5, 12,
			1,  9, 11, 10,  0,  8, 12,  4, 13,  3,  7, 15, 14,  5,  6,  2
		};

		private static int[] Rp = new int[] {
			 5, 14,  7,  0,  9,  2, 11,  4, 13,  6, 15,  8,  1, 10,  3, 12,
			 6, 11,  3,  7,  0, 13,  5, 10, 14, 15,  8, 12,  4,  9,  1,  2,
			15,  5,  1,  3,  7, 14,  6,  9, 11,  8, 12,  2, 10,  0,  4, 13,
			 8,  6,  4,  1,  3, 11, 15,  0,  5, 12,  2, 13,  9,  7, 10, 14
		};

		private static int[] S = new int[] {
			11, 14, 15, 12,  5,  8,  7,  9, 11, 13, 14, 15,  6,  7,  9,  8,
			7,   6,  8, 13, 11,  9,  7, 15,  7, 12, 15,  9, 11,  7, 13, 12,
			11, 13,  6,  7, 14,  9, 13, 15, 14,  8, 13,  6,  5, 12,  7,  5,
			11, 12, 14, 15, 14, 15,  9,  8,  9, 14,  5,  6,  8,  6,  5, 12
		};

		private static int[] Sp = new int[] {
			 8,  9,  9, 11, 13, 15, 15,  5,  7,  7,  8, 11, 14, 14, 12,  6,
			 9, 13, 15,  7, 12,  8,  9, 11,  7,  7, 12,  7,  6, 15, 13, 11,
			 9,  7, 15, 11,  8,  6,  6, 14, 12, 13,  5, 14, 13, 13,  7,  5,
			15,  5,  8, 11, 14, 14,  6, 14,  6,  9, 12,  9, 12,  5, 15,  8
		};
		#endregion


		/// <summary>Initializes a new instance of the RIPEMD256 class.</summary>
		public RipeMD256() : base(64) {
			HashSizeValue = 256;
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				accumulator = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0x76543210, 0xFEDCBA98, 0x89ABCDEF, 0x01234567 };
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				uint[] workBuffer;
				uint A, B, C, D, Ap, Bp, Cp, Dp, T, i;
				int s;

				workBuffer = Conversions.ByteToUInt(inputBuffer, inputOffset, BlockSize);

				A = accumulator[0];
				B = accumulator[1];
				C = accumulator[2];
				D = accumulator[3];
				Ap = accumulator[4];
				Bp = accumulator[5];
				Cp = accumulator[6];
				Dp = accumulator[7];


				// Round 1
				for (i = 0; i < 16; i++) {
					s = S[i];
					T = A + (B ^ C ^ D) + workBuffer[i];
					A = D; D = C; C = B;
					B = BitTools.RotateLeft(T, s);

					s = Sp[i];
					T = Ap + ((Bp & Dp) | (Cp & ~Dp)) + workBuffer[Rp[i]] + 0x50A28BE6;
					Ap = Dp; Dp = Cp; Cp = Bp;
					Bp = BitTools.RotateLeft(T, s);
				}
				T = A; A = Ap; Ap = T;

				// Round 2
				for (i = 16; i < 32; i++) {
					s = S[i];
					T = A + ((B & C) | (~B & D)) + workBuffer[R[i]] + 0x5A827999;
					A = D; D = C; C = B;
					B = BitTools.RotateLeft(T, s);

					s = Sp[i];
					T = Ap + ((Bp | ~Cp) ^ Dp) + workBuffer[Rp[i]] + 0x5C4DD124;
					Ap = Dp; Dp = Cp; Cp = Bp;
					Bp = BitTools.RotateLeft(T, s);
				}
				T = B; B = Bp; Bp = T;

				// Round 3
				for (i = 32; i < 48; i++) {
					s = S[i];
					T = A + ((B | ~C) ^ D) + workBuffer[R[i]] + 0x6ED9EBA1;
					A = D; D = C; C = B;
					B = BitTools.RotateLeft(T, s);

					s = Sp[i];
					T = Ap + ((Bp & Cp) | (~Bp & Dp)) + workBuffer[Rp[i]] + 0x6D703EF3;
					Ap = Dp; Dp = Cp; Cp = Bp;
					Bp = BitTools.RotateLeft(T, s);
				}
				T = C; C = Cp; Cp = T;

				// Round 4
				for (i = 48; i < 64; i++) {
					s = S[i];
					T = A + ((B & D) | (C & ~D)) + workBuffer[R[i]] + 0x8F1BBCDC;
					A = D; D = C; C = B;
					B = BitTools.RotateLeft(T, s);

					s = Sp[i];
					T = Ap + (Bp ^ Cp ^ Dp) + workBuffer[Rp[i]];
					Ap = Dp; Dp = Cp; Cp = Bp;
					Bp = BitTools.RotateLeft(T, s);
				}
				T = D; D = Dp; Dp = T;

				accumulator[0] += A;
				accumulator[1] += B;
				accumulator[2] += C;
				accumulator[3] += D;
				accumulator[4] += Ap;
				accumulator[5] += Bp;
				accumulator[6] += Cp;
				accumulator[7] += Dp;
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
	}
}
