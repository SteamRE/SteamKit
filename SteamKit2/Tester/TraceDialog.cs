using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using SteamKit2;

namespace Tester
{
    public partial class TraceDialog : Form, IDebugListener
    {
        object logLock = new object();

        public TraceDialog()
        {
            DebugLog.AddListener( this );
            InitializeComponent();
        }


        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            DebugLog.RemoveListener( this );

            base.OnFormClosed( e );
        }

        public void WriteLine( string msg )
        {
            lock ( logLock )
            {
                this.Invoke( new MethodInvoker( () =>
                {
                    txtTrace.AppendText( msg + Environment.NewLine );
                    txtTrace.ScrollToCaret();
                } ) );
            }
        }

    }
}