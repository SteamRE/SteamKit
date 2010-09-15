using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SteamKit
{

    delegate bool AsyncCall( ref SteamProgress progress, out SteamError err );

    public class SteamProgress
    {
        public string Description { get; internal set; }

        public bool IsValid() { return !string.IsNullOrEmpty( Description ); }
    }

    public class SteamCallHandle
    {

        Thread CallThread { get; set; }
        internal List<AsyncCall> FuncCalls { get; private set; }
        int CallsMade { get; set; }
        AutoResetEvent Trigger { get; set; }

        SteamError lastError;
        SteamProgress progress;

        object lockObj = new object();


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
#if DEBUG
                TimeSpan timeOut = TimeSpan.FromMilliseconds( -1 ); // calls never timeout in debug builds
#else
                TimeSpan timeOut = TimeSpan.FromSeconds( 5 );
#endif

                if ( !Trigger.WaitOne( timeOut ) )
                {
                    lock ( lockObj )
                    {
                        lastError = new SteamError( ESteamErrorCode.CallTimedOut );
                    }

                    return;
                }

                lock ( lockObj )
                {
                    bool result = call( ref progress, out lastError );

                    CallsMade++;

                    if ( !result )
                        return;
                }
            }
        }

        public bool Process( ref SteamProgress progress, out SteamError err )
        {
            lock ( lockObj )
            {
                err = this.lastError;
                progress = this.progress;


                if ( err.IsError() || this.CallsMade == this.FuncCalls.Count )
                    return true;
            }

            Trigger.Set();

            return false;
        }
    }

}
