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
	/// <summary>Computes the Jenkins Hash for the input data using the managed library.</summary>
	public class JenkinsHash : BlockHashAlgorithm {
		private readonly object syncLock = new object();

		private uint[] accumulator = new uint[] { 0x9E3779B9, 0x9E3779B9, 0 };
		private uint length;


		/// <summary>Initializes a new instance of the JHash class.</summary>
		public JenkinsHash() : base(12) {
			HashSizeValue = 32;
		}


		/// <summary>Initializes an implementation of System.Security.Cryptography.HashAlgorithm.</summary>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				accumulator = new uint[] { 0x9E3779B9, 0x9E3779B9, 0 };
				length = 0;
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		override protected void ProcessBlock(byte[] inputBuffer, int inputOffset) {
			lock (syncLock) {
				accumulator[0] += (inputBuffer[inputOffset + 0] + (((uint)inputBuffer[inputOffset + 1]) << 8) + (((uint)inputBuffer[inputOffset + 2]) << 16) +  (((uint)inputBuffer[inputOffset + 3]) << 24));
				accumulator[1] += (inputBuffer[inputOffset + 4] + (((uint)inputBuffer[inputOffset + 5]) << 8) + (((uint)inputBuffer[inputOffset + 6]) << 16) +  (((uint)inputBuffer[inputOffset + 7]) << 24));
				accumulator[2] += (inputBuffer[inputOffset + 8] + (((uint)inputBuffer[inputOffset + 9]) << 8) + (((uint)inputBuffer[inputOffset + 10]) << 16) + (((uint)inputBuffer[inputOffset + 11]) << 24));
				length += 12;

				mixAccumulator();
			}
		}


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		override protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			lock (syncLock) {
				accumulator[2] += length + (uint)inputCount;

				switch (inputCount) {
					case 11: accumulator[2] += ((uint)inputBuffer[inputOffset + 10] << 24);	goto case 10;
					case 10: accumulator[2] += ((uint)inputBuffer[inputOffset + 9] << 16);	goto case 9;
					case 9:  accumulator[2] += ((uint)inputBuffer[inputOffset + 8] << 8);	goto case 8;
					case 8:  accumulator[1] += ((uint)inputBuffer[inputOffset + 7] << 24);	goto case 7;
					case 7:  accumulator[1] += ((uint)inputBuffer[inputOffset + 6] << 16);	goto case 6;
					case 6:  accumulator[1] += ((uint)inputBuffer[inputOffset + 5] << 8);	goto case 5;
					case 5:  accumulator[1] += ((uint)inputBuffer[inputOffset + 4]);		goto case 4;
					case 4:  accumulator[0] += ((uint)inputBuffer[inputOffset + 3] << 24);	goto case 3;
					case 3:  accumulator[0] += ((uint)inputBuffer[inputOffset + 2] << 16);	goto case 2;
					case 2:  accumulator[0] += ((uint)inputBuffer[inputOffset + 1] << 8);	goto case 1;
					case 1:  accumulator[0] += ((uint)inputBuffer[inputOffset + 0]);		break;
				}

				mixAccumulator();

				return Conversions.UIntToByte(accumulator[2], EndianType.BigEndian);
			}
		}


		private void mixAccumulator() {
			accumulator[0] -= accumulator[1]; accumulator[0] -= accumulator[2]; accumulator[0] ^= (accumulator[2] >> 13);
			accumulator[1] -= accumulator[2]; accumulator[1] -= accumulator[0]; accumulator[1] ^= (accumulator[0] << 8);
			accumulator[2] -= accumulator[0]; accumulator[2] -= accumulator[1]; accumulator[2] ^= (accumulator[1] >> 13);
			accumulator[0] -= accumulator[1]; accumulator[0] -= accumulator[2]; accumulator[0] ^= (accumulator[2] >> 12);
			accumulator[1] -= accumulator[2]; accumulator[1] -= accumulator[0]; accumulator[1] ^= (accumulator[0] << 16);
			accumulator[2] -= accumulator[0]; accumulator[2] -= accumulator[1]; accumulator[2] ^= (accumulator[1] >> 5);
			accumulator[0] -= accumulator[1]; accumulator[0] -= accumulator[2]; accumulator[0] ^= (accumulator[2] >> 3);
			accumulator[1] -= accumulator[2]; accumulator[1] -= accumulator[0]; accumulator[1] ^= (accumulator[0] << 10);
			accumulator[2] -= accumulator[0]; accumulator[2] -= accumulator[1]; accumulator[2] ^= (accumulator[1] >> 15);
		}
	}
}
