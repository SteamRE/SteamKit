using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using SteamKit2;
using System.Diagnostics;

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

        public static void OpenProfile( SteamID steamId )
        {
            string friendUrl = string.Format( "http://www.steamcommunity.com/profiles/{0}", steamId.ConvertToUint64() );

            if ( Util.IsMono() )
            {
                Process.Start( string.Format( "xdg-open {0:s}", friendUrl ) );
            }
            else
            {
                Process.Start( friendUrl );
            }
        }

        public const UInt32 FLASHW_STOP = 0;
        public const UInt32 FLASHW_CAPTION = 1;
        public const UInt32 FLASHW_TRAY = 2;
        public const UInt32 FLASHW_ALL = 3;
        public const UInt32 FLASHW_TIMER = 4;
        public const UInt32 FLASHW_TIMERNOFG = 12; 

        [StructLayout( LayoutKind.Sequential )]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        [DllImport( "user32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        static extern bool FlashWindowEx( ref FLASHWINFO pwfi );


        public static void FlashWindow( Form wnd )
        {
            if ( IsMono() )
            {
                // welp, sorry folks!
                return;
            }

            FLASHWINFO flashInfo = new FLASHWINFO();

            flashInfo.cbSize = ( uint )Marshal.SizeOf( flashInfo );
            flashInfo.dwFlags = FLASHW_TIMERNOFG | FLASHW_TRAY;
            flashInfo.hwnd = wnd.Handle;

            FlashWindowEx( ref flashInfo );
        }

        public static bool IsMono()
        {
            return ( Type.GetType( "Mono.Runtime" ) != null );
        }

    }
}
