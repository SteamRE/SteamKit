using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SteamKit2;
using System.Diagnostics;

namespace Tester
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
        }

        public void UpdateCallbacks()
        {
            CallbackMsg callback = SteamContext.SteamClient.GetCallback();

            if ( callback == null )
                return;

            SteamContext.SteamClient.FreeLastCallback();

            if ( callback is ConnectedCallback )
            {
                SteamContext.SteamUser.LogOn( SteamContext.LoginDetails );
                Trace.WriteLine( "Connection established.", "Tester" );
            }
            if ( callback is LogOnResponseCallback )
            {
            }
        }
    }
}
