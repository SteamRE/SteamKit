using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SteamLib
{

    delegate bool AsyncCall( ref SteamProgress progress, out SteamError err );

    public class SteamProgress
    {
        public string Description { get; internal set; }
    }

    public class SteamCallHandle
    {

        Thread CallThread { get; set; }
        internal List<AsyncCall> FuncCalls { get; private set; }
        int CallsMade { get; set; }
        AutoResetEvent Trigger { get; set; }

        SteamError lastError;
        SteamProgress progress;

        public static SteamCallHandle InvalidHandle
        {
            get { return null; }
        }


        internal SteamCallHandle()
        {
            this.lastError = new SteamError();
            this.progress = new SteamProgress();

            this.FuncCalls = new List<AsyncCall>();
            this.CallsMade = 0;

            this.CallThread = new Thread( ThreadFunc );
            this.Trigger = new AutoResetEvent( false );
        }

        internal void Start()
        {
            this.CallThread.Start();
        }

        internal void ThreadFunc( object param )
        {
            foreach ( AsyncCall call in FuncCalls )
            {
                if ( !Trigger.WaitOne( TimeSpan.FromSeconds( 5 ) ) )
                {
                    lastError = new SteamError( ESteamErrorCode.CallTimedOut );
                    return;
                }

                bool result = call( ref progress, out lastError );

                CallsMade++;

                if ( !result )
                    return;
            }
        }

        internal bool Process( ref SteamProgress progress, out SteamError err )
        {
            err = this.lastError;
            progress = this.progress;

            if ( err.IsError() || this.CallsMade == this.FuncCalls.Count )
                return true;

            Trigger.Set();

            return false;
        }

        internal virtual object GetCompletionData()
        {
            return null;
        }
    }

}
