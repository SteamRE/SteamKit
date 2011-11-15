using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.IO;
using System.Diagnostics;

namespace Vapor
{
    class FileTrace : IDebugListener
    {
        const string LogFile = "debug.log";
        private object logLock = new object();

        public FileTrace()
        {
            DebugLog.AddListener( this );

            try
            {
                lock (logLock)
                {
                    File.AppendAllText(LogFile, Environment.NewLine + Environment.NewLine);
                    File.AppendAllText(LogFile, string.Format("New log started on {0} at {1}" + Environment.NewLine, DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString()));
                }
            }
            catch { }
        }

        public void WriteLine( string msg )
        {
            try
            {
                lock (logLock)
                {
                    File.AppendAllText(LogFile, string.Format("{0}" + Environment.NewLine, msg));
                }
            }
            catch { }
        }
    }

    class ConsoleDebugListener : IDebugListener
    {
        public void WriteLine( string msg )
        {
            Console.WriteLine( msg );
            Trace.WriteLine( msg );
        }

    }
}
