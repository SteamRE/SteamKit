using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace SteamKit2
{
    public class NetMsgEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }

        public NetMsgEventArgs()
        {
        }
        public NetMsgEventArgs( byte[] data )
        {
            this.Data = data;
        }
    }

    public class NetFilterEncryption
    {
        byte[] sessionKey;

        public NetFilterEncryption( byte[] sessionKey )
        {
            this.sessionKey = sessionKey;
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            return CryptoHelper.SymmetricDecrypt( data, sessionKey );
        }

        public byte[] ProcessOutgoing( byte[] ms )
        {
            return CryptoHelper.SymmetricEncrypt( ms, sessionKey );
        }
    }

    public abstract class Connection
    {
        public static readonly IPEndPoint[] CMServers =
        {
            new IPEndPoint( IPAddress.Parse( "68.142.64.164" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.64.165" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.91.34" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.91.35" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.91.36" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.116.178" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.116.179" ), 27017 ),

            new IPEndPoint( IPAddress.Parse( "69.28.145.170" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "69.28.145.171" ), 27017 ),
            new IPEndPoint( IPAddress.Parse(  "69.28.145.172" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "69.28.156.250" ), 27017 ),

            new IPEndPoint( IPAddress.Parse( "72.165.61.185" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "72.165.61.186" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "72.165.61.187" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "72.165.61.188" ), 27017 ),

            new IPEndPoint( IPAddress.Parse( "208.111.133.84" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.133.85" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.158.52" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.158.53" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.171.82" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.171.83" ), 27017 ),
        };

        public NetFilterEncryption NetFilter { get; set; }


        public event EventHandler<NetMsgEventArgs> NetMsgReceived;
        protected void OnNetMsgReceived( NetMsgEventArgs e )
        {
            if ( NetMsgReceived != null )
                NetMsgReceived( this, e );
        }

        public abstract void Connect( IPEndPoint endPoint );
        public abstract void Disconnect();

        public abstract void Send( IClientMsg clientMsg );

    }

}
