using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SteamKit2
{
    public interface IDebugListener
    {
        void WriteLine( string msg );
    }

    public static class DebugLog
    {
        public static bool Enabled { get; set; }

        static List<IDebugListener> listeners;

        static DebugLog()
        {
            listeners = new List<IDebugListener>();
#if DEBUG
            Enabled = true;
#else
            Enabled = false;
#endif
        }

        public static void AddListener( IDebugListener listener )
        {
            listeners.Add( listener );
        }
        public static void RemoveListener( IDebugListener listener )
        {
            listeners.Remove( listener );
        }

        public static void WriteLine( string category, string msg, params object[] args )
        {
            if ( !DebugLog.Enabled )
                return;

            string strMsg = string.Format( msg, args );

            Console.WriteLine( string.Format( "{0}: {1}", category, strMsg ) );
            Trace.WriteLine( strMsg, category );

            foreach ( IDebugListener debugListener in listeners )
                debugListener.WriteLine( string.Format( "{0}: {1}", category, strMsg ) );
        }
    }
}