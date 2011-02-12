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

            chatFriend.SetSteamID( new Friend( steamId ) );
            chatFriend.BorderStyle = BorderStyle.None;

            this.Text = string.Format( "{0} - Chat", chatFriend.Friend.GetName() );

            this.chatFriend.IsHighlighted = false;
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

            switch ( type )
            {
                case EChatEntryType.ChatMsg:
                    this.AppendText( Util.GetStatusColor( new Friend( sender ) ), friendName );
                    this.AppendText( Color.White, ": " + msg );
                    break;

                case EChatEntryType.Emote:
                    this.AppendText( Color.White, "* " );
                    this.AppendText( Util.GetStatusColor( new Friend( sender ) ), friendName );
                    this.AppendText( Color.White, " " + msg );
                    break;

                default:
                    return;

            }

            this.AppendText( Environment.NewLine );
            this.ScrollLog();
        }
        public void HandleChat( SteamFriends.FriendMsgCallback friendMsg )
        {
            HandleChat( friendMsg.Sender, friendMsg.EntryType, friendMsg.Message );
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

            this.HandleChat( Steam3.SteamUser.GetSteamID(), type, msg );
            Steam3.SteamFriends.SendChatMessage( chatFriend.Friend.SteamID, type, msg );
        }

        private void txtLog_LinkClicked( object sender, LinkClickedEventArgs e )
        {
            Process.Start( e.LinkText );
        }
    }
}
