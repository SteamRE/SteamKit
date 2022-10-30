using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NetHookAnalyzer2
{
	[ToolboxItem( true )]
	[ToolboxBitmap( typeof( ListView ) )]
	class ListViewDoubleBuffered : ListView
	{
		public ListViewDoubleBuffered()
		{
			this.DoubleBuffered = true;
		}
	}
}
