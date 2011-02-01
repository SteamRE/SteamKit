using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace SteamKit2
{
    public class TcpConnection : Connection
    {
        static readonly UInt32 Magic = 0x31305456;

        Socket sock;
        byte[] recvBuffer = new byte[ 1024 * 15 ];


        public TcpConnection()
        {
            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        }


        public override void Connect( IPEndPoint endPoint )
        {
            sock.Connect( endPoint );
            StartRecv();
        }

        public override void Disconnect()
        {
            sock.Shutdown( SocketShutdown.Both );
            sock.Disconnect( true );
            sock.Close();
        }

        public override void Send( IClientMsg clientMsg )
        {
            byte[] data = clientMsg.Serialize();

            if ( NetFilter != null )
            {
                data = NetFilter.ProcessOutgoing( data );
            }

            ByteBuffer bb = new ByteBuffer( data.Length + 8 );
            bb.Append( ( uint )data.Length );
            bb.Append( TcpConnection.Magic );
            bb.Append( data );


            sock.Send( bb.ToArray() );
        }


        void StartRecv()
        {
            Array.Clear( recvBuffer, 0, recvBuffer.Length );

            SocketAsyncEventArgs sockArgs = new SocketAsyncEventArgs();
            sockArgs.SetBuffer( recvBuffer, 0, recvBuffer.Length );
            sockArgs.Completed += RecvCompleted;

            bool bCompleted = sock.ReceiveAsync( sockArgs );

            if ( !bCompleted )
            {
                RecvCompleted( null, sockArgs );
            }
        }

        void RecvCompleted( object sender, SocketAsyncEventArgs e )
        {
            DataStream ds = new DataStream( e.Buffer );

            uint len = ds.ReadUInt32();
            uint magic = ds.ReadUInt32();

            if ( magic != TcpConnection.Magic )
            {
#if DEBUG
                Trace.WriteLine( "TcpConnection RecvCompleted got a packet with invalid magic!", "Steam3" );
#endif

                return;
            }

            byte[] packetData = ds.ReadBytes( len );

            if ( NetFilter != null )
            {
                packetData = NetFilter.ProcessIncoming( packetData );
            }

            OnNetMsgReceived( new NetMsgEventArgs( packetData ) );
            StartRecv();
        }
    }
}
