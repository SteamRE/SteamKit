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
	/// <summary>A class that contains the parameters necessary to initialize a Sum algorithm.</summary>
	public class SumParameters : HashAlgorithmParameters {
		private int order;


		/// <summary>Gets or sets the order of the Sum (e.g., how many bits).</summary>
		/// <exception cref="ArgumentOutOfRangeException">When the specified value is not a multiple of 8, is less than 8, or is greater than 64.</exception>
		public int Order {
			get { return order; }
			set {
				if (((value % 8) != 0) || (value < 8) || (value > 64)) {
					throw new ArgumentOutOfRangeException("value", value, Properties.Resources.invalidSumOrder);
				} else {
					order = value;
				}
			}
		}


		/// <summary>Initializes a new instance of the SumParameters class.</summary>
		/// <param name="order">The order of the Sum (e.g., how many bits).</param>
		public SumParameters(int order) {
			Order = order;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:d})", this.GetType().Name, Order);
		}


		/// <summary>Retrieves a standard set of Sum parameters.</summary>
		/// <param name="standard">The name of the standard parameter set to retrieve.</param>
		/// <returns>The Sum Parameters for the given standard.</returns>
		public static SumParameters GetParameters(SumStandard standard) {
			SumParameters temp = null;

			switch (standard) {
				case SumStandard.Sum8Bit: temp = new SumParameters(8); break;
				case SumStandard.Sum16Bit: temp = new SumParameters(16); break;
				case SumStandard.Sum24Bit: temp = new SumParameters(24); break;
				case SumStandard.Sum32Bit: temp = new SumParameters(32); break;
				case SumStandard.Sum64Bit: temp = new SumParameters(64); break;
			}

			return temp;
		}
	}
}
