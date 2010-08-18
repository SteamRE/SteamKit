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
	/// <summary>A class that contains the parameters necessary to initialize a Snefru2 algorithm.</summary>
	public class Snefru2Parameters : HashAlgorithmParameters {
		private short passes;
		private short length;


		/// <summary>Gets or sets the number of passes.</summary>
		/// <exception cref="ArgumentOutOfRangeException">When the specified value is not either 4 or 8.</exception>
		public short Passes {
			get { return passes; }
			set {
				if ((value != 4) && (value != 8)) {
					throw new ArgumentOutOfRangeException("value", Hasher.Properties.Resources.invalidSnefruPasses);
				} else {
					passes = value;
				}
			}
		}

		/// <summary>Gets or sets the bit length.</summary>
		/// <exception cref="ArgumentOutOfRangeException">When the specified value is not either 128 or 256.</exception>
		public short Length {
			get { return length; }
			set {
				if ((value != 128) && (value != 256)) {
					throw new ArgumentOutOfRangeException("value", Hasher.Properties.Resources.invalidSnefruLength);
				} else {
					length = value;
				}
			}
		}


		/// <summary>Initializes a new instance of the Snefru2Paramters class.</summary>
		/// <param name="passes">How many transformation passes to do.</param>
		/// <param name="length">The bit length of the final hash.</param>
		public Snefru2Parameters(short passes, short length) {
			this.Passes = passes;
			this.Length = length;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:d}:{2:d})", this.GetType().Name, Passes, Length);
		}


		/// <summary>Retrieves a standard set of Snefru2 parameters.</summary>
		/// <param name="standard">The name of the standard parameter set to retrieve.</param>
		/// <returns>The Snefru2 Parameters for the given standard.</returns>
		public static Snefru2Parameters GetParameters(Snefru2Standard standard) {
			Snefru2Parameters temp = null;

			switch (standard) {
				case Snefru2Standard.Snefru128Bit4Pass:	temp = new Snefru2Parameters(4, 128);	break;
				case Snefru2Standard.Snefru256Bit4Pass:	temp = new Snefru2Parameters(4, 256);	break;
				case Snefru2Standard.Snefru128Bit8Pass:	temp = new Snefru2Parameters(8, 128);	break;
				case Snefru2Standard.Snefru256Bit8Pass:	temp = new Snefru2Parameters(8, 256);	break;
			}

			return temp;
		}
	}
}
