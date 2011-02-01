using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SteamKit2;
using System.Net;
using System.Threading;

namespace Tester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();

            LoginDialog ld = new LoginDialog();

            if ( ld.ShowDialog() != DialogResult.OK )
                return;

            Application.Run( new MainForm() );
        }
    }
}
