using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

namespace SteamKit
{

    public class TcpPacket : ByteBuffer
    {
        public TcpPacket()
            : base( true )
        {
        }


        public byte[] GetPayload()
        {
            return this.ToArray();
        }

        public void SetPayload( byte[] payload )
        {
            this.Clear();
            this.Append( payload );
        }


        public byte[] GetData()
        {
            ByteBuffer bb = new ByteBuffer( true );

            byte[] payload = this.GetPayload();

            bb.Append( ( uint )payload.Length );
            bb.Append( payload );

            return bb.ToArray();
        }
    }

    public class TcpSocket
    {
        Socket sock;

        NetworkStream netStream;

        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }


        public TcpSocket()
        {
        }


        public void Connect( IPEndPoint endPoint )
        {
            try
            {
                Disconnect();
            }
            catch { }

            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            sock.LingerState.Enabled = false;
            sock.ReceiveTimeout = 250;

            IAsyncResult ar = sock.BeginConnect( endPoint, null, null );
            WaitHandle waitHandle = ar.AsyncWaitHandle;

            try
            {
                if ( !waitHandle.WaitOne( TimeSpan.FromMilliseconds( 2000 ), false ) )
                {
                    Disconnect();
                }

                sock.EndConnect( ar );
            }
            catch { }

            //sock.Connect( endPoint );

            netStream = new NetworkStream( sock, true );

            Reader = new BinaryReader( netStream );
            Writer = new BinaryWriter( netStream );
        }

        public void Send( TcpPacket packet )
        {
            Writer.Write( packet.GetData() );
        }

        public TcpPacket Receive()
        {
            TcpPacket pack = new TcpPacket();

            uint size = NetHelpers.EndianSwap( this.Reader.ReadUInt32() );//( uint )IPAddress.NetworkToHostOrder( Reader.ReadInt32() );
            byte[] payload = Reader.ReadBytes( ( int )size );

            pack.SetPayload( payload );

            return pack;
        }

        public void Disconnect()
        {
            if (sock != null)
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Disconnect(true);
                sock.Close();

                sock = null;
            }

            if (netStream != null)
            {
                netStream.Close();

                netStream = null;
            }
        }

    }
}