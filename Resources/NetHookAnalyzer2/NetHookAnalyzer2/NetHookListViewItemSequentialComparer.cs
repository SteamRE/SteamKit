using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NetHookAnalyzer2
{
	class NetHookListViewItemSequentialComparer : IComparer<ListViewItem>, IComparer
	{

		#region IComparer

		public int Compare(object x, object y)
		{
			if (x == null)
			{
				throw new ArgumentNullException("x");
			}

			if (y == null)
			{
				throw new ArgumentNullException("y");
			}

			var firstItem = x as ListViewItem;
			if (firstItem == null)
			{
				throw new ArgumentException("NetHookListViewItemSequentialComparer can only compare ListViewItems.", "x");
			}

			var secondItem = y as ListViewItem;
			if (secondItem == null)
			{
				throw new ArgumentException("NetHookListViewItemSequentialComparer can only compare ListViewItems.", "y");
			}

			return Compare(firstItem, secondItem);
		}

		#endregion

		#region IComparer<T>

		public int Compare(ListViewItem x, ListViewItem y)
		{
			if (x == null)
			{
				throw new ArgumentNullException("x");
			}

			if (y == null)
			{
				throw new ArgumentNullException("y");
			}

			var firstItem = (NetHookItem)x.Tag;
			var secondItem = (NetHookItem)y.Tag;

			return firstItem.Sequence - secondItem.Sequence;
		}

		#endregion
	}
}
