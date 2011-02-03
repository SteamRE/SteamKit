using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SteamKit2;
using System.Net;
using System.Threading;

namespace Tester
{

    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();

            TraceDialog td = new TraceDialog();
            td.Show();

            if ( new LoginDialog().ShowDialog() != DialogResult.OK )
                return;

            SteamContext.InitializeSteam3();

            MainForm mf = new MainForm();

            mf.Show();

            while ( mf.Created )
            {
                mf.UpdateCallbacks();

                Application.DoEvents();
            }

            SteamContext.ShutdownSteam3();
        }

        public static bool IsMono()
        {
            return Type.GetType( "Mono.Runtime" ) != null;
        }
    }
}
