using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Vapor
{
    static class Util
    {
        public static DialogResult MsgBox( IWin32Window parent, string msg, MessageBoxButtons btns )
        {
            return MessageBox.Show( parent, msg, "Vapor", btns );
        }
        public static DialogResult MsgBox( IWin32Window parent, string msg )
        {
            return MsgBox( parent, msg, MessageBoxButtons.OK );
        }
        public static DialogResult MsgBox( string msg )
        {
            return MsgBox( null, msg );
        }

        public static Color GetStatusColor( Friend steamid )
        {
            Color inGame = Color.FromArgb( 177, 251, 80 );
            Color online = Color.FromArgb( 111, 189, 255 );
            Color offline = Color.FromArgb( 137, 137, 137 );
            Color blocked = Color.FromArgb( 251, 80, 80 );
            Color invited = Color.FromArgb( 250, 218, 94 );
            Color requesting = Color.FromArgb( 135, 169, 107 );

            if ( steamid.IsAcceptingFriendship() )
                return invited;

            if ( steamid.IsRequestingFriendship() )
                return requesting;

            if ( steamid.IsBlocked() )
                return blocked;

            if ( steamid.IsInGame() )
                return inGame;

            if ( !steamid.IsOnline() )
                return offline;

            return online;
        }

        public static bool IsZeros( byte[] bytes )
        {
            for ( int i = 0 ; i < bytes.Length ; i++ )
            {
                if ( bytes[ i ] != 0 )
                    return false;
            }
            return true;
        }

    }
}
