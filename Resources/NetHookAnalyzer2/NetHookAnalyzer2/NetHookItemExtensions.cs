using System.Globalization;
using System.Windows.Forms;

namespace NetHookAnalyzer2
{
	static class NetHookItemExtensions
	{
		public static ListViewItem AsListViewItem(this NetHookItem item)
		{
			var lvi = new ListViewItem(item.EMsg.ToString())
			{
				Tag = item
			};

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem
			{
				Name = "#",
				Text = item.Sequence.ToString(),
			});

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem
			{
				Name = "Timestamp",
				Text = item.Timestamp.ToString(CultureInfo.CurrentUICulture),
			});

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem
			{
				Name = "Direction",
				Text = item.Direction.ToString().ToLowerInvariant(),
			});

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem
			{
				Name = "Message",
				Text = item.EMsg.ToString(),
			});

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem
			{
				Name = "Inner Message",
				Text = item.InnerMessageName,
			});

			return lvi;
		}

		public static NetHookItem GetNetHookItem(this ListViewItem item)
		{
			return item.Tag as NetHookItem;
		}
	}
}
