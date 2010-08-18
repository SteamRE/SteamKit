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
		/// <summary>The class containing data for the Changed event.</summary>
		public class ChangedEventArgs : EventArgs {
			private System.Security.Cryptography.HashAlgorithm targetHashAlgorithm;
			private ChangedEventType changeType = ChangedEventType.Unknown;


			/// <summary>Gets the HashAlgorithm that was the target of the change in the List, if known.</summary>
			public System.Security.Cryptography.HashAlgorithm TargetHashAlgorithm {
				get { return targetHashAlgorithm; }
			}

			/// <summary>Gets the type of change that was made to the List.</summary>
			public ChangedEventType ChangeType {
				get { return changeType; }
			}


			/// <summary>Initializes an instance of ChangedEventArgs.</summary>
			public ChangedEventArgs() : base() { }

			/// <summary>Initializes an instance of ChangedEventArgs.</summary>
			/// <param name="targetHashAlgorithm">The HashAlgorithm that was the target of the change in the List.</param>
			public ChangedEventArgs(System.Security.Cryptography.HashAlgorithm targetHashAlgorithm) : base() {
				this.targetHashAlgorithm = targetHashAlgorithm;
			}

			/// <summary>Initializes an instance of ChangedEventArgs.</summary>
			/// <param name="type">The type of change that was made.</param>
			public ChangedEventArgs(ChangedEventType type) : base() {
				this.changeType = type;
			}

			/// <summary>Initializes an instance of ChangedEventArgs.</summary>
			/// <param name="targetHashAlgorithm">The HashAlgorithm that was the target of the change in the List.</param>
			/// <param name="type">The type of change that was made.</param>
			public ChangedEventArgs(System.Security.Cryptography.HashAlgorithm targetHashAlgorithm, ChangedEventType type) : base() {
				this.targetHashAlgorithm = targetHashAlgorithm;
				this.changeType = type;
			}
		}
}
