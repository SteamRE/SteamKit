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

        static string RegistryPathToSteam
        {
            get { return Environment.Is64BitProcess ? @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Valve\Steam" : @"HKEY_LOCAL_MACHINE\Software\Valve\Steam"; }
        }

        public static string GetSteamDir()
        {
            string installPath = "";

            try
            {
                installPath = ( string )Registry.GetValue(
                     RegistryPathToSteam,
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
