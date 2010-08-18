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
	/// <summary>Computes the Whirlpool hash for the input data using the managed library.</summary>
	public class Whirlpool : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		static private ulong[,] T;
		static private ulong[] rc;
		private ulong[] accumulator = new ulong[8];


		/// <summary>Initializes a new instance of the Whirlpool class.</summary>
		public Whirlpool() : base(64) {
			HashSizeValue = 512;
		}

		/// <summary>Initialize the substitution boxes.</summary>
		static Whirlpool() {
			ulong s1, s2, /*s3,*/ s4, s5, s8, s9, t;
			byte[] S = new byte[256];
			ushort c;

			ushort[] Sd = new ushort[] {
				0x1823, 0xC6E8, 0x87B8, 0x014F, 0x36A6, 0xD2F5, 0x796F, 0x9152,
				0x60BC, 0x9B8E, 0xA30C, 0x7B35, 0x1DE0, 0xD7C2, 0x2E4B, 0xFE57,
				0x1577, 0x37E5, 0x9FF0, 0x4ADA, 0x58C9, 0x290A, 0xB1A0, 0x6B85,
				0xBD5D, 0x10F4, 0xCB3E, 0x0567, 0xE427, 0x418B, 0xA77D, 0x95D8,
				0xFBEE, 0x7C66, 0xDD17, 0x479E, 0xCA2D, 0xBF07, 0xAD5A, 0x8333,
				0x6302, 0xAA71, 0xC819, 0x49D9, 0xF2E3, 0x5B88, 0x9A26, 0x32B0,
				0xE90F, 0xD580, 0xBECD, 0x3448, 0xFF7A, 0x905F, 0x2068, 0x1AAE,
				0xB454, 0x9322, 0x64F1, 0x7312, 0x4008, 0xC3EC, 0xDBA1, 0x8D3D,
				0x9700, 0xCF2B, 0x7682, 0xD61B, 0xB5AF, 0x6A50, 0x45F3, 0x30EF,
				0x3F55, 0xA2EA, 0x65BA, 0x2FC0, 0xDE1C, 0xFD4D, 0x9275, 0x068A,
				0xB2E6, 0x0E1F, 0x62D4, 0xA896, 0xF9C5, 0x2559, 0x8472, 0x394C,
				0x5E78, 0x388C, 0xD1A5, 0xE261, 0xB321, 0x9C1E, 0x43C7, 0xFC04,
				0x5199, 0x6D0D, 0xFADF, 0x7E24, 0x3BAB, 0xCE11, 0x8F4E, 0xB7EB,
				0x3C81, 0x94F7, 0xB913, 0x2CD3, 0xE76E, 0xC403, 0x5644, 0x7FA9,
				0x2ABB, 0xC153, 0xDC0B, 0x9D6C, 0x3174, 0xF646, 0xAC89, 0x14E1,
				0x163A, 0x6909, 0x70B6, 0xD0ED, 0xCC42, 0x98A4, 0x285C, 0xF886
			};

			T = new ulong[8,256];
			rc = new ulong[10];

			for (int i = 0; i < 256; i++) {
				c = Sd[i >> 1];

				s1 = (ulong)(((i & 1) == 0 ? c >> 8 : c) & 0xFF);
				s2 = s1 << 1;
				if (s2 > 0xFF) { s2 ^= 0x011D; }

				//s3 = s2 ^ s1;
				s4 = s2 << 1;
				if (s4 > 0xFF) { s4 ^= 0x011D; }

				s5 = s4 ^ s1;
				s8 = s4 << 1;
				if (s8 > 0xFF) { s8 ^= 0x011D; }
				s9 = s8 ^ s1;

				S[i] = (byte) s1;
				t = (s1 << 56) | (s1 << 48) | (s4 << 40) | (s1 << 32) | (s8 << 24) | (s5 << 16) | (s2 << 8) | (s9);
				T[0,i] = t;
				T[1,i] = (t >>  8) | (t << 56);
				T[2,i] = (t >> 16) | (t << 48);
				T[3,i] = (t >> 24) | (t << 40);
				T[4,i] = (t >> 32) | (t << 32);
				T[5,i] = (t >> 40) | (t << 24);
				T[6,i] = (t >> 48) | (t << 16);
				T[7,i] = (t >> 56) | (t <<  8);
			}

			rc = Conversions.ByteToULong(S, EndianType.BigEndian);
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				accumulator = new ulong[8];
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				ulong[] workBuffer = new ulong[8];
				ulong[] k = new ulong[8];
				ulong[] nn = new ulong[8];
				ulong[] w = new ulong[8];
				ulong[] kr = new ulong[8];
				int i;

				workBuffer = Conversions.ByteToULong(inputBuffer, inputOffset, BlockSize, EndianType.BigEndian);

				Array.Copy(accumulator, 0, k, 0, 8);
				for (i = 0; i < 8; i++) {
					nn[i] = workBuffer[i] ^ k[i];
				}

				for (int r = 0; r < 10; r++) {
					kr[0] = T[0,(int)((k[0] >> 56) & 0xFF)] ^ T[1,(int)((k[7] >> 48) & 0xFF)] ^
							T[2,(int)((k[6] >> 40) & 0xFF)] ^ T[3,(int)((k[5] >> 32) & 0xFF)] ^
							T[4,(int)((k[4] >> 24) & 0xFF)] ^ T[5,(int)((k[3] >> 16) & 0xFF)] ^
							T[6,(int)((k[2] >>  8) & 0xFF)] ^ T[7,(int)((k[1]      ) & 0xFF)] ^
							rc[r];
					kr[1] = T[0,(int)((k[1] >> 56) & 0xFF)] ^ T[1,(int)((k[0] >> 48) & 0xFF)] ^
							T[2,(int)((k[7] >> 40) & 0xFF)] ^ T[3,(int)((k[6] >> 32) & 0xFF)] ^
							T[4,(int)((k[5] >> 24) & 0xFF)] ^ T[5,(int)((k[4] >> 16) & 0xFF)] ^
							T[6,(int)((k[3] >>  8) & 0xFF)] ^ T[7,(int)((k[2]      ) & 0xFF)];
					kr[2] = T[0,(int)((k[2] >> 56) & 0xFF)] ^ T[1,(int)((k[1] >> 48) & 0xFF)] ^
							T[2,(int)((k[0] >> 40) & 0xFF)] ^ T[3,(int)((k[7] >> 32) & 0xFF)] ^
							T[4,(int)((k[6] >> 24) & 0xFF)] ^ T[5,(int)((k[5] >> 16) & 0xFF)] ^
							T[6,(int)((k[4] >>  8) & 0xFF)] ^ T[7,(int)((k[3]      ) & 0xFF)];
					kr[3] = T[0,(int)((k[3] >> 56) & 0xFF)] ^ T[1,(int)((k[2] >> 48) & 0xFF)] ^
							T[2,(int)((k[1] >> 40) & 0xFF)] ^ T[3,(int)((k[0] >> 32) & 0xFF)] ^
							T[4,(int)((k[7] >> 24) & 0xFF)] ^ T[5,(int)((k[6] >> 16) & 0xFF)] ^
							T[6,(int)((k[5] >>  8) & 0xFF)] ^ T[7,(int)((k[4]      ) & 0xFF)];
					kr[4] = T[0,(int)((k[4] >> 56) & 0xFF)] ^ T[1,(int)((k[3] >> 48) & 0xFF)] ^
							T[2,(int)((k[2] >> 40) & 0xFF)] ^ T[3,(int)((k[1] >> 32) & 0xFF)] ^
							T[4,(int)((k[0] >> 24) & 0xFF)] ^ T[5,(int)((k[7] >> 16) & 0xFF)] ^
							T[6,(int)((k[6] >>  8) & 0xFF)] ^ T[7,(int)((k[5]      ) & 0xFF)];
					kr[5] = T[0,(int)((k[5] >> 56) & 0xFF)] ^ T[1,(int)((k[4] >> 48) & 0xFF)] ^
							T[2,(int)((k[3] >> 40) & 0xFF)] ^ T[3,(int)((k[2] >> 32) & 0xFF)] ^
							T[4,(int)((k[1] >> 24) & 0xFF)] ^ T[5,(int)((k[0] >> 16) & 0xFF)] ^
							T[6,(int)((k[7] >>  8) & 0xFF)] ^ T[7,(int)((k[6]      ) & 0xFF)];
					kr[6] = T[0,(int)((k[6] >> 56) & 0xFF)] ^ T[1,(int)((k[5] >> 48) & 0xFF)] ^
							T[2,(int)((k[4] >> 40) & 0xFF)] ^ T[3,(int)((k[3] >> 32) & 0xFF)] ^
							T[4,(int)((k[2] >> 24) & 0xFF)] ^ T[5,(int)((k[1] >> 16) & 0xFF)] ^
							T[6,(int)((k[0] >>  8) & 0xFF)] ^ T[7,(int)((k[7]      ) & 0xFF)];
					kr[7] = T[0,(int)((k[7] >> 56) & 0xFF)] ^ T[1,(int)((k[6] >> 48) & 0xFF)] ^
							T[2,(int)((k[5] >> 40) & 0xFF)] ^ T[3,(int)((k[4] >> 32) & 0xFF)] ^
							T[4,(int)((k[3] >> 24) & 0xFF)] ^ T[5,(int)((k[2] >> 16) & 0xFF)] ^
							T[6,(int)((k[1] >>  8) & 0xFF)] ^ T[7,(int)((k[0]      ) & 0xFF)];

					Array.Copy(kr, 0, k, 0, 8);

					w[0] =	T[0,(int)((nn[0] >> 56) & 0xFF)] ^ T[1,(int)((nn[7] >> 48) & 0xFF)] ^
							T[2,(int)((nn[6] >> 40) & 0xFF)] ^ T[3,(int)((nn[5] >> 32) & 0xFF)] ^
							T[4,(int)((nn[4] >> 24) & 0xFF)] ^ T[5,(int)((nn[3] >> 16) & 0xFF)] ^
							T[6,(int)((nn[2] >>  8) & 0xFF)] ^ T[7,(int)((nn[1]      ) & 0xFF)] ^
							kr[0];
					w[1] =	T[0,(int)((nn[1] >> 56) & 0xFF)] ^ T[1,(int)((nn[0] >> 48) & 0xFF)] ^
							T[2,(int)((nn[7] >> 40) & 0xFF)] ^ T[3,(int)((nn[6] >> 32) & 0xFF)] ^
							T[4,(int)((nn[5] >> 24) & 0xFF)] ^ T[5,(int)((nn[4] >> 16) & 0xFF)] ^
							T[6,(int)((nn[3] >>  8) & 0xFF)] ^ T[7,(int)((nn[2]      ) & 0xFF)] ^
							kr[1];
					w[2] =	T[0,(int)((nn[2] >> 56) & 0xFF)] ^ T[1,(int)((nn[1] >> 48) & 0xFF)] ^
							T[2,(int)((nn[0] >> 40) & 0xFF)] ^ T[3,(int)((nn[7] >> 32) & 0xFF)] ^
							T[4,(int)((nn[6] >> 24) & 0xFF)] ^ T[5,(int)((nn[5] >> 16) & 0xFF)] ^
							T[6,(int)((nn[4] >>  8) & 0xFF)] ^ T[7,(int)((nn[3]      ) & 0xFF)] ^
							kr[2];
					w[3] =	T[0,(int)((nn[3] >> 56) & 0xFF)] ^ T[1,(int)((nn[2] >> 48) & 0xFF)] ^
							T[2,(int)((nn[1] >> 40) & 0xFF)] ^ T[3,(int)((nn[0] >> 32) & 0xFF)] ^
							T[4,(int)((nn[7] >> 24) & 0xFF)] ^ T[5,(int)((nn[6] >> 16) & 0xFF)] ^
							T[6,(int)((nn[5] >>  8) & 0xFF)] ^ T[7,(int)((nn[4]      ) & 0xFF)] ^
							kr[3];
					w[4] =	T[0,(int)((nn[4] >> 56) & 0xFF)] ^ T[1,(int)((nn[3] >> 48) & 0xFF)] ^
							T[2,(int)((nn[2] >> 40) & 0xFF)] ^ T[3,(int)((nn[1] >> 32) & 0xFF)] ^
							T[4,(int)((nn[0] >> 24) & 0xFF)] ^ T[5,(int)((nn[7] >> 16) & 0xFF)] ^
							T[6,(int)((nn[6] >>  8) & 0xFF)] ^ T[7,(int)((nn[5]      ) & 0xFF)] ^
							kr[4];
					w[5] =	T[0,(int)((nn[5] >> 56) & 0xFF)] ^ T[1,(int)((nn[4] >> 48) & 0xFF)] ^
							T[2,(int)((nn[3] >> 40) & 0xFF)] ^ T[3,(int)((nn[2] >> 32) & 0xFF)] ^
							T[4,(int)((nn[1] >> 24) & 0xFF)] ^ T[5,(int)((nn[0] >> 16) & 0xFF)] ^
							T[6,(int)((nn[7] >>  8) & 0xFF)] ^ T[7,(int)((nn[6]      ) & 0xFF)] ^
							kr[5];
					w[6] =	T[0,(int)((nn[6] >> 56) & 0xFF)] ^ T[1,(int)((nn[5] >> 48) & 0xFF)] ^
							T[2,(int)((nn[4] >> 40) & 0xFF)] ^ T[3,(int)((nn[3] >> 32) & 0xFF)] ^
							T[4,(int)((nn[2] >> 24) & 0xFF)] ^ T[5,(int)((nn[1] >> 16) & 0xFF)] ^
							T[6,(int)((nn[0] >>  8) & 0xFF)] ^ T[7,(int)((nn[7]      ) & 0xFF)] ^
							kr[6];
					w[7] =	T[0,(int)((nn[7] >> 56) & 0xFF)] ^ T[1,(int)((nn[6] >> 48) & 0xFF)] ^
							T[2,(int)((nn[5] >> 40) & 0xFF)] ^ T[3,(int)((nn[4] >> 32) & 0xFF)] ^
							T[4,(int)((nn[3] >> 24) & 0xFF)] ^ T[5,(int)((nn[2] >> 16) & 0xFF)] ^
							T[6,(int)((nn[1] >>  8) & 0xFF)] ^ T[7,(int)((nn[0]      ) & 0xFF)] ^
							kr[7];

					Array.Copy(w, 0, nn, 0, 8);
				}

				for (i = 0; i < 8; i++) {
					accumulator[i] ^= w[i] ^ workBuffer[i];
				}
			}
		}


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		override protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			lock (syncLock) {
				// Figure out how much padding is needed.
				int n = (int)((Count + inputCount + 33) % BlockSize);
				int paddingLength = (n == 0) ? 33 : (BlockSize - n + 33);
				byte[] padding = new byte[paddingLength];

				// Assemble the padding.
				padding[0] = (byte)0x80;
				ulong bits = (ulong)(Count + inputCount) << 3;
				paddingLength -= 8;
				Array.Copy(Conversions.ULongToByte(bits, EndianType.BigEndian), 0, padding, paddingLength, 8);

				// Attach the padding to the last bytes we have to process.
				byte[] temp = new byte[inputCount + padding.Length];
				Array.Copy(inputBuffer, inputOffset, temp, 0, inputCount);
				Array.Copy(padding, 0, temp, inputCount, padding.Length);

				// Push the final block(s) into the calculation.
				ProcessBlock(temp, 0);
				if (temp.Length == (BlockSize * 2)) {
					ProcessBlock(temp, BlockSize);
				}

				return Conversions.ULongToByte(accumulator, EndianType.BigEndian);
			}
		}
	}
}
