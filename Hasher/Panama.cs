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
	/// <summary>Computes the Panama hash for the input data using the managed library.</summary>
	public class Panama : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		private uint[] accumulator = new uint[17];
		private uint[,] stages = new uint[32, 8];
		private int tap;


		/// <summary>Initializes a new instance of the Panama class.</summary>
		public Panama() : base(32) {
			HashSizeValue = 256;
		}


		/// <summary>Initializes the algorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				accumulator = new uint[17];
				stages = new uint[32,8];
				tap = 0;
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				uint[] gamma = new uint[17];
				uint[] pi = new uint[17];
				uint[] theta = new uint[17];
				uint[] state = new uint[17];
				uint[] workBuffer;
				int tap16, tap25;

				Array.Copy(accumulator, 0, state, 0, 17);
				workBuffer = Conversions.ByteToUInt(inputBuffer, inputOffset, BlockSize);

				// Gamma
				gamma[ 0] = state[ 0] ^ (state[ 1] | ~state[ 2]);
				gamma[ 1] = state[ 1] ^ (state[ 2] | ~state[ 3]);
				gamma[ 2] = state[ 2] ^ (state[ 3] | ~state[ 4]);
				gamma[ 3] = state[ 3] ^ (state[ 4] | ~state[ 5]);
				gamma[ 4] = state[ 4] ^ (state[ 5] | ~state[ 6]);
				gamma[ 5] = state[ 5] ^ (state[ 6] | ~state[ 7]);
				gamma[ 6] = state[ 6] ^ (state[ 7] | ~state[ 8]);
				gamma[ 7] = state[ 7] ^ (state[ 8] | ~state[ 9]);
				gamma[ 8] = state[ 8] ^ (state[ 9] | ~state[10]);
				gamma[ 9] = state[ 9] ^ (state[10] | ~state[11]);
				gamma[10] = state[10] ^ (state[11] | ~state[12]);
				gamma[11] = state[11] ^ (state[12] | ~state[13]);
				gamma[12] = state[12] ^ (state[13] | ~state[14]);
				gamma[13] = state[13] ^ (state[14] | ~state[15]);
				gamma[14] = state[14] ^ (state[15] | ~state[16]);
				gamma[15] = state[15] ^ (state[16] | ~state[ 0]);
				gamma[16] = state[16] ^ (state[ 0] | ~state[ 1]);

				// Pi
				pi[ 0] = gamma[0];
				pi[ 1] = BitTools.RotateLeft(gamma[ 7],  1);
				pi[ 2] = BitTools.RotateLeft(gamma[14],  3);
				pi[ 3] = BitTools.RotateLeft(gamma[ 4],  6);
				pi[ 4] = BitTools.RotateLeft(gamma[11], 10);
				pi[ 5] = BitTools.RotateLeft(gamma[ 1], 15);
				pi[ 6] = BitTools.RotateLeft(gamma[ 8], 21);
				pi[ 7] = BitTools.RotateLeft(gamma[15], 28);
				pi[ 8] = BitTools.RotateLeft(gamma[ 5],  4);
				pi[ 9] = BitTools.RotateLeft(gamma[12], 13);
				pi[10] = BitTools.RotateLeft(gamma[ 2], 23);
				pi[11] = BitTools.RotateLeft(gamma[ 9],  2);
				pi[12] = BitTools.RotateLeft(gamma[16], 14);
				pi[13] = BitTools.RotateLeft(gamma[ 6], 27);
				pi[14] = BitTools.RotateLeft(gamma[13],  9);
				pi[15] = BitTools.RotateLeft(gamma[ 3], 24);
				pi[16] = BitTools.RotateLeft(gamma[10],  8);

				// Theta
				theta[ 0] = pi[ 0] ^ pi[ 1] ^ pi[ 4];
				theta[ 1] = pi[ 1] ^ pi[ 2] ^ pi[ 5];
				theta[ 2] = pi[ 2] ^ pi[ 3] ^ pi[ 6];
				theta[ 3] = pi[ 3] ^ pi[ 4] ^ pi[ 7];
				theta[ 4] = pi[ 4] ^ pi[ 5] ^ pi[ 8];
				theta[ 5] = pi[ 5] ^ pi[ 6] ^ pi[ 9];
				theta[ 6] = pi[ 6] ^ pi[ 7] ^ pi[10];
				theta[ 7] = pi[ 7] ^ pi[ 8] ^ pi[11];
				theta[ 8] = pi[ 8] ^ pi[ 9] ^ pi[12];
				theta[ 9] = pi[ 9] ^ pi[10] ^ pi[13];
				theta[10] = pi[10] ^ pi[11] ^ pi[14];
				theta[11] = pi[11] ^ pi[12] ^ pi[15];
				theta[12] = pi[12] ^ pi[13] ^ pi[16];
				theta[13] = pi[13] ^ pi[14] ^ pi[ 0];
				theta[14] = pi[14] ^ pi[15] ^ pi[ 1];
				theta[15] = pi[15] ^ pi[16] ^ pi[ 2];
				theta[16] = pi[16] ^ pi[ 0] ^ pi[ 3];

				// Shift the tap points.
				int mod = 0x1F;
				tap16 = (tap + 16) & mod;
				tap = (tap - 1) & mod;
				tap25 = (tap + 25) & mod;

				// Lambda
				stages[tap25,0] ^= stages[tap,2];
				stages[tap25,1] ^= stages[tap,3];
				stages[tap25,2] ^= stages[tap,4];
				stages[tap25,3] ^= stages[tap,5];
				stages[tap25,4] ^= stages[tap,6];
				stages[tap25,5] ^= stages[tap,7];
				stages[tap25,6] ^= stages[tap,0];
				stages[tap25,7] ^= stages[tap,1];
				stages[tap,0] ^= workBuffer[0];
				stages[tap,1] ^= workBuffer[1];
				stages[tap,2] ^= workBuffer[2];
				stages[tap,3] ^= workBuffer[3];
				stages[tap,4] ^= workBuffer[4];
				stages[tap,5] ^= workBuffer[5];
				stages[tap,6] ^= workBuffer[6];
				stages[tap,7] ^= workBuffer[7];

				// Sigma
				state[ 0] = theta[ 0] ^ 0x01;
				state[ 1] = theta[ 1] ^ workBuffer[0];
				state[ 2] = theta[ 2] ^ workBuffer[1];
				state[ 3] = theta[ 3] ^ workBuffer[2];
				state[ 4] = theta[ 4] ^ workBuffer[3];
				state[ 5] = theta[ 5] ^ workBuffer[4];
				state[ 6] = theta[ 6] ^ workBuffer[5];
				state[ 7] = theta[ 7] ^ workBuffer[6];
				state[ 8] = theta[ 8] ^ workBuffer[7];
				state[ 9] = theta[ 9] ^ stages[tap16,0];
				state[10] = theta[10] ^ stages[tap16,1];
				state[11] = theta[11] ^ stages[tap16,2];
				state[12] = theta[12] ^ stages[tap16,3];
				state[13] = theta[13] ^ stages[tap16,4];
				state[14] = theta[14] ^ stages[tap16,5];
				state[15] = theta[15] ^ stages[tap16,6];
				state[16] = theta[16] ^ stages[tap16,7];

				Array.Copy(state, 0, accumulator, 0, 17);
			}
		}


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The final block of data to process.</param>
		/// <param name="inputOffset">Where to start in the array.</param>
		/// <param name="inputCount">How many bytes should be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		override protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			lock (syncLock) {
				byte[] temp;

				// Pad and push the final block into the calculation.
				temp = new byte[BlockSize];
				Array.Copy(inputBuffer, inputOffset, temp, 0, inputCount);
				temp[inputCount] = 0x01;
				ProcessBlock(temp, 0);

				return Pull();
			}
		}


		private byte[] Pull() {
			uint[] gamma = new uint[17];
			uint[] pi = new uint[17];
			uint[] theta = new uint[17];
			uint[] state = new uint[17];
			uint[] hash = new uint[8];
			int tap4, tap16, tap25;
			int mod = 0x1F;

			Array.Copy(accumulator, 0, state, 0, 17);

			for (int i = 0; i < 33; i++) {
				Array.Copy(state, 9, hash, 0, 8);

				// Gamma
				gamma[ 0] = state[ 0] ^ (state[ 1] | ~state[ 2]);
				gamma[ 1] = state[ 1] ^ (state[ 2] | ~state[ 3]);
				gamma[ 2] = state[ 2] ^ (state[ 3] | ~state[ 4]);
				gamma[ 3] = state[ 3] ^ (state[ 4] | ~state[ 5]);
				gamma[ 4] = state[ 4] ^ (state[ 5] | ~state[ 6]);
				gamma[ 5] = state[ 5] ^ (state[ 6] | ~state[ 7]);
				gamma[ 6] = state[ 6] ^ (state[ 7] | ~state[ 8]);
				gamma[ 7] = state[ 7] ^ (state[ 8] | ~state[ 9]);
				gamma[ 8] = state[ 8] ^ (state[ 9] | ~state[10]);
				gamma[ 9] = state[ 9] ^ (state[10] | ~state[11]);
				gamma[10] = state[10] ^ (state[11] | ~state[12]);
				gamma[11] = state[11] ^ (state[12] | ~state[13]);
				gamma[12] = state[12] ^ (state[13] | ~state[14]);
				gamma[13] = state[13] ^ (state[14] | ~state[15]);
				gamma[14] = state[14] ^ (state[15] | ~state[16]);
				gamma[15] = state[15] ^ (state[16] | ~state[ 0]);
				gamma[16] = state[16] ^ (state[ 0] | ~state[ 1]);

				// Pi
				pi[ 0] = gamma[0];
				pi[ 1] = BitTools.RotateLeft(gamma[ 7],  1);
				pi[ 2] = BitTools.RotateLeft(gamma[14],  3);
				pi[ 3] = BitTools.RotateLeft(gamma[ 4],  6);
				pi[ 4] = BitTools.RotateLeft(gamma[11], 10);
				pi[ 5] = BitTools.RotateLeft(gamma[ 1], 15);
				pi[ 6] = BitTools.RotateLeft(gamma[ 8], 21);
				pi[ 7] = BitTools.RotateLeft(gamma[15], 28);
				pi[ 8] = BitTools.RotateLeft(gamma[ 5],  4);
				pi[ 9] = BitTools.RotateLeft(gamma[12], 13);
				pi[10] = BitTools.RotateLeft(gamma[ 2], 23);
				pi[11] = BitTools.RotateLeft(gamma[ 9],  2);
				pi[12] = BitTools.RotateLeft(gamma[16], 14);
				pi[13] = BitTools.RotateLeft(gamma[ 6], 27);
				pi[14] = BitTools.RotateLeft(gamma[13],  9);
				pi[15] = BitTools.RotateLeft(gamma[ 3], 24);
				pi[16] = BitTools.RotateLeft(gamma[10],  8);

				// Theta
				theta[ 0] = pi[ 0] ^ pi[ 1] ^ pi[ 4];
				theta[ 1] = pi[ 1] ^ pi[ 2] ^ pi[ 5];
				theta[ 2] = pi[ 2] ^ pi[ 3] ^ pi[ 6];
				theta[ 3] = pi[ 3] ^ pi[ 4] ^ pi[ 7];
				theta[ 4] = pi[ 4] ^ pi[ 5] ^ pi[ 8];
				theta[ 5] = pi[ 5] ^ pi[ 6] ^ pi[ 9];
				theta[ 6] = pi[ 6] ^ pi[ 7] ^ pi[10];
				theta[ 7] = pi[ 7] ^ pi[ 8] ^ pi[11];
				theta[ 8] = pi[ 8] ^ pi[ 9] ^ pi[12];
				theta[ 9] = pi[ 9] ^ pi[10] ^ pi[13];
				theta[10] = pi[10] ^ pi[11] ^ pi[14];
				theta[11] = pi[11] ^ pi[12] ^ pi[15];
				theta[12] = pi[12] ^ pi[13] ^ pi[16];
				theta[13] = pi[13] ^ pi[14] ^ pi[ 0];
				theta[14] = pi[14] ^ pi[15] ^ pi[ 1];
				theta[15] = pi[15] ^ pi[16] ^ pi[ 2];
				theta[16] = pi[16] ^ pi[ 0] ^ pi[ 3];

				// Shift the tap points.
				tap4 = (tap + 4) & mod;
				tap16 = (tap + 16) & mod;
				tap = (tap - 1) & mod;
				tap25 = (tap + 25) & mod;

				// Lambda
				stages[tap25,0] ^= stages[tap,2];
				stages[tap25,1] ^= stages[tap,3];
				stages[tap25,2] ^= stages[tap,4];
				stages[tap25,3] ^= stages[tap,5];
				stages[tap25,4] ^= stages[tap,6];
				stages[tap25,5] ^= stages[tap,7];
				stages[tap25,6] ^= stages[tap,0];
				stages[tap25,7] ^= stages[tap,1];
				stages[tap,0] ^= state[1];
				stages[tap,1] ^= state[2];
				stages[tap,2] ^= state[3];
				stages[tap,3] ^= state[4];
				stages[tap,4] ^= state[5];
				stages[tap,5] ^= state[6];
				stages[tap,6] ^= state[7];
				stages[tap,7] ^= state[8];

				// Sigma
				state[ 0] = theta[ 0] ^ 0x01;
				state[ 1] = theta[ 1] ^ stages[tap4,0];
				state[ 2] = theta[ 2] ^ stages[tap4,1];
				state[ 3] = theta[ 3] ^ stages[tap4,2];
				state[ 4] = theta[ 4] ^ stages[tap4,3];
				state[ 5] = theta[ 5] ^ stages[tap4,4];
				state[ 6] = theta[ 6] ^ stages[tap4,5];
				state[ 7] = theta[ 7] ^ stages[tap4,6];
				state[ 8] = theta[ 8] ^ stages[tap4,7];
				state[ 9] = theta[ 9] ^ stages[tap16,0];
				state[10] = theta[10] ^ stages[tap16,1];
				state[11] = theta[11] ^ stages[tap16,2];
				state[12] = theta[12] ^ stages[tap16,3];
				state[13] = theta[13] ^ stages[tap16,4];
				state[14] = theta[14] ^ stages[tap16,5];
				state[15] = theta[15] ^ stages[tap16,6];
				state[16] = theta[16] ^ stages[tap16,7];
			}

			Array.Copy(state, 0, accumulator, 0, 17);
			return Conversions.UIntToByte(hash, EndianType.LittleEndian);
		}
	}
}
