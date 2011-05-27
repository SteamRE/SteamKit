using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Vapor
{
    class VaporComboBox : ComboBox
    {
        public VaporComboBox()
        {
            this.BackColor = Color.FromArgb( 58, 58, 58 );
            this.ForeColor = Color.White;
            this.FlatStyle = FlatStyle.Flat;
        }
    }
}
