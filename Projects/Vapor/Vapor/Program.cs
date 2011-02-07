using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Vapor
{
    class Program
    {
        public static void Main()
        {
            //Application.EnableVisualStyles();

            LoginDialog ld = new LoginDialog();

            if ( ld.ShowDialog() != DialogResult.OK )
                return;

            MainForm mf = new MainForm();
            mf.Show();

            while ( mf.Created )
            {
                Steam3.Update();
                Application.DoEvents();

                Thread.Sleep( 1 ); // sue me, AzuiSleet.
            }

            Steam3.Shutdown();
        }
    }
}
