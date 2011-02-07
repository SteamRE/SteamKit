using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Vapor.Properties;
using SteamKit2;

namespace Vapor
{
    partial class MainForm : Form, ICallbackHandler
    {
        Timer sortTimer;

        public MainForm()
        {
            InitializeComponent();

            Steam3.AddHandler( this );

            this.Icon = Icon.FromHandle( Resources.MainIcon.GetHicon() );

            selfControl.IsHighlighted = false;
            selfControl.BorderStyle = BorderStyle.None;
        }

        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            Steam3.RemoveHandler( this );
            base.OnFormClosing( e );
        }

        public void HandleCallback( CallbackMsg msg )
        {
            if ( msg is PersonaStateCallback )
            {
                var perState = ( PersonaStateCallback )msg;

                if ( perState.FriendID == selfControl.Friend.SteamID )
                {
                    selfControl.SetSteamID( selfControl.Friend );
                    return;
                }

                if ( sortTimer == null )
                {
                    sortTimer = new Timer();
                    sortTimer.Tick += new EventHandler( sortTimer_Tick );
                    sortTimer.Interval = 10000;
                    sortTimer.Start();
                }
            }

            if ( msg is FriendsListCallback )
            {
                selfControl.SetSteamID( new Friend( Steam3.SteamUser.GetSteamID() ) );
                this.ReloadFriends();
            }

            if ( msg is LoginKeyCallback )
            {
                Steam3.SteamFriends.SetPersonaState( EPersonaState.Online );
            }
        }

        void sortTimer_Tick( object sender, EventArgs e )
        {
            sortTimer.Stop();
            this.ReloadFriends();
        }

        void ReloadFriends()
        {
            List<Friend> friendsList = GetFriends();

            friendsList.Sort( ( a, b ) =>
            {
                if ( a.IsInGame() && b.IsInGame() )
                    return StringComparer.OrdinalIgnoreCase.Compare( a.GetName(), b.GetName() );

                if ( !a.IsOnline() && !b.IsOnline() )
                    return StringComparer.OrdinalIgnoreCase.Compare( a.GetName(), b.GetName() );

                if ( a.IsOnline() && !a.IsInGame() && b.IsOnline() && !b.IsInGame() )
                    return StringComparer.OrdinalIgnoreCase.Compare( a.GetName(), b.GetName() );

                if ( a.IsInGame() && !b.IsInGame() )
                    return -1;

                if ( a.IsOnline() && !b.IsOnline() )
                    return -1;

                if ( !a.IsInGame() && b.IsInGame() )
                    return 1;

                if ( !a.IsOnline() && b.IsOnline() )
                    return 1;

                return 0;

            } );

            int scroll = friendsFlow.VerticalScroll.Value;

            friendsFlow.SuspendLayout();
            friendsFlow.Controls.Clear();

            foreach ( Friend friend in friendsList )
            {
                FriendControl fc = new FriendControl( friend );

                friendsFlow.Controls.Add( fc );
            }

            ResizeFriends();

            friendsFlow.ResumeLayout();
            friendsFlow.PerformLayout();
            friendsFlow.Refresh();

            friendsFlow.VerticalScroll.Value = scroll;
        }

        private void ResizeFriends()
        {
            foreach ( FriendControl fc in friendsFlow.Controls )
            {
                fc.Width = this.Width - 34;
            }
        }

        List<Friend> GetFriends()
        {
            List<Friend> friends = new List<Friend>();

            int friendCount = Steam3.SteamFriends.GetFriendCount();
            for ( int x = 0 ; x < friendCount ; ++x )
            {
                ulong friendId = Steam3.SteamFriends.GetFriendByIndex( x );

                Friend friend = new Friend( friendId );
                friends.Add( friend );
            }

            return friends;
        }

        private void MainForm_Resize( object sender, EventArgs e )
        {
            friendsFlow.SuspendLayout();

            ResizeFriends();

            friendsFlow.ResumeLayout();
        }

        private void refreshListToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.ReloadFriends();
        }
    }
}
