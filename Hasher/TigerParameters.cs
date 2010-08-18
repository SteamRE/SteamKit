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

namespace Classless.Hasher {
	/// <summary>A class that contains the parameters necessary to initialize a Tiger algorithm.</summary>
	public class TigerParameters : HashAlgorithmParameters {
		private short length;
		private TigerAlgorithmType algorithmType;


		/// <summary>Gets or sets the bit length of the result hash.</summary>
		/// <exception cref="ArgumentOutOfRangeException">When the specified value is not 128, 160, or 192.</exception>
		public short Length {
			get { return length; }
			set {
				if ((value != 128) && (value != 160) && (value != 192)) {
					throw new ArgumentOutOfRangeException("value", Hasher.Properties.Resources.invalidTigerLength);
				} else {
					length = value;
				}
			}
		}

		/// <summary>Gets or sets the Tiger algorithm variation.</summary>
		public TigerAlgorithmType AlgorithmType {
			get { return algorithmType; }
			set { algorithmType = value; }
		}


		/// <summary>Initializes a new instance of the TigerParameters class.</summary>
		/// <param name="length">The bit length of the final hash.</param>
		/// <param name="type">The Tiger algorithm variation.</param>
		public TigerParameters(short length, TigerAlgorithmType type) {
			this.Length = length;
			this.AlgorithmType = type;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:d}:{2})", this.GetType().Name, Length, AlgorithmType);
		}


		/// <summary>Retrieves a standard set of Tiger parameters.</summary>
		/// <param name="standard">The name of the standard parameter set to retrieve.</param>
		/// <returns>The Tiger Parameters for the given standard.</returns>
		public static TigerParameters GetParameters(TigerStandard standard) {
			TigerParameters temp = null;

			switch (standard) {
				case TigerStandard.Tiger128BitVersion1: temp = new TigerParameters(128, TigerAlgorithmType.Tiger1); break;
				case TigerStandard.Tiger160BitVersion1: temp = new TigerParameters(160, TigerAlgorithmType.Tiger1); break;
				case TigerStandard.Tiger192BitVersion1: temp = new TigerParameters(192, TigerAlgorithmType.Tiger1); break;
				case TigerStandard.Tiger128BitVersion2: temp = new TigerParameters(128, TigerAlgorithmType.Tiger2); break;
				case TigerStandard.Tiger160BitVersion2: temp = new TigerParameters(160, TigerAlgorithmType.Tiger2); break;
				case TigerStandard.Tiger192BitVersion2: temp = new TigerParameters(192, TigerAlgorithmType.Tiger2); break;
			}

			return temp;
		}
	}
}
