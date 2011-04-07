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
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void btnLogin_Click( object sender, EventArgs e )
        {
            this.Enabled = false;

            ClientTGT clientTgt;
            byte[] serverTgt;
            Blob accRecord;

            try
            {
                Steam2.Initialize( txtUser.Text, txtPass.Text, out clientTgt, out serverTgt, out accRecord );
            }
            catch ( Steam2Exception ex )
            {
                Util.MsgBox( this, "Unable to login to Steam2: " + ex.Message );
                this.Enabled = true;
                return;
            }

            try
            {
                Steam3.UserName = txtUser.Text;
                Steam3.Password = txtPass.Text;

                Steam3.ClientTGT = clientTgt;
                Steam3.ServerTGT = serverTgt;
                Steam3.AccountRecord = accRecord;

                Steam3.Initialize();

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
