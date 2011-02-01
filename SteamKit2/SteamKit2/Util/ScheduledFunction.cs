using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SteamKit2
{
    delegate void ScheduledFunc();

    class ScheduledFunction
    {
        TimeSpan delay;
        ScheduledFunc func;

        DateTime lastRun;

        Thread funcThread;
        bool running;

        object lockObj = new object();

        public ScheduledFunction( ScheduledFunc func, TimeSpan delay, string name )
        {
            this.func = func;
            this.delay = delay;

            lastRun = DateTime.MinValue;

            funcThread = new Thread( ThreadFunc );
            funcThread.Name = name;

            running = true;

            funcThread.Start();
        }
        ~ScheduledFunction()
        {
            Stop();
        }


        public void Stop()
        {
            lock ( lockObj )
            {
                running = false;
            }
        }

        void ThreadFunc()
        {
            while ( true )
            {
                lock ( lockObj )
                {
                    if ( !running )
                        return;
                }

                Thread.Sleep( 100 );

                this.CheckAndRun();
            }
        }

        void CheckAndRun()
        {
            TimeSpan diff = DateTime.Now - this.lastRun;

            if ( diff >= this.delay )
                this.Run();
        }

        void Run()
        {
            if ( func != null )
                func();

            this.lastRun = DateTime.Now;
        }
    }
}
