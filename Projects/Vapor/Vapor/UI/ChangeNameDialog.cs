using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SteamKit2;

namespace Vapor
{
    partial class ChangeNameDialog : VaporForm
    {
        public ChangeNameDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrEmpty( txtName.Text ) )
                return;

            Steam3.SteamFriends.SetPersonaName( txtName.Text );

            this.Close();
        }

        private void btnCancel_Click( object sender, EventArgs e )
        {
            this.Close();
        }
    }
}
