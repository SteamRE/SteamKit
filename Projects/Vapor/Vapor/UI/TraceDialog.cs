using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SteamKit2;

namespace Vapor
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
            this.Invoke( new MethodInvoker( () =>
            {
                lock ( logLock )
                {
                    txtTrace.AppendText( msg + Environment.NewLine );
                    txtTrace.ScrollToCaret();
                }
            } ) );
        }

    }
}
