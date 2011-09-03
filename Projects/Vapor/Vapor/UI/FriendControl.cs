using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Vapor.Properties;
using System.Linq;
using SteamKit2;

namespace Vapor
{
    partial class FriendControl : UserControl, ICallbackHandler
    {

        public Friend Friend { get; private set; }
        public bool IsHighlighted { get; set; }
        public bool CanOpenProfile { get; set; }

        bool highlighted;


        public FriendControl()
        {
            InitializeComponent();

            btnAccept.Visible = false;
            btnDeny.Visible = false;

            IsHighlighted = true;

            Steam3.AddHandler( this );

            this.MouseDoubleClick += FriendControl_MouseDoubleClick;
            this.MouseEnter += FriendControl_MouseEnter;
            this.MouseLeave += FriendControl_MouseLeave;

            foreach ( Control ctrl in this.Controls )
            {
                ctrl.MouseDoubleClick += FriendControl_MouseDoubleClick;
                ctrl.MouseEnter += FriendControl_MouseEnter;
                ctrl.MouseLeave += FriendControl_MouseLeave;
            }


            if ( Friend == null )
                return;

            avatarBox.Image = ComposeAvatar( this.Friend, null );

        }
        ~FriendControl()
        {
            Steam3.RemoveHandler( this );
        }

        public FriendControl( Friend steamid )
            : this()
        {
            SetSteamID( steamid );
        }

        public void DisableContextMenu()
        {
            this.ContextMenuStrip = null;
        }
        public void DisableDoubleClick()
        {
            this.MouseDoubleClick -= FriendControl_MouseDoubleClick;

            foreach ( Control ctrl in this.Controls )
                ctrl.MouseDoubleClick -= FriendControl_MouseDoubleClick;
        }


        public void HandleCallback( CallbackMsg msg )
        {
            if ( msg.IsType<SteamFriends.PersonaStateCallback>() )
            {
                var perState = ( SteamFriends.PersonaStateCallback )msg;

                if ( this.Friend == null )
                    return;

                if ( perState.FriendID != this.Friend.SteamID )
                    return;

                this.SetSteamID( this.Friend );

                if ( perState.AvatarHash != null && !Util.IsZeros( perState.AvatarHash ) )
                {
                    CDNCache.DownloadAvatar( perState.FriendID, perState.AvatarHash, AvatarDownloaded );
                }
            }
        }

        void AvatarDownloaded( AvatarDownloadDetails details )
        {
            try
            {
                avatarBox.Image = ComposeAvatar( this.Friend, ( details.Success ? details.Filename : null ) );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "FriendControl", "Unable to compose avatar: {0}", ex.Message );
            }
        }

        public void SetSteamID( Friend steamid )
        {
            Friend = steamid;

            nameLbl.Text = steamid.GetName();
            statusLbl.Text = steamid.GetStatus();
            gameLbl.Text = steamid.GetGameName();

            if ( steamid.IsRequestingFriendship() )
            {
                btnAccept.Visible = true;
                btnDeny.Visible = true;
            }

            nameLbl.ForeColor = statusLbl.ForeColor = gameLbl.ForeColor = Util.GetStatusColor( steamid );

            byte[] avatarHash = Steam3.SteamFriends.GetFriendAvatar( steamid.SteamID );

            if ( avatarHash == null )
            {
                avatarBox.Image = ComposeAvatar( this.Friend, null );
                return;
            }

            CDNCache.DownloadAvatar( steamid.SteamID, avatarHash, AvatarDownloaded );
        }


        void Highlight()
        {
            if ( !this.IsHighlighted )
                return;

            if ( this.highlighted )
                return;

            this.BackColor = Color.FromArgb( 38, 38, 39 );

            highlighted = true;
        }
        void UnHighlight()
        {
            if ( !this.IsHighlighted )
                return;

            if ( !this.highlighted )
                return;

            this.BackColor = Color.FromArgb( 58, 58, 58 );

            highlighted = false;
        }


        Bitmap GetHolder( Friend steamid )
        {
            if ( steamid.IsInGame() )
                return Resources.IconIngame;

            if ( steamid.IsOnline() )
                return Resources.IconOnline;

            return Resources.IconOffline;
        }
        Bitmap GetAvatar( string path )
        {
            try
            {
                if (path == null)
                    return Resources.IconUnknown;
                return ( Bitmap )Bitmap.FromFile( path );
            }
            catch
            {
                return Resources.IconUnknown;
            }
        }

        Bitmap ComposeAvatar( Friend steamid, string path )
        {
            Bitmap holder = GetHolder( steamid );
            Bitmap avatar = GetAvatar( path );

            Graphics gfx = null;
            try
            {
                gfx = Graphics.FromImage( holder );
                gfx.DrawImage( avatar, new Rectangle( 4, 4, avatar.Width, avatar.Height ) );
            }
            finally
            {
                gfx.Dispose();
            }

            return holder;
        }

        void FriendControl_MouseEnter( object sender, EventArgs e )
        {
            this.Highlight();
        }
        void FriendControl_MouseLeave( object sender, EventArgs e )
        {
            this.UnHighlight();
        }

        void FriendControl_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            Steam3.ChatManager.GetChat( this.Friend.SteamID );
        }

        private void btnAccept_Click( object sender, EventArgs e )
        {
            Steam3.SteamFriends.AddFriend( this.Friend.SteamID );
        }

        private void btnDeny_Click( object sender, EventArgs e )
        {
            Steam3.SteamFriends.RemoveFriend( this.Friend.SteamID );
        }

        private void removeFriendToolStripMenuItem_Click( object sender, EventArgs e )
        {
            DialogResult result = Util.MsgBox(
                this,
                string.Format( "Are you sure you wish to remove {0} from your friends list?", this.Friend.GetName() ),
                MessageBoxButtons.YesNo
            );

            if ( result != DialogResult.Yes )
                return;

            Steam3.SteamFriends.RemoveFriend( this.Friend.SteamID );
        }

        private void addFriendToolStripMenuItem_Click( object sender, EventArgs e )
        {
            MainForm mf = this.ParentForm as MainForm;
            mf.AddFriend();
        }

        private void refreshToolStripMenuItem_Click( object sender, EventArgs e )
        {
            MainForm mf = this.ParentForm as MainForm;
            mf.ReloadFriends();
        }

        private void avatarBox_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            if ( this.CanOpenProfile )
            {
                Util.OpenProfile( this.Friend.SteamID );
            }
        }

        private void viewProfileToolStripMenuItem_Click( object sender, EventArgs e )
        {
            Util.OpenProfile( this.Friend.SteamID );
        }

        
    }
}