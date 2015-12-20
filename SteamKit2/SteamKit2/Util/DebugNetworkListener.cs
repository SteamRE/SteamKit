/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.Threading;

namespace SteamKit2
{
    /// <summary>
    /// This is a debug utility, do not use it to implement your business logic.
    /// 
    /// This interface is used for logging network messages sent to and received from the Steam server that the client is connected to.
    /// </summary>
    public interface IDebugNetworkListener
    {
        /// <summary>
        /// Called when a packet is received from the Steam server.
        /// </summary>
        /// <param name="msgType">Network message type of this packet message.</param>
        /// <param name="data">Raw packet data that was received.</param>
        void OnIncomingNetworkMessage( EMsg msgType, byte[] data );

        /// <summary>
        /// Called when a packet is about to be sent to the Steam server.
        /// </summary>
        /// <param name="msgType">Network message type of this packet message.</param>
        /// <param name="data">Raw packet data that will be sent.</param>
        void OnOutgoingNetworkMessage( EMsg msgType, byte[] data );
    }

    /// <summary>
    /// Dump any network messages sent to and received from the Steam server that the client is connected to.
    /// These messages are dumped to file, and can be analyzed further with NetHookAnalyzer, a hex editor, or your own purpose-built tools.
    ///
    /// Be careful with this, sensitive data may be written to the disk (such as your Steam password).
    /// </summary>
    public class NetHookNetworkListener : IDebugNetworkListener
    {
        private long MessageNumber;
        private string LogDirectory;

        /// <summary>
        /// Will create a folder in path "%assembly%/nethook/%currenttime%/"
        /// </summary>
        public NetHookNetworkListener()
        {
            var uri = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase );

            LogDirectory = Path.Combine(
                Path.GetDirectoryName( uri.LocalPath ),
                "nethook",
                DateUtils.DateTimeToUnixTime( DateTime.Now ).ToString()
            );

            Directory.CreateDirectory( LogDirectory );

            DebugLog.WriteLine( "Created nethook directory: {0}", LogDirectory );
        }

        /// <summary>
        /// Log to your own folder.
        /// </summary>
        /// <param name="path">Path to folder.</param>
        public NetHookNetworkListener( string path )
        {
            if ( !Directory.Exists( path ) )
            {
                throw new DirectoryNotFoundException( $"{path} does not exist." );
            }

            LogDirectory = path;
        }

        /// <summary>
        /// Called when a packet is received from the Steam server.
        /// </summary>
        /// <param name="msgType">Network message type of this packet message.</param>
        /// <param name="data">Raw packet data that was received.</param>
        public void OnIncomingNetworkMessage( EMsg msgType, byte[] data )
        {
            LogNetMessage( "in", msgType, data );
        }

        /// <summary>
        /// Called when a packet is about to be sent to the Steam server.
        /// </summary>
        /// <param name="msgType">Network message type of this packet message.</param>
        /// <param name="data">Raw packet data that will be sent.</param>
        public void OnOutgoingNetworkMessage( EMsg msgType, byte[] data )
        {
            LogNetMessage( "out", msgType, data );
        }

        void LogNetMessage( string direction, EMsg msgType, byte[] data )
        {
            var path = Path.Combine( LogDirectory, GetFileName( direction, msgType ) );

            File.WriteAllBytes( path, data );
        }

        string GetFileName( string direction, EMsg msgType )
        {
            return string.Format(
                "{0:D3}_{1}_{2:D}_k_EMsg{3}.bin",
                Interlocked.Increment( ref MessageNumber ),
                direction,
                msgType,
                msgType
            );
        }
    }
}
