/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Threading;

namespace SteamKit2
{
    class ScheduledFunction
    {
        public TimeSpan Delay { get; set; }

        Action func;

        bool bStarted;
        Timer timer;

        public ScheduledFunction( Action func )
            : this( func, Timeout.InfiniteTimeSpan )
        {
        }

        public ScheduledFunction( Action func, TimeSpan delay )
        {
            this.func = func;
            this.Delay = delay;

            timer = new Timer( Tick, null, Timeout.InfiniteTimeSpan, delay );
        }
        ~ScheduledFunction()
        {
            Stop();
        }


        public void Start()
        {
            if ( bStarted )
                return;

            bStarted = timer.Change( TimeSpan.Zero, Delay );
        }

        public void Stop()
        {
            if ( !bStarted )
                return;

            bStarted = !timer.Change( Timeout.InfiniteTimeSpan, Delay );
        }


        void Tick( object? state )
        {
            func?.Invoke();
        }
    }
}
