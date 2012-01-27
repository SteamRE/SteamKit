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
    partial class AddFriendDialog : VaporForm
    {
        public AddFriendDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click( object sender, EventArgs e )
        {
            SteamID steamId = new SteamID();
            steamId.SetFromString( txtFriend.Text, Steam3.SteamClient.ConnectedUniverse );

            if ( steamId.IsValid )
            {
                Steam3.SteamFriends.AddFriend( steamId );
            }
            else
            {
                Steam3.SteamFriends.AddFriend( txtFriend.Text );
            }

            this.Close();
        }

        private void btnCancel_Click( object sender, EventArgs e )
        {
            this.Close();
        }
    }
}
