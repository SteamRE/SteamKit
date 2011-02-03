using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Tester
{

    public partial class TraceDialog : Form
    {
        TesterTraceListener listener;
        object logLock = new object();

        public TraceDialog()
        {
            listener = new TesterTraceListener( TraceFunc );
            Trace.Listeners.Add( listener );

            InitializeComponent();
        }

        void TraceFunc( object sender, TraceEventArgs e )
        {
            lock ( logLock )
            {
                this.Invoke( new MethodInvoker( () =>
                    {
                        txtTrace.AppendText( e.Message + Environment.NewLine );
                        txtTrace.ScrollToCaret();
                    } ) );
            }
        }

        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            Trace.Listeners.Remove( listener );

            base.OnFormClosed( e );
        }
    }

    class TraceEventArgs : EventArgs
    {
        public string Message { get; set; }

        public TraceEventArgs( string msg )
        {
            this.Message = msg;
        }
    }

    class TesterTraceListener : TraceListener
    {
        event EventHandler<TraceEventArgs> Trace;

        public TesterTraceListener( EventHandler<TraceEventArgs> traceFunc )
        {
            this.Trace = traceFunc;
        }

        public override void Write( string message )
        {
            Trace( this, new TraceEventArgs( message ) );
        }

        public override void WriteLine( string message )
        {
            Trace( this, new TraceEventArgs( message ) );
        }
    }
}
