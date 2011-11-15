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

        bool shouldClose;

        bool suppressStateMsg;
        bool expectDisconnect;


        public MainForm()
        {
            InitializeComponent();
            this.Enabled = false; // input is disabled until we login to steam3

            Steam3.AddHandler( this );

            selfControl.IsHighlighted = false;
            selfControl.BorderStyle = BorderStyle.None;
            selfControl.CanOpenProfile = true;

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
                    selfControl.UpdateFriend( selfControl.Friend );

                    suppressStateMsg = true;
                    stateComboBox.SelectedIndex = GetIndexFromState( perState.State );
                    suppressStateMsg = false;

                    return;
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
                selfControl.UpdateFriend( new Friend( Steam3.SteamUser.GetSteamID() ) );
                this.UpdateFriends();
            }

            if ( msg.IsType<SteamUser.LogOnCallback>() )
            {
                var logOnResp = ( SteamUser.LogOnCallback )msg;

                if ( logOnResp.Result == EResult.AccountLogonDenied )
                {
                    expectDisconnect = true;

                    SteamGuardDialog sgDialog = new SteamGuardDialog();

                    if ( sgDialog.ShowDialog( this ) != DialogResult.OK )
                    {
                        this.Relog = true;
                        this.Close();

                        return;
                    }

                    Steam3.AuthCode = sgDialog.AuthCode;

                    // if we got this logon response, we got disconnected, so lets reconnect
                    try
                    {
                        Steam3.Connect();
                    }
                    catch ( Steam3Exception ex )
                    {
                        Util.MsgBox( this, string.Format( "Unable to connect to Steam3: {0}", ex.Message ) );

                        this.Relog = true;
                        this.Close();

                        return;
                    }
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

            msg.Handle<SteamClient.DisconnectCallback>( ( callback ) =>
                {
                    // if we expected this disconnection (cause of steamguard), we do nothing
                    if ( expectDisconnect )
                    {
                        expectDisconnect = false;
                        return;
                    }

                    Util.MsgBox( this, "Disconnected from Steam3!" );

                    this.Relog = true;
                    this.Close();

                    return;
                } );
        }

        private static int compareFriends(Friend a, Friend b)
        {
            if (a == b)
                return 0;

            // always show requesters on top
            if (a.IsRequestingFriendship())
                return -1;

            if (b.IsRequestingFriendship())
                return 1;


            // show people we've added at the bottom
            if (a.IsAcceptingFriendship())
                return 1;

            if (b.IsAcceptingFriendship())
                return -1;


            if (a.IsInGame() && b.IsInGame())
                return StringComparer.OrdinalIgnoreCase.Compare(a.GetName(), b.GetName());

            if (!a.IsOnline() && !b.IsOnline())
                return StringComparer.OrdinalIgnoreCase.Compare(a.GetName(), b.GetName());

            if (a.IsOnline() && !a.IsInGame() && b.IsOnline() && !b.IsInGame())
                return StringComparer.OrdinalIgnoreCase.Compare(a.GetName(), b.GetName());

            if (a.IsInGame() && !b.IsInGame())
                return -1;

            if (a.IsOnline() && !b.IsOnline())
                return -1;

            if (!a.IsInGame() && b.IsInGame())
                return 1;

            if (!a.IsOnline() && b.IsOnline())
                return 1;

            return 0;
        }

        // sort and reflow controls
        public void UpdateFriends()
        {
            List<Friend> friendsList = GetFriends();
            friendsList.Sort(compareFriends);

            int scroll = friendsFlow.VerticalScroll.Value;

            friendsFlow.SuspendLayout();

            int clientIndex = 0;

            foreach ( Friend friend in friendsList )
            {
                FriendControl friendControl = null;
                
                foreach( FriendControl fc in friendsFlow.Controls )
                {
                    if( fc.Friend.Equals(friend) )
                    {
                        friendControl = fc;
                        break;
                    }
                }

                if ( friendControl == null )
                {
                    friendControl = new FriendControl( friend );
                    friendsFlow.Controls.Add( friendControl );
                }
                else
                {
                    friendControl.UpdateFriend( friend );
                }

                friendsFlow.Controls.SetChildIndex( friendControl, clientIndex );
                clientIndex++;
            }

            List<FriendControl> controlsToRemove = new List<FriendControl>();
            foreach (FriendControl fc in friendsFlow.Controls)
            {
                if (!friendsList.Contains(fc.Friend))
                {
                    controlsToRemove.Add(fc);
                }
            }

            foreach (FriendControl fc in controlsToRemove)
                friendsFlow.Controls.Remove(fc);

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
                SteamID friendId = Steam3.SteamFriends.GetFriendByIndex( x );

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


        private void refreshListToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.UpdateFriends();
        }
        private void btnAddFriend_Click( object sender, EventArgs e )
        {
            this.AddFriend();
        }
        private void addFriendToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.AddFriend();
        }

        private void MainForm_FormClosing( object sender, FormClosingEventArgs e )
        {
#if TRAY_BUILD
            if ( shouldClose )
            {
                return;
            }

            e.Cancel = true;
            this.Hide();
#endif
        }

        private void vaporContextMenu2_Opening( object sender, CancelEventArgs e )
        {
            showHideToolStripMenuItem.Text = ( this.Visible ? "Hide" : "Show" );
        }

        private void showHideToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToggleFormVisibility();
        }

        private void ToggleFormVisibility()
        {
            if ( this.Visible )
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.Focus();
            }
        }

        private void exitToolStripMenuItem_Click( object sender, EventArgs e )
        {
            shouldClose = true;
            this.Close();
        }


        // translates a state combo box selection index to persona state
        EPersonaState GetStateFromIndex( int idx )
        {
            // note: this _MUST_ match the UI items
            EPersonaState[] states =
            {
                EPersonaState.Online,
                EPersonaState.Away,
                EPersonaState.Busy,
                EPersonaState.Snooze,
                EPersonaState.Offline,
            };

            if ( idx < 0 || idx >= states.Length )
            {
                return EPersonaState.Online;
            }

            return states[ idx ];
        }

        // translates persona state to state combo box selection index
        int GetIndexFromState( EPersonaState state )
        {
            switch ( state )
            {
                case EPersonaState.Online:
                    return 0;

                case EPersonaState.Away:
                    return 1;

                case EPersonaState.Busy:
                    return 2;

                case EPersonaState.Snooze:
                    return 3;

                case EPersonaState.Offline:
                    return 4;
            }

            return 0;
        }

        private void stateComboBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( suppressStateMsg )
            {
                return;
            }

            Steam3.SteamFriends.SetPersonaState( GetStateFromIndex( stateComboBox.SelectedIndex ) );
        }

        private void MainForm_ResizeEnd( object sender, EventArgs e )
        {
            friendsFlow.SuspendLayout();

            ResizeFriends();

            friendsFlow.ResumeLayout();
        }

        private void notifyIcon1_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            ToggleFormVisibility();
        }
    }
}
