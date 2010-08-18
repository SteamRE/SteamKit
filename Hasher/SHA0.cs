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
	/// <summary>Computes the SHA0 hash for the input data using the managed library.</summary>
	public class Sha0 : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		private uint[] accumulator = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0xC3D2E1F0 };


		/// <summary>Initializes a new instance of the SHA0 class.</summary>
		public Sha0() : base(64) {
			HashSizeValue = 160;
		}


		/// <summary>Initializes an implementation of System.Security.Cryptography.HashAlgorithm.</summary>
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
				uint[] workBuffer = new uint[80];
				uint A = accumulator[0];
				uint B = accumulator[1];
				uint C = accumulator[2];
				uint D = accumulator[3];
				uint E = accumulator[4];

				uint[] temp = Conversions.ByteToUInt(inputBuffer, inputOffset, BlockSize, EndianType.BigEndian);
				Array.Copy(temp, 0, workBuffer, 0, temp.Length);
				for (int i = 16; i < 80; i++) {
					workBuffer[i] = workBuffer[i - 16] ^ workBuffer[i - 14] ^ workBuffer[i - 8] ^ workBuffer[i - 3];
				}

				#region Round 1
				E += BitTools.RotateLeft(A, 5) + F1(B, C, D) + workBuffer[0];  B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F1(A, B, C) + workBuffer[1];  A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F1(E, A, B) + workBuffer[2];  E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F1(D, E, A) + workBuffer[3];  D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F1(C, D, E) + workBuffer[4];  C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F1(B, C, D) + workBuffer[5];  B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F1(A, B, C) + workBuffer[6];  A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F1(E, A, B) + workBuffer[7];  E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F1(D, E, A) + workBuffer[8];  D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F1(C, D, E) + workBuffer[9];  C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F1(B, C, D) + workBuffer[10]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F1(A, B, C) + workBuffer[11]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F1(E, A, B) + workBuffer[12]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F1(D, E, A) + workBuffer[13]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F1(C, D, E) + workBuffer[14]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F1(B, C, D) + workBuffer[15]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F1(A, B, C) + workBuffer[16]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F1(E, A, B) + workBuffer[17]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F1(D, E, A) + workBuffer[18]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F1(C, D, E) + workBuffer[19]; C = BitTools.RotateLeft(C, 30);
				#endregion

				#region Round 2
				E += BitTools.RotateLeft(A, 5) + F2(B, C, D) + workBuffer[20]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F2(A, B, C) + workBuffer[21]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F2(E, A, B) + workBuffer[22]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F2(D, E, A) + workBuffer[23]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F2(C, D, E) + workBuffer[24]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F2(B, C, D) + workBuffer[25]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F2(A, B, C) + workBuffer[26]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F2(E, A, B) + workBuffer[27]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F2(D, E, A) + workBuffer[28]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F2(C, D, E) + workBuffer[29]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F2(B, C, D) + workBuffer[30]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F2(A, B, C) + workBuffer[31]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F2(E, A, B) + workBuffer[32]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F2(D, E, A) + workBuffer[33]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F2(C, D, E) + workBuffer[34]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F2(B, C, D) + workBuffer[35]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F2(A, B, C) + workBuffer[36]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F2(E, A, B) + workBuffer[37]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F2(D, E, A) + workBuffer[38]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F2(C, D, E) + workBuffer[39]; C = BitTools.RotateLeft(C, 30);
				#endregion

				#region Round 3
				E += BitTools.RotateLeft(A, 5) + F3(B, C, D) + workBuffer[40]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F3(A, B, C) + workBuffer[41]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F3(E, A, B) + workBuffer[42]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F3(D, E, A) + workBuffer[43]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F3(C, D, E) + workBuffer[44]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F3(B, C, D) + workBuffer[45]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F3(A, B, C) + workBuffer[46]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F3(E, A, B) + workBuffer[47]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F3(D, E, A) + workBuffer[48]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F3(C, D, E) + workBuffer[49]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F3(B, C, D) + workBuffer[50]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F3(A, B, C) + workBuffer[51]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F3(E, A, B) + workBuffer[52]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F3(D, E, A) + workBuffer[53]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F3(C, D, E) + workBuffer[54]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F3(B, C, D) + workBuffer[55]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F3(A, B, C) + workBuffer[56]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F3(E, A, B) + workBuffer[57]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F3(D, E, A) + workBuffer[58]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F3(C, D, E) + workBuffer[59]; C = BitTools.RotateLeft(C, 30);
				#endregion

				#region Round 4
				E += BitTools.RotateLeft(A, 5) + F4(B, C, D) + workBuffer[60]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F4(A, B, C) + workBuffer[61]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F4(E, A, B) + workBuffer[62]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F4(D, E, A) + workBuffer[63]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F4(C, D, E) + workBuffer[64]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F4(B, C, D) + workBuffer[65]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F4(A, B, C) + workBuffer[66]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F4(E, A, B) + workBuffer[67]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F4(D, E, A) + workBuffer[68]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F4(C, D, E) + workBuffer[69]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F4(B, C, D) + workBuffer[70]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F4(A, B, C) + workBuffer[71]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F4(E, A, B) + workBuffer[72]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F4(D, E, A) + workBuffer[73]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F4(C, D, E) + workBuffer[74]; C = BitTools.RotateLeft(C, 30);
				E += BitTools.RotateLeft(A, 5) + F4(B, C, D) + workBuffer[75]; B = BitTools.RotateLeft(B, 30);
				D += BitTools.RotateLeft(E, 5) + F4(A, B, C) + workBuffer[76]; A = BitTools.RotateLeft(A, 30);
				C += BitTools.RotateLeft(D, 5) + F4(E, A, B) + workBuffer[77]; E = BitTools.RotateLeft(E, 30);
				B += BitTools.RotateLeft(C, 5) + F4(D, E, A) + workBuffer[78]; D = BitTools.RotateLeft(D, 30);
				A += BitTools.RotateLeft(B, 5) + F4(C, D, E) + workBuffer[79]; C = BitTools.RotateLeft(C, 30);
				#endregion

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
				Array.Copy(Conversions.ULongToByte(size, EndianType.BigEndian), 0, temp, (inputCount + paddingSize), 8);

				// Push the final block(s) into the calculation.
				ProcessBlock(temp, 0);
				if (temp.Length == (BlockSize * 2)) {
					ProcessBlock(temp, BlockSize);
				}

				return Conversions.UIntToByte(accumulator, EndianType.BigEndian);
			}
		}


		static private uint F1(uint a, uint b, uint c) {
			return (c ^ (a & (b ^ c))) + 0x5A827999;
		}

		static private uint F2(uint a, uint b, uint c) {
			return (a ^ b ^ c) + 0x6ED9EBA1;
		}

		static private uint F3(uint a, uint b, uint c) {
			return ((a & b) | (c & (a | b))) + 0x8F1BBCDC;
		}

		static private uint F4(uint a, uint b, uint c) {
			return (a ^ b ^ c) + 0xCA62C1D6;
		}
	}
}
