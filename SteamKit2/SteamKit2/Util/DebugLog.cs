/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamKit2
{
    /// <summary>
    /// Interface all debug log listeners must implement in order to register themselves.
    /// </summary>
    public interface IDebugListener
    {
        /// <summary>
        /// Called when the DebugLog wishes to inform listeners of debug spew.
        /// </summary>
        /// <param name="category">The category of the message.</param>
        /// <param name="msg">The message to log.</param>
        void WriteLine( string category, string msg );
    }

    class ActionListener : IDebugListener
    {
        public Action<string, string> Action;

        public ActionListener( Action<string, string> action )
        {
            this.Action = action;
        }

        public void WriteLine( string category, string msg )
        {
            Action( category, msg );
        }

    }

    /// <summary>
    /// Represents the root debug logging functionality. 
    /// </summary>
    public static class DebugLog
    {
        /// <summary>
        /// Gets or sets a value indicating whether debug logging is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool Enabled { get; set; }

        static List<IDebugListener> listeners = new List<IDebugListener>();


        /// <summary>
        /// Initializes the <see cref="DebugLog"/> class.
        /// </summary>
        static DebugLog()
        {
#if DEBUG
            Enabled = true;
#else
            Enabled = false;
#endif
        }

        /// <summary>
        /// Adds a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public static void AddListener( IDebugListener listener )
        {
            listeners.Add( listener );
        }
        /// <summary>
        /// Adds an action listener.
        /// </summary>
        /// <param name="listenerAction">The listener action.</param>
        public static void AddListener( Action<string, string> listenerAction )
        {
            if ( listenerAction == null )
                return;

            AddListener( new ActionListener( listenerAction ) );
        }
        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener( IDebugListener listener )
        {
            listeners.Remove( listener );
        }
        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listenerAction">The previously registered listener action.</param>
        public static void RemoveListener( Action<string, string> listenerAction )
        {
            // probably don't need this function at all, since actions are designed to be anonymous methods
            // Just In Case

            if ( listenerAction == null )
                return;

            var removals = listeners
                 .Where( list => list is ActionListener )
                 .Cast<ActionListener>()
                 .Where( actList => actList.Action == listenerAction )
                 .ToArray();

            if ( removals.Length == 0 )
                return;

            listeners.Remove( removals[ 0 ] );
        }

        /// <summary>
        /// Writes a line to the debug log, informing all listeners.
        /// </summary>
        /// <param name="category">The category of the message.</param>
        /// <param name="msg">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more objects to format.</param>
        public static void WriteLine( string category, string msg, params object[] args )
        {
            if ( !DebugLog.Enabled )
                return;

            string strMsg = string.Format( msg, args );

            foreach ( IDebugListener debugListener in listeners )
            {
                debugListener.WriteLine( category, strMsg );
            }
        }
    }
}