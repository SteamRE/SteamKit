using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SteamKit2
{
    
    class TcpConnection : Connection
    {
        const uint MAGIC = 0x31305456;

        Socket sock;
        MemoryStream recvBuffer;

        Thread netThread;
        bool bConnected;

        int sizeLeft;


        public TcpConnection()
        {
            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

            recvBuffer = new MemoryStream();

            bConnected = false;


        }


        public override void Connect( IPEndPoint endPoint )
        {
            sock.Connect( endPoint );

            bConnected = true;

            netThread = new Thread( NetLoop );
            netThread.Start();
        }

        public override void Disconnect()
        {
            if ( !sock.Connected )
                return;

            sock.Shutdown( SocketShutdown.Both );
            sock.Disconnect( true );
            sock.Close();

            lock ( ConnLock )
            {
                bConnected = false;
            }

            netThread.Join();
        }

        public override void Send( IClientMsg clientMsg )
        {
            if ( !sock.Connected )
                return;

            byte[] data = clientMsg.Serialize();

            if ( NetFilter != null )
            {
                data = NetFilter.ProcessOutgoing( data );
            }

            ByteBuffer bb = new ByteBuffer( data.Length + 8 );
            bb.Append( ( uint )data.Length );
            bb.Append( TcpConnection.MAGIC );
            bb.Append( data );

            sock.Send( bb.ToArray() );
        }

        void NetLoop()
        {
            while ( true )
            {
                // todo: determine if we have any real performance issues here
                Thread.Sleep( 10 );

                lock ( ConnLock )
                {
                    if ( !bConnected )
                        return;
                }

                if ( sock.Available == 0 )
                    continue; // we don't want to stay blocked incase the thread needs to shutdown
                
                if ( sizeLeft == 0 )
                {
                    // new packet
                    recvBuffer.SetLength( 0 );

                    byte[] packetHeader = new byte[ 8 ];
                    int headerLen = sock.Receive( packetHeader );

                    if ( headerLen != 8 )
                    {
                        DebugLog.WriteLine( "TcpConnection", "Recv'd truncated packet header!!" );
                        continue;
                    }

                    DataStream ds = new DataStream( packetHeader );

                    uint packetLen = ds.ReadUInt32();
                    uint packetMagic = ds.ReadUInt32();

                    if ( packetMagic != TcpConnection.MAGIC )
                    {
                        DebugLog.WriteLine( "TcpConnection", "RecvCompleted got a packet with invalid magic!" );
                        return;
                    }

                    sizeLeft = ( int )packetLen;
                }

                // continuing packet
                int readSize = Math.Min( 1024, sizeLeft );
                byte[] packetChunk = new byte[ readSize ];

                int numRead = sock.Receive( packetChunk );

                recvBuffer.Write( packetChunk, 0, numRead );
                sizeLeft -= numRead;

                if ( sizeLeft == 0 )
                {
                    // packet completed
                    byte[] packData = recvBuffer.ToArray();

                    if ( NetFilter != null )
                    {
                        packData = NetFilter.ProcessIncoming( packData );
                    }

                    OnNetMsgReceived( new NetMsgEventArgs( packData ) );
                }

            }
        }
    }
}
