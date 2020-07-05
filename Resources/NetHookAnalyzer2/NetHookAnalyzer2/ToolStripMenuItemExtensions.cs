using System;
using System.Windows.Forms;

namespace NetHookAnalyzer2
{
	static class ToolStripMenuItemExtensions
	{
		public static ToolStripMenuItem AsRadioCheck(this ToolStripMenuItem item)
		{
			item.Click += OnClick;
			return item;
		}

		static void OnClick( object sender, EventArgs e )
		{
			if (!(sender is ToolStripMenuItem item))
			{
				return;
			}

			item.Checked = true;

			var parent = item.GetCurrentParent();
			while ( parent is ToolStripDropDownMenu parentMenu)
			{
				foreach ( var neighbouringItem in parentMenu.Items )
				{
					if ( neighbouringItem is ToolStripMenuItem menuItem && !item.Equals( menuItem ) )
					{
						menuItem.Checked = false;
					}
				}

				if ( parentMenu.OwnerItem is ToolStripMenuItem parentItem )
				{
					parentItem.Checked = true;
					parent = parentItem.GetCurrentParent();
				}
				else
				{
					parent = null;
				}
			}
		}
	}
}
