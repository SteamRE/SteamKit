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
	/// <summary>Computes the SHA384 hash for the input data using the managed library.</summary>
	public class Sha384 : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		private ulong[] accumulator = new ulong[] { 0xCBBB9D5DC1059ED8, 0x629A292A367CD507, 0x9159015A3070DD17, 0x152FECD8F70E5939, 0x67332667FFC00B31, 0x8EB44A8768581511, 0xDB0C2E0D64F98FA7, 0x47B5481DBEFA4FA4 };


		#region Table
		static private ulong[] K = new ulong[] {
			0x428A2F98D728AE22, 0x7137449123EF65CD, 0xB5C0FBCFEC4D3B2F,
			0xE9B5DBA58189DBBC, 0x3956C25BF348B538, 0x59F111F1B605D019,
			0x923F82A4AF194F9B, 0xAB1C5ED5DA6D8118, 0xD807AA98A3030242,
			0x12835B0145706FBE, 0x243185BE4EE4B28C, 0x550C7DC3D5FFB4E2,
			0x72BE5D74F27B896F, 0x80DEB1FE3B1696B1, 0x9BDC06A725C71235,
			0xC19BF174CF692694, 0xE49B69C19EF14AD2, 0xEFBE4786384F25E3,
			0x0FC19DC68B8CD5B5, 0x240CA1CC77AC9C65, 0x2DE92C6F592B0275,
			0x4A7484AA6EA6E483, 0x5CB0A9DCBD41FBD4, 0x76F988DA831153B5,
			0x983E5152EE66DFAB, 0xA831C66D2DB43210, 0xB00327C898FB213F,
			0xBF597FC7BEEF0EE4, 0xC6E00BF33DA88FC2, 0xD5A79147930AA725,
			0x06CA6351E003826F, 0x142929670A0E6E70, 0x27B70A8546D22FFC,
			0x2E1B21385C26C926, 0x4D2C6DFC5AC42AED, 0x53380D139D95B3DF,
			0x650A73548BAF63DE, 0x766A0ABB3C77B2A8, 0x81C2C92E47EDAEE6,
			0x92722C851482353B, 0xA2BFE8A14CF10364, 0xA81A664BBC423001,
			0xC24B8B70D0F89791, 0xC76C51A30654BE30, 0xD192E819D6EF5218,
			0xD69906245565A910, 0xF40E35855771202A, 0x106AA07032BBD1B8,
			0x19A4C116B8D2D0C8, 0x1E376C085141AB53, 0x2748774CDF8EEB99,
			0x34B0BCB5E19B48A8, 0x391C0CB3C5C95A63, 0x4ED8AA4AE3418ACB,
			0x5B9CCA4F7763E373, 0x682E6FF3D6B2B8A3, 0x748F82EE5DEFB2FC,
			0x78A5636F43172F60, 0x84C87814A1F0AB72, 0x8CC702081A6439EC,
			0x90BEFFFA23631E28, 0xA4506CEBDE82BDE9, 0xBEF9A3F7B2C67915,
			0xC67178F2E372532B, 0xCA273ECEEA26619C, 0xD186B8C721C0C207,
			0xEADA7DD6CDE0EB1E, 0xF57D4F7FEE6ED178, 0x06F067AA72176FBA,
			0x0A637DC5A2C898A6, 0x113F9804BEF90DAE, 0x1B710B35131C471B,
			0x28DB77F523047D84, 0x32CAAB7B40C72493, 0x3C9EBE0A15C9BEBC,
			0x431D67C49C100D4C, 0x4CC5D4BECB3E42B6, 0x597F299CFC657E2A,
			0x5FCB6FAB3AD6FAEC, 0x6C44198C4A475817
		};
		#endregion


		/// <summary>Initializes a new instance of the SHA384 class.</summary>
		public Sha384() : base(128) {
			HashSizeValue = 384;
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				accumulator = new ulong[] { 0xCBBB9D5DC1059ED8, 0x629A292A367CD507, 0x9159015A3070DD17, 0x152FECD8F70E5939, 0x67332667FFC00B31, 0x8EB44A8768581511, 0xDB0C2E0D64F98FA7, 0x47B5481DBEFA4FA4 };
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				ulong[] workBuffer = new ulong[80];
				ulong a = accumulator[0];
				ulong b = accumulator[1];
				ulong c = accumulator[2];
				ulong d = accumulator[3];
				ulong e = accumulator[4];
				ulong f = accumulator[5];
				ulong g = accumulator[6];
				ulong h = accumulator[7];
				ulong T1, T2;


				ulong[] temp = Conversions.ByteToULong(inputBuffer, inputOffset, BlockSize, EndianType.BigEndian);
				Array.Copy(temp, 0, workBuffer, 0, temp.Length);
				for (int i = 16; i < 80; i++) {
					workBuffer[i] = Sig1R(workBuffer[i - 2]) + workBuffer[i - 7] + Sig0R(workBuffer[i - 15]) + workBuffer[i - 16];
				}

				for (int i = 0; i < 80; i++) {
					T1 = h + Sig1(e) + Ch(e, f, g) + K[i] + workBuffer[i];
					T2 = Sig0(a) + Maj(a, b, c);

					h = g; g = f; f = e;
					e = d + T1;
					d = c; c = b; b = a;
					a = T1 + T2;
				}

				accumulator[0] += a;
				accumulator[1] += b;
				accumulator[2] += c;
				accumulator[3] += d;
				accumulator[4] += e;
				accumulator[5] += f;
				accumulator[6] += g;
				accumulator[7] += h;
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
				paddingSize = (BlockSize - 16) - paddingSize;
				if (paddingSize < 1) { paddingSize += BlockSize; }

				// Create the final, padded block(s).
				temp = new byte[inputCount + paddingSize + 16];
				Array.Copy(inputBuffer, inputOffset, temp, 0, inputCount);
				temp[inputCount] = 0x80;
				size = ((ulong)Count + (ulong)inputCount) * 8;
				Array.Copy(Conversions.ULongToByte(size, EndianType.BigEndian), 0, temp, (temp.Length - 8), 8);

				// Push the final block(s) into the calculation.
				ProcessBlock(temp, 0);
				if (temp.Length == (BlockSize * 2)) {
					ProcessBlock(temp, BlockSize);
				}

				return Conversions.ULongToByte(accumulator, 0, (accumulator.Length - 2), EndianType.BigEndian);
			}
		}


		static private ulong Ch(ulong x, ulong y, ulong z) {
			return (x & y) ^ (~x & z);
		}

		static private ulong Maj(ulong x, ulong y, ulong z) {
			return (x & y) ^ (x & z) ^ (y & z);
		}

		static private ulong Sig0(ulong x) {
			return BitTools.RotateRight(x, 28) ^ BitTools.RotateRight(x, 34) ^ BitTools.RotateRight(x, 39);
		}

		static private ulong Sig1(ulong x) {
			return BitTools.RotateRight(x, 14) ^ BitTools.RotateRight(x, 18) ^ BitTools.RotateRight(x, 41);
		}

		static private ulong Sig0R(ulong x) {
			return BitTools.RotateRight(x, 1) ^ BitTools.RotateRight(x, 8) ^ R(7, x);
		}

		static private ulong Sig1R(ulong x) {
			return BitTools.RotateRight(x, 19) ^ BitTools.RotateRight(x, 61) ^ R(6, x);
		}

		static private ulong R(int offset, ulong x) {
			return (x >> offset);
		}
	}
}
