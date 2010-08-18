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
using System.Collections.Generic;

namespace Classless.Hasher {
	/// <summary>Represents a strongly typed list of HashAlgorithm objects that can be accessed by index.</summary>
	public class HashAlgorithmCollection : List<System.Security.Cryptography.HashAlgorithm> {
		/// <summary>The event triggered when the content of the List is changed.</summary>
		public event EventHandler<ChangedEventArgs> Changed;


		/// <summary>Initializes an instance of HashAlgorithmList.</summary>
		public HashAlgorithmCollection() : base() { }

		/// <summary>Initializes an instance of HashAlgorithmList that is empty and has the specified initial capacity.</summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		public HashAlgorithmCollection(int capacity) : base(capacity) { }

		/// <summary>Initializes an instance of HashAlgorithmList that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.</summary>
		/// <param name="collection">The collection whose elements are copied to the new List.</param>
		public HashAlgorithmCollection(IEnumerable<System.Security.Cryptography.HashAlgorithm> collection) : base() {
			AddRange(collection);
		}


		/// <summary>Triggers the Changed event for this List.</summary>
		/// <param name="args">Data about the event.</param>
		protected virtual void OnChanged(ChangedEventArgs args) {
			if (Changed != null) {
				Changed(this, args);
			}
		}


		/// <summary>Adds a HashAlgorithm to the end of the List.</summary>
		/// <param name="item">The HashAlgorithm to be added to the end of the List.</param>
		/// <exception cref="ArgumentNullException">When the specified HashAlgorithm is null.</exception>
		new public void Add(System.Security.Cryptography.HashAlgorithm item) {
			if (item == null) { throw new ArgumentNullException("item", Properties.Resources.hashCantBeNull); }

			base.Add(item);
			OnChanged(new ChangedEventArgs(item, ChangedEventType.Element));
		}


		/// <summary>Adds the elements of the specified collection to the end of the List.</summary>
		/// <param name="collection">The collection whose elements should be added to the end of the List.</param>
		new public void AddRange(IEnumerable<System.Security.Cryptography.HashAlgorithm> collection) {
			InsertRange(Count, collection);
		}


		/// <summary>Removes all elements from the List.</summary>
		new public void Clear() {
			base.Clear();
			OnChanged(new ChangedEventArgs(ChangedEventType.Element));
		}


		/// <summary>Inserts a HashAlgorithm into the List at the specified index.</summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The HashAlgorithm to insert.</param>
		/// <exception cref="ArgumentNullException">When the specified HashAlgorithm is null.</exception>
		new public void Insert(int index, System.Security.Cryptography.HashAlgorithm item) {
			if (item == null) { throw new ArgumentNullException("item", Properties.Resources.hashCantBeNull); }

			base.Insert(index, item);
			OnChanged(new ChangedEventArgs(item, ChangedEventType.Element));
		}


		/// <summary>Inserts the elements of a collection into the List at the specified index.</summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="collection">The collection whose elements should be inserted into the List.</param>
		/// <exception cref="ArgumentNullException">When a HashAlgorithm within the specified collection is null.</exception>
		new public void InsertRange(int index, IEnumerable<System.Security.Cryptography.HashAlgorithm> collection) {
			if (collection == null) { return; }

			System.Security.Cryptography.HashAlgorithm hasher = null;
			foreach (System.Security.Cryptography.HashAlgorithm item in collection) {
				if (item == null) { throw new ArgumentNullException("collection", Properties.Resources.hashCantBeNull); }
				if (hasher != null) { hasher = item; }
			}

			base.InsertRange(index, collection);
			OnChanged(new ChangedEventArgs(hasher, ChangedEventType.Element)); // We'll only report the first one.
		}


		/// <summary>Removes the first occurrence of a specific HashAlgorithm from the List.</summary>
		/// <param name="item">The HashAlgorithm to remove from the List.</param>
		/// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the List.</returns>
		new public bool Remove(System.Security.Cryptography.HashAlgorithm item) {
			int index = IndexOf(item);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			} else {
				return false;
			}
		}


		/// <summary>Removes the all the elements that match the conditions defined by the specified predicate.</summary>
		/// <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param>
		/// <returns>The number of elements removed from the List.</returns>
		new public int RemoveAll(Predicate<System.Security.Cryptography.HashAlgorithm> match) {
			int numRemoved = base.RemoveAll(match);
			if (numRemoved > 0) {
				OnChanged(new ChangedEventArgs(ChangedEventType.Element));
			}
			return numRemoved;
		}


		/// <summary>Removes the HashAlgorithm at the specified index of the List.</summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		new public void RemoveAt(int index) {
			System.Security.Cryptography.HashAlgorithm hasher = this[index];
			base.RemoveAt(index);
			OnChanged(new ChangedEventArgs(hasher, ChangedEventType.Element));
		}


		/// <summary>Removes a range of elements from the List.</summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove.</param>
		new public void RemoveRange(int index, int count) {
			System.Security.Cryptography.HashAlgorithm hasher = this[index];
			base.RemoveRange(index, count);
			OnChanged(new ChangedEventArgs(hasher, ChangedEventType.Element));
		}


		/// <summary>Reverses the order of the elements in the entire List.</summary>
		new public void Reverse() {
			Reverse(0, Count);
		}
		
		/// <summary>Reverses the order of the elements in the specified range.</summary>
		/// <param name="index">The zero-based starting index of the range to reverse.</param>
		/// <param name="count">The number of elements in the range to reverse.</param>
		new public void Reverse(int index, int count) {
			base.Reverse(index, count);
			OnChanged(new ChangedEventArgs(ChangedEventType.Order));
		}


		/// <summary>Sorts the elements in the entire List using the default comparer.</summary>
		new public void Sort() {
			Sort(0, Count, null);
		}

		/// <summary>Sorts the elements in the entire List using the specified comparer.</summary>
		/// <param name="comparer">The IComparer implementation to use when comparing elements, or null to use the default comparer Comparer.Default.</param>
		new public void Sort(IComparer<System.Security.Cryptography.HashAlgorithm> comparer) {
			Sort(0, Count, comparer);
		}

		/// <summary>Sorts the elements in the entire List using the specified System.Comparison.</summary>
		/// <param name="comparison">The System.Comparison to use when comparing elements.</param>
		new public void Sort(Comparison<System.Security.Cryptography.HashAlgorithm> comparison) {
			base.Sort(comparison);
			OnChanged(new ChangedEventArgs(ChangedEventType.Order));
		}

		/// <summary>Sorts the elements in a range of elements in List using the specified comparer.</summary>
		/// <param name="index">The zero-based starting index of the range to sort.</param>
		/// <param name="count">The length of the range to sort.</param>
		/// <param name="comparer">The IComparer implementation to use when comparing elements, or null to use the default comparer Comparer.Default.</param>
		new public void Sort(int index, int count, IComparer<System.Security.Cryptography.HashAlgorithm> comparer) {
			base.Sort(index, count, comparer);
			OnChanged(new ChangedEventArgs(ChangedEventType.Order));
		}
	}
}
