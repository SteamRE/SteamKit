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
	/// <summary>A class that contains the parameters necessary to initialize a Fletcher algorithm.</summary>
	public class FletcherParameters : HashAlgorithmParameters {
		private int order;


		/// <summary>Gets or sets the order of the Sum (e.g., how many bits).</summary>
		/// <exception cref="ArgumentOutOfRangeException">When the specified value is not 8, 16, or 32.</exception>
		public int Order {
			get { return order; }
			set {
				if ((value != 8) && (value != 16) && (value != 32)) {
					throw new ArgumentOutOfRangeException("value", value, Properties.Resources.invalidFletcherOrder);
				} else {
					order = value;
				}
			}
		}


		/// <summary>Initializes a new instance of the FletcherParameters class.</summary>
		/// <param name="order">The order of the Fletcher (e.g., how many bits).</param>
		public FletcherParameters(int order) {
			Order = order;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:d})", this.GetType().Name, Order);
		}


		/// <summary>Retrieves a standard set of Fletcher parameters.</summary>
		/// <param name="standard">The name of the standard parameter set to retrieve.</param>
		/// <returns>The Fletcher Parameters for the given standard.</returns>
		public static FletcherParameters GetParameters(FletcherStandard standard) {
			FletcherParameters temp = null;

			switch (standard) {
				case FletcherStandard.Fletcher8Bit: temp = new FletcherParameters(8); break;
				case FletcherStandard.Fletcher16Bit: temp = new FletcherParameters(16); break;
				case FletcherStandard.Fletcher32Bit: temp = new FletcherParameters(32); break;
			}

			return temp;
		}
	}
}
