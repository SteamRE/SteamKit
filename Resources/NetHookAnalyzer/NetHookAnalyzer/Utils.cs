using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NetHookAnalyzer
{
    static class Utils
    {

        public static DialogResult MsgBox( string msg )
        {
            return MsgBox( null, msg );
        }

        public static DialogResult MsgBox( IWin32Window owner, string msg )
        {
            return MsgBox( owner, msg, MessageBoxButtons.OK );
        }
        public static DialogResult MsgBox( IWin32Window owner, string msg, MessageBoxButtons buttons )
        {
            return MsgBox( owner, msg, buttons, MessageBoxIcon.None );
        }
        public static DialogResult MsgBox( IWin32Window owner, string msg, MessageBoxButtons buttons, MessageBoxIcon icon )
        {
            return MessageBox.Show( owner, msg, "NetHookAnalyzer", buttons, icon );
        }

        public static string GetSteamDir()
        {
            string installPath = "";

            try
            {
                installPath = ( string )Registry.GetValue(
                     @"HKEY_LOCAL_MACHINE\Software\Valve\Steam",
                     "InstallPath",
                     null );
            }
            catch
            {
            }

            return installPath;
        }

    }
}
