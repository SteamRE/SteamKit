using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SteamKit2;
using System.Diagnostics;

namespace Vapor
{
    partial class ChatDialog : VaporForm
    {
        public ChatDialog( SteamID steamId )
        {
            InitializeComponent();

            chatFriend.UpdateFriend( new Friend( steamId ) );

            chatFriend.CanOpenProfile = true;
            chatFriend.BorderStyle = BorderStyle.None;

            chatFriend.DisableContextMenu();
            chatFriend.DisableDoubleClick();

            this.Text = string.Format( "{0} - Chat", chatFriend.Friend.GetName() );

            this.chatFriend.IsHighlighted = false;
			
			this.txtChat.Select();
        }

        public new void Show()
        {
            txtChat.Focus();
            base.Show();
        }

        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            Steam3.ChatManager.Remove( chatFriend.Friend.SteamID );
            base.OnFormClosed( e );
        }


        public void HandleChat( SteamID sender, EChatEntryType type, string msg )
        {
            string friendName = Steam3.SteamFriends.GetFriendPersonaName( sender );
            string time = DateTime.Now.ToString( "h:mm tt" );

            var friend = new Friend( sender );
            var statusColor = Util.GetStatusColor( friend );

            switch ( type )
            {
                case EChatEntryType.ChatMsg:

                    this.AppendText( statusColor, string.Format( "{0} - {1}", time, friendName ) );
                    this.AppendText( Color.White, ": " + msg );

                    if ( sender != Steam3.SteamClient.SteamID )
                        FlashWindow();

                    break;

                case EChatEntryType.Emote:

                    this.AppendText( statusColor, string.Format( "{0} - {1}", time, friendName ) );
                    this.AppendText( statusColor, " " + msg );

                    if ( sender != Steam3.SteamClient.SteamID )
                        FlashWindow();

                    break;

                case EChatEntryType.InviteGame:
                    this.AppendText( statusColor, string.Format( "{0} - {1}", time, friendName ) );
                    this.AppendText( statusColor, " has invited you to play a game." );

                    if ( sender != Steam3.SteamClient.SteamID )
                        FlashWindow();

                    break;

                default:
                    return;

            }

            this.AppendText( Environment.NewLine );
            this.ScrollLog();
        }

        private void FlashWindow()
        {
            if ( this.Focused || txtChat.Focused )
            {
                // don't flash if we already have focus, because that would be silly!
                return;
            }

            Util.FlashWindow( this, true );
        }
        public void HandleChat( SteamFriends.FriendMsgCallback friendMsg )
        {
            HandleChat( friendMsg.Sender, friendMsg.EntryType, friendMsg.Message );
        }
        public void HandleState( SteamFriends.PersonaStateCallback personaState )
        {
            // todo: show name changes and update window title
        }

        void AppendText( string text )
        {
            txtLog.AppendText( text );
        }
        void AppendText( Color color, string text )
        {
            Color oldClr = txtLog.SelectionColor;

            int start = txtLog.TextLength;
            txtLog.AppendText( text );
            int end = txtLog.TextLength;

            txtLog.Select( start, end - start );
            txtLog.SelectionColor = color;
            txtLog.SelectionLength = 0;

            txtLog.SelectionColor = oldClr;
        }

        void ScrollLog()
        {
            if ( txtLog.SelectionStart == txtLog.TextLength )
                txtLog.ScrollToCaret();
        }

        private void txtChat_EnterPressed( object sender, EventArgs e )
        {
            if ( string.IsNullOrEmpty( txtChat.Text ) )
                return;

            string msg = txtChat.Text;
            txtChat.Clear();

            EChatEntryType type = EChatEntryType.ChatMsg;

            if ( msg.StartsWith( "/me ", StringComparison.OrdinalIgnoreCase ) )
            {
                msg = msg.Substring( 4, msg.Length - 4 );
                type = EChatEntryType.Emote;
            }

            this.HandleChat( Steam3.SteamUser.SteamID, type, msg );
            Steam3.SteamFriends.SendChatMessage( chatFriend.Friend.SteamID, type, msg );
        }

        private void txtLog_LinkClicked( object sender, LinkClickedEventArgs e )
        {
            Process.Start( e.LinkText );
        }

        private void ChatDialog_Load( object sender, EventArgs e )
        {
            Util.FlashWindow( this, false );
        }

        private void ChatDialog_Activated( object sender, EventArgs e )
        {
            Util.FlashWindow( this, false );
        }

    }
}
