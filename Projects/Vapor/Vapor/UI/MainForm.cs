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
        public bool Relog { get; private set; }

        Timer sortTimer;


        public MainForm()
        {
            InitializeComponent();
            this.Enabled = false; // input is disabled until we login to steam3

            Steam3.AddHandler( this );

            selfControl.IsHighlighted = false;
            selfControl.BorderStyle = BorderStyle.None;
            selfControl.DisableContextMenu();
            selfControl.DisableDoubleClick();
        }

        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            Steam3.RemoveHandler( this );
            base.OnFormClosing( e );
        }

        public void HandleCallback( CallbackMsg msg )
        {
            if ( msg.IsType<SteamFriends.PersonaStateCallback>() )
            {
                var perState = ( SteamFriends.PersonaStateCallback )msg;

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

            if ( msg.IsType<SteamUser.LoggedOffCallback>() )
            {
                var callback = (SteamUser.LoggedOffCallback )msg;

                Util.MsgBox( this, string.Format( "Logged off from Steam3: {0}", callback.Result ) );

                this.Relog = true;
                this.Close();

                return;
            }

            if ( msg.IsType<SteamFriends.FriendsListCallback>() )
            {
                selfControl.SetSteamID( new Friend( Steam3.SteamUser.GetSteamID() ) );
                this.ReloadFriends();
            }

            if ( msg.IsType<SteamUser.LogOnCallback>() )
            {
                var logOnResp = ( SteamUser.LogOnCallback )msg;

                if ( logOnResp.Result == ( EResult )63 ) // temporary eresult for steamguard enabled
                {
                    SteamGuardDialog sgDialog = new SteamGuardDialog();

                    if ( sgDialog.ShowDialog( this ) != DialogResult.OK )
                    {
                        this.Relog = true;
                        this.Close();

                        return;
                    }

                    Steam3.AuthCode = sgDialog.AuthCode;

                    // if we got this logon response, we got disconnected, so lets reconnect
                    Steam3.Connect();
                }
                else if ( logOnResp.Result != EResult.OK )
                {
                    Util.MsgBox( this, string.Format( "Unable to login to Steam3. Result code: {0}", logOnResp.Result ) );

                    this.Relog = true;
                    this.Close();

                    return;
                }
            }

            if ( msg.IsType<SteamUser.LoginKeyCallback>() )
            {
                Steam3.SteamFriends.SetPersonaState( EPersonaState.Online );
                this.Enabled = true;
            }

            if ( msg.IsType<SteamFriends.FriendAddedCallback>() )
            {
                var friendAdded = ( SteamFriends.FriendAddedCallback )msg;

                if ( friendAdded.Result != EResult.OK )
                {
                    Util.MsgBox( this, "Unable to add friend! Result: " + friendAdded.Result );
                }
            }
        }

        void sortTimer_Tick( object sender, EventArgs e )
        {
            sortTimer.Stop();
            sortTimer.Dispose();

            this.ReloadFriends();
        }

        public void ReloadFriends()
        {
            List<Friend> friendsList = GetFriends();

            friendsList.Sort( ( a, b ) =>
            {
                if ( a == b )
                    return 0;

                // always show requesters on top
                if ( a.IsRequestingFriendship() )
                    return -1;

                if ( b.IsRequestingFriendship() )
                    return 1;


                // show people we've added at the bottom
                if ( a.IsAcceptingFriendship() )
                    return 1;

                if ( b.IsAcceptingFriendship() )
                    return -1;


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

        void ResizeFriends()
        {
            foreach ( FriendControl fc in friendsFlow.Controls )
            {
                fc.Width = this.ClientSize.Width - 6;

                if ( friendsFlow.VerticalScroll.Visible )
                    fc.Width -= 18;
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

        public void AddFriend()
        {
            var friendDialog = new AddFriendDialog();

            friendDialog.ShowDialog( this );
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
        private void btnAddFriend_Click( object sender, EventArgs e )
        {
            this.AddFriend();
        }
        private void addFriendToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.AddFriend();
        }
    }
}
