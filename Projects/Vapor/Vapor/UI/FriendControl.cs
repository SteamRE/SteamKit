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
using SteamKit2;

namespace Vapor
{
    partial class FriendControl : UserControl, ICallbackHandler
    {

        public Friend Friend { get; private set; }

        bool highlighted;


        public FriendControl()
        {
            InitializeComponent();

            Steam3.AddHandler( this );

            this.MouseEnter += FriendControl_MouseEnter;
            this.MouseLeave += FriendControl_MouseLeave;

            foreach ( Control ctrl in this.Controls )
            {
                ctrl.MouseEnter += FriendControl_MouseEnter;
                ctrl.MouseLeave += FriendControl_MouseLeave;
            }

        }
        public FriendControl( Friend steamid )
            : this()
        {
            SetSteamID( steamid );
        }


        public void HandleCallback( CallbackMsg msg )
        {
            if ( msg is PersonaStateCallback )
            {
                var perState = ( PersonaStateCallback )msg;

                if ( perState.FriendID == this.Friend.SteamID )
                    this.SetSteamID( this.Friend );
            }
        }

        public void SetSteamID( Friend steamid )
        {
            Friend = steamid;

            nameLbl.Text = steamid.GetName();
            statusLbl.Text = steamid.GetStatus();
            gameLbl.Text = steamid.GetGameName();

            nameLbl.ForeColor = statusLbl.ForeColor = gameLbl.ForeColor = Util.GetStatusColor( steamid );

            avatarBox.Image = ComposeAvatar( steamid );
        }


        void Highlight()
        {
            if ( this.Friend == null || this.Friend.SteamID == Steam3.SteamUser.GetSteamID() )
                return;

            if ( this.highlighted )
                return;

            this.BackColor = Color.FromArgb( 38, 38, 39 );

            highlighted = true;
        }
        void UnHighlight()
        {
            if ( this.Friend.SteamID == Steam3.SteamUser.GetSteamID() )
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
        Bitmap GetAvatar( Friend steamid )
        {
            GCHandle imgHandle = new GCHandle();

            /*
            try
            {
                int avatarId = SteamContext.ClientFriends.GetSmallFriendAvatar( steamid.SteamID );

                if ( avatarId == 0 )
                    return Resources.IconUnknown;

                int width = 32, height = 32;
                byte[] imgData = new byte[ 4 * width * height ];

                if ( !SteamContext.SteamUtils.GetImageRGBA( avatarId, imgData, imgData.Length ) )
                    return Resources.IconUnknown;

                // imgData is in RGBA format, .NET expects BGRA
                // so lets prep the data by swapping R and B
                for ( int x = 0 ; x < imgData.Length ; x += 4 )
                {
                    byte r = imgData[ x ];
                    byte b = imgData[ x + 2 ];

                    imgData[ x ] = b;
                    imgData[ x + 2 ] = r;
                }

                imgHandle = GCHandle.Alloc( imgData, GCHandleType.Pinned );

                return new Bitmap( width, height, 4 * width, PixelFormat.Format32bppArgb, imgHandle.AddrOfPinnedObject() );
            }
            catch
            {
                return Resources.IconUnknown;
            }
            finally
            {
                imgHandle.Free();
            }*/

            return Resources.IconUnknown;
        }

        Bitmap ComposeAvatar( Friend steamid )
        {
            Bitmap holder = GetHolder( steamid );
            Bitmap avatar = GetAvatar( steamid );

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

        private void FriendControl_MouseEnter( object sender, EventArgs e )
        {
            this.Highlight();
        }
        private void FriendControl_MouseLeave( object sender, EventArgs e )
        {
            this.UnHighlight();
        }
    }
}