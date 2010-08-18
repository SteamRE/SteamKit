using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SteamLib
{
    static class FunctionScheduler
    {
        static Thread scheduleThread;
        static List<IScheduledFunction> scheduledFuncs;
        static bool running;

        public static object StaticLock = new object();


        static FunctionScheduler()
        {
            scheduledFuncs = new List<IScheduledFunction>();

            running = true;

            scheduleThread = new Thread( ScheduleThreadFunc );
            scheduleThread.Start();
        }

        public static void ScheduleFunc( IScheduledFunction func )
        {
            lock ( StaticLock )
            {
                if ( scheduledFuncs.Contains( func ) )
                    return;

                scheduledFuncs.Add( func );
            }
        }

        public static void RemoveFunc( IScheduledFunction func )
        {
            lock ( StaticLock )
            {
                if ( !scheduledFuncs.Contains( func ) )
                    return;

                scheduledFuncs.Remove( func );
            }
        }


        static void ScheduleThreadFunc()
        {
            while ( true )
            {
                Thread.Sleep( 1000 ); // scheduled funcs only offer ~1 second granularity

                lock ( StaticLock )
                {

                    if ( !running )
                        return;

                    foreach ( IScheduledFunction func in scheduledFuncs )
                        func.CheckAndRun();
                }
            }
        }
    }

    delegate void ScheduledDelegate<T>( T obj );

    interface IScheduledFunction
    {
        void CheckAndRun();
    }

    class ScheduledFunction<T> : IScheduledFunction
    {
        TimeSpan delay;

        T obj;
        ScheduledDelegate<T> func;

        DateTime lastRun;


        public ScheduledFunction()
        {
            delay = TimeSpan.MaxValue;

            FunctionScheduler.ScheduleFunc( this );
        }
        ~ScheduledFunction()
        {
            FunctionScheduler.RemoveFunc( this );
        }


        public void SetDelay( TimeSpan delay )
        {
            lock ( FunctionScheduler.StaticLock )
                this.delay = delay;
        }

        public void SetObject( T obj )
        {
            lock ( FunctionScheduler.StaticLock )
                this.obj = obj;
        }

        public void SetFunc( ScheduledDelegate<T> func )
        {
            lock ( FunctionScheduler.StaticLock )
                this.func = func;
        }


        void Run()
        {
            if ( func != null )
                func( obj );

            lastRun = DateTime.Now;
        }

        public void CheckAndRun()
        {
            // no need for locks since they've been aquired when this func is called
            // precondition: called from the FunctionScheduler

            // haven't setup the delay yet
            if ( delay == TimeSpan.MaxValue )
                return;

            TimeSpan diff = DateTime.Now - lastRun;

            if ( diff >= delay )
                Run();
        }
    }
}
