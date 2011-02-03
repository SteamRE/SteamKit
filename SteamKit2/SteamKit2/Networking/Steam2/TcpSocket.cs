using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace SteamKit2
{
    public class TcpSocket
    {
        Socket sock;

        NetworkStream sockStream;

        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }

        public TcpSocket()
        {
        }


        public void Connect( IPEndPoint endPoint )
        {
            Disconnect();

            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            sock.Connect( endPoint );

            sockStream = new NetworkStream( sock, true );

            Reader = new BinaryReader( sockStream );
            Writer = new BinaryWriter( sockStream );
        }

        public void Disconnect()
        {
            // mono doesn't like calling Shutdown if we're not connected
            if ( sock == null || !sock.Connected )
                return;

            if ( sock != null )
            {
                sock.Shutdown( SocketShutdown.Both );
                sock.Disconnect( true );
                sock.Close();

                sock = null;
            }
        }

        public void Send( byte[] data )
        {
            sock.Send( data );
        }

        public void Send( TcpPacket packet )
        {
            this.Send( packet.GetData() );
        }

        public TcpPacket ReceivePacket()
        {
            TcpPacket pack = new TcpPacket();

            uint size = NetHelpers.EndianSwap( this.Reader.ReadUInt32() );
            byte[] payload = Reader.ReadBytes( ( int )size );

            pack.SetPayload( payload );

            return pack;
        }
    }
}
