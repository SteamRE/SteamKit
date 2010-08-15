using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Steam3Lib
{
    struct UdpDataPacket
    {
        public IPEndPoint EndPoint;
        public byte[] Data;
    }

    class PacketReceivedEventArgs : EventArgs
    {
        public UdpDataPacket Packet { get; set; }

        public PacketReceivedEventArgs( UdpDataPacket pkt )
        {
            Packet = pkt;
        }
    }

    class UdpSocket
    {
        UdpClient sock;
        Thread netThread;

        bool running;

        Queue<UdpDataPacket> workQueue;

        object lockObj = new object();


        public event EventHandler<PacketReceivedEventArgs> PacketReceived;


        public UdpSocket()
        {
            workQueue = new Queue<UdpDataPacket>();

            sock = new UdpClient( 27000 );

            running = true;

            netThread = new Thread( NetFunc );
            netThread.Start();
        }


        public void Shutdown()
        {
            Monitor.Enter( lockObj );

            running = false;

            Monitor.Exit( lockObj );
        }

        public void Send( byte[] data, IPEndPoint endPoint )
        {
            UdpDataPacket udp = new UdpDataPacket()
            {
                EndPoint = endPoint,
                Data = data,
            };

            Send( udp );
        }

        public void Send( UdpDataPacket pkt )
        {
            lock ( lockObj )
            {
                workQueue.Enqueue( pkt );
            }
        }

        void NetFunc()
        {
            while ( true )
            {
                lock ( lockObj )
                {
                    if ( !running )
                        return;

                    if ( workQueue.Count > 0 )
                    {
                        UdpDataPacket pkt = workQueue.Dequeue();
                        sock.Send( pkt.Data, pkt.Data.Length, pkt.EndPoint );

                        Console.WriteLine( "sent pkt" );
                    }

                }


                if ( sock.Available == 0 )
                    continue;


                IPEndPoint ep = null;
                byte[] packet = sock.Receive( ref ep );

                UdpDataPacket udpPkt = new UdpDataPacket()
                {
                    EndPoint = ep,
                    Data = packet,
                };

                if ( PacketReceived != null )
                    PacketReceived( this, new PacketReceivedEventArgs( udpPkt ) );
            }
        }
    }
}
