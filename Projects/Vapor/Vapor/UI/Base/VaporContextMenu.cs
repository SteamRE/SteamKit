using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Vapor
{
    class VaporContextMenu : ContextMenuStrip
    {
        public VaporContextMenu()
        {
            this.BackColor = Color.FromArgb( 38, 38, 39 );
            this.ShowImageMargin = false;
        }
    }
}
