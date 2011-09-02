using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Vapor
{
    partial class ErrorDialog : VaporForm
    {
        public ErrorDialog( Exception ex )
        {
            InitializeComponent();

            txtException.Text = ex.ToString();
        }

        private void btnOk_Click( object sender, EventArgs e )
        {
            this.Close();
        }
    }
}
