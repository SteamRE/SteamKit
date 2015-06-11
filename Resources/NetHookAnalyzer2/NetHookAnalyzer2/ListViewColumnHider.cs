using System;
using System.Windows.Forms;

namespace NetHookAnalyzer2
{
	class ListViewColumnHider : IDisposable
	{
		public ListViewColumnHider(ListView listView, int columnIndex)
		{
			this.listView = listView;
			this.columnIndex = columnIndex;

			listView.Columns[columnIndex].Width = 0;
			listView.ColumnWidthChanging += OnColumnWidthChanging;
		}

		readonly ListView listView;
		readonly int columnIndex;

		void OnColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
		{
			// Don't allow the user to display the first column (item name).
			if (e.ColumnIndex == columnIndex)
			{
				e.Cancel = true;
				e.NewWidth = 0;
			}
		}
		
		void IDisposable.Dispose()
		{
			listView.ColumnWidthChanging -= OnColumnWidthChanging;
		}
	}
}
