using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using SteamKit2;

namespace Vapor
{
    partial class LoginDialog : VaporForm
    {
        bool useUdp;

        public LoginDialog( bool useUdp )
        {
            InitializeComponent();

            this.useUdp = useUdp;
        }

        private void btnLogin_Click( object sender, EventArgs e )
        {
            this.Enabled = false;

            try
            {
                Steam3.UserName = txtUser.Text;
                Steam3.Password = txtPass.Text;

                Steam3.Initialize( useUdp );

                Steam3.Connect();
            }
            catch ( Steam3Exception ex )
            {
                Util.MsgBox( this, "Unable to login to Steam3: " + ex.Message );
                this.Enabled = true;
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
