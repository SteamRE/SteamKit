using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SteamKit2;

namespace Tester
{
    public partial class MessageDialog : Form
    {
        public SteamID FriendID { get; set; }

        public MessageDialog( SteamID friendId )
        {
            this.FriendID = friendId;

            InitializeComponent();

            string friendName = SteamContext.SteamFriends.GetFriendPersonaName( FriendID );
            this.Text = string.Format( "{0} - Chat", friendName );
        }

        

        public void RecvMessage( FriendMsgCallback msg )
        {
            string appendMsg = "";
            string friendName = SteamContext.SteamFriends.GetFriendPersonaName( FriendID );

            if ( msg.EntryType == EChatEntryType.Typing )
                appendMsg = string.Format( "* {0} is typing a message...", friendName );

            if ( msg.EntryType == EChatEntryType.ChatMsg )
                appendMsg = string.Format( "{0}: {1}", friendName, msg.Message );

            if ( msg.EntryType == EChatEntryType.Emote )
                appendMsg = string.Format( "* {0} {1}", friendName, msg.Message );

            appendMsg += Environment.NewLine;

            txtDisplay.AppendText( appendMsg );
            txtDisplay.ScrollToCaret();
        }

        private void txtChat_EnterPressed( object sender, EventArgs e )
        {
            if ( string.IsNullOrEmpty( txtChat.Text ) )
                return;

            string msg = txtChat.Text;
            txtChat.Text = "";

            SteamContext.SteamFriends.SendChatMessage( FriendID, EChatEntryType.ChatMsg, msg );

            string myName = SteamContext.SteamFriends.GetPersonaName();

            txtDisplay.AppendText( string.Format( "{0}: {1}{2}", myName, msg, Environment.NewLine ) );
            txtDisplay.ScrollToCaret();
        }
    }
}
