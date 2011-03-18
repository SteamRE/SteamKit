using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace CDRUpdater
{
    static class DebugLog
    {
        public const string Filename = "debug.log";

        static string SessionFile;

        static DebugLog()
        {
            SessionFile = Path.Combine( Application.StartupPath, Filename );
        }

        public static void Write( string format, params object[] args )
        {
            try
            {
                string line = string.Format( "[ {0} ] {1}", DateTime.Now.ToShortTimeString(), string.Format( format, args ) );
                File.AppendAllText( SessionFile, line );
                Console.Write( line );
            }
            catch { }
        }
    }
}
