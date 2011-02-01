using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tester
{
    public partial class LoginDialog : Form
    {

        public LoginDialog()
        {
            InitializeComponent();
        }

        private void btnLogin_Click( object sender, EventArgs e )
        {
            if ( !SteamContext.InitializeSteam2( txtUser.Text, txtPass.Text ) )
            {
                MessageBox.Show( "Unable to login to Steam2." );
                return;
            }

            SteamContext.InitializeSteam3();

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click( object sender, EventArgs e )
        {
            this.Close();
        }
    }
}
