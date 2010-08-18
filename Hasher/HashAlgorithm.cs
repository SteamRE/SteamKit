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
using Classless.Hasher.Methods;
using Classless.Hasher.Utilities;

namespace Classless.Hasher {
	/// <summary>Represents the base class from which all implementations of cryptographic hash algorithms must derive.</summary>
	abstract public class HashAlgorithm : System.Security.Cryptography.HashAlgorithm {
		/// <summary>Initializes the algorithm.</summary>
		/// <remarks>If this function is overriden in a derived class, the new function should call back to
		/// this function or you could risk garbage being carried over from one calculation to the next.</remarks>
		override public void Initialize() {
			State = 0;
		}


		/// <summary>Returns a String that represents the current Object.</summary>
		/// <returns>A String that represents the current Object.</returns>
		override public string ToString() {
			IParametrizedHashAlgorithm ipha = this as IParametrizedHashAlgorithm;
			if (ipha != null) {
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1})", base.ToString(), ipha.Parameters.ToString());
			} else {
				return base.ToString();
			}
		}


		/// <summary>Creates an instance of the default implementation of HashAlgorithm.</summary>
		/// <returns>An instance of a cryptographic object to perform the hash algorithm.</returns>
		/// <remarks>The default implementation of HashAlgorithm is SHA1.</remarks>
		new public static System.Security.Cryptography.HashAlgorithm Create() {
			return HashAlgorithm.Create("Classless.Hasher.Sha1");
		}

		/// <summary>Creates an instance of the specified implementation of HashAlgorithm.</summary>
		/// <param name="hashName">The implementation of HashAlgorithm to create.</param>
		/// <returns>An instance of a cryptographic object to perform the hash algorithm.</returns>
		new public static System.Security.Cryptography.HashAlgorithm Create(string hashName) {
			string thisAssembly = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
			string normalizedName = hashName;
			if (!normalizedName.StartsWith(thisAssembly, StringComparison.Ordinal)) {
				normalizedName = thisAssembly + "." + normalizedName;
			}

			try {
				return (System.Security.Cryptography.HashAlgorithm)Activator.CreateInstance(thisAssembly, normalizedName).Unwrap();
			} catch (TypeLoadException) {
				try {
					// It's not one of ours, let the host framework figure it out.
					return System.Security.Cryptography.HashAlgorithm.Create(hashName);
				} catch (InvalidCastException) {
					return null;
				}
			} catch (InvalidCastException) {
				return null;
			}
		}
	}
}
