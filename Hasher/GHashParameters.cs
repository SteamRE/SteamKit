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
	/// <summary>A class that contains the parameters necessary to initialize a GHash algorithm.</summary>
	public class GHashParameters : HashAlgorithmParameters {
		private int shift;


		/// <summary>Gets or sets the shift value.</summary>
		public int Shift {
			get { return shift; }
			set { shift = value; }
		}


		/// <summary>Initializes a new instance of the GHashParamters class.</summary>
		/// <param name="shift">How many bits to shift.</param>
		public GHashParameters(int shift) {
			Shift = shift;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:d})", this.GetType().Name, Shift);
		}


		/// <summary>Retrieves a standard set of GHash parameters.</summary>
		/// <param name="standard">The name of the standard parameter set to retrieve.</param>
		/// <returns>The GHash Parameters for the given standard.</returns>
		public static GHashParameters GetParameters(GHashStandard standard) {
			GHashParameters temp = null;

			switch (standard) {
				case GHashStandard.GHash3:	temp = new GHashParameters(3);	break;
				case GHashStandard.GHash5:	temp = new GHashParameters(5);	break;
			}

			return temp;
		}
	}
}
