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
	/// <summary>Represents the abstract class from which all implementations of the Classless.Hasher.BlockHashAlgorithm inherit.</summary>
	abstract public class BlockHashAlgorithm : HashAlgorithm {
		private readonly object syncLock = new object();

		private int blockSize;
		private byte[] buffer;
		private int bufferCount;
		private long count;


		/// <summary>The size in bytes of an individual block.</summary>
		public int BlockSize {
			get { return blockSize; }
		}

		/// <summary>The number of bytes currently in the buffer waiting to be processed.</summary>
		public int BufferCount {
			get { return bufferCount; }
		}

		/// <summary>The number of bytes that have been processed.</summary>
		/// <remarks>This number does NOT include the bytes that are waiting in the buffer.</remarks>
		public long Count {
			get { return count; }
		}


		/// <summary>Initializes a new instance of the BlockHashAlgorithm class.</summary>
		/// <param name="blockSize">The size in bytes of an individual block.</param>
		protected BlockHashAlgorithm(int blockSize) : base() {
			if (blockSize < 1) {
				throw new ArgumentException(Hasher.Properties.Resources.invalidBlockSize, "blockSize");
			}
			this.blockSize = blockSize;
			buffer = new byte[BlockSize];
		}


		/// <summary>Initializes the algorithm.</summary>
		/// <remarks>If this function is overriden in a derived class, the new function should call back to
		/// this function or you could risk garbage being carried over from one calculation to the next.</remarks>
		override public void Initialize() {
			lock (syncLock) {
				base.Initialize();
				count = 0;
				bufferCount = 0;
				buffer = new byte[BlockSize];
			}
		}


		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="ibStart">The position in the array to begin reading from.</param>
		/// <param name="cbSize">How many bytes in the array to read.</param>
		override protected void HashCore(byte[] array, int ibStart, int cbSize) {
			lock (syncLock) {
				int i;

				// Use what may already be in the buffer.
				if (BufferCount > 0) {
					if (cbSize < (BlockSize - BufferCount)) {
						// Still don't have enough for a full block, just store it.
						Array.Copy(array, ibStart, buffer, BufferCount, cbSize);
						bufferCount += cbSize;
						return;
					} else {
						// Fill out the buffer to make a full block, and then process it.
						i = BlockSize - BufferCount;
						Array.Copy(array, ibStart, buffer, BufferCount, i);
						ProcessBlock(buffer, 0);
						count += (long)BlockSize;
						bufferCount = 0;
						ibStart += i;
						cbSize -= i;
					}
				}

				// For as long as we have full blocks, process them.
				for (i = 0; i < (cbSize - (cbSize % BlockSize)); i += BlockSize) {
					ProcessBlock(array, ibStart + i);
					count += (long)BlockSize;
				}

				// If we still have some bytes left, store them for later.
				int bytesLeft = cbSize % BlockSize;
				if (bytesLeft != 0) {
					Array.Copy(array, ((cbSize - bytesLeft) + ibStart), buffer, 0, bytesLeft);
					bufferCount = bytesLeft;
				}
			}
		}


		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		override protected byte[] HashFinal() {
			lock (syncLock) {
				return ProcessFinalBlock(buffer, 0, bufferCount);
			}
		}


		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		abstract protected void ProcessBlock(byte[] inputBuffer, int inputOffset);


		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		abstract protected byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
	}
}
