using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Vapor
{
    class FriendsListControl : FlowLayoutPanel
    {

        /*
        public FriendControl GetFriendControl( Friend friend )
        {
            foreach ( FriendControl fc in this.Controls )
            {
                if ( fc.Friend.SteamID == friend.SteamID )
                    return fc;
            }

            return null;
        }*/

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FriendsListControl
            // 
            this.AutoScroll = true;
            this.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ResumeLayout( false );

        }
    }
}
