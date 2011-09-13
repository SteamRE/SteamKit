/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SteamKit3
{
    /// <summary>
    /// Interface all debug log listeners must implement in order to register themselves.
    /// </summary>
    public interface IDebugListener
    {
        /// <summary>
        /// Called when the DebugLog wishes to inform listeners of debug spew.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        void WriteLine( string msg );
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

        static List<IDebugListener> listeners;


        /// <summary>
        /// Initializes the <see cref="DebugLog"/> class.
        /// </summary>
        static DebugLog()
        {
            listeners = new List<IDebugListener>();
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
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener( IDebugListener listener )
        {
            listeners.Remove( listener );
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
                debugListener.WriteLine( string.Format( "{0}: {1}", category, strMsg ) );
            }
        }
    }
}