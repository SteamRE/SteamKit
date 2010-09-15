using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SteamKit
{
    struct UdpData
    {
        public IPEndPoint EndPoint;
        public byte[] Data;
    }

    class PacketReceivedEventArgs : EventArgs
    {
        public UdpData Packet { get; set; }

        public PacketReceivedEventArgs( UdpData pkt )
        {
            Packet = pkt;
        }
    }

    class UdpSocket
    {
        UdpClient sock;
        Thread netThread;

        bool running;

        Queue<UdpData> workQueue;

        object lockObj = new object();


        public event EventHandler<PacketReceivedEventArgs> PacketReceived;


        public UdpSocket()
        {
            workQueue = new Queue<UdpData>();

            sock = new UdpClient( 27000 );

            running = true;

            netThread = new Thread( NetFunc );
            netThread.Start();
        }


        public void Shutdown()
        {
            lock ( lockObj )
                running = false;
        }

        public void Send( byte[] data, IPEndPoint endPoint )
        {
            UdpData udp = new UdpData()
            {
                EndPoint = endPoint,
                Data = data,
            };

            Send( udp );
        }

        public void Send( UdpData pkt )
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
                Thread.Sleep( 1 );

                lock ( lockObj )
                {
                    if ( !running )
                        return;

                    if ( workQueue.Count > 0 )
                    {
                        UdpData pkt = workQueue.Dequeue();
                        sock.Send( pkt.Data, pkt.Data.Length, pkt.EndPoint );
                    }

                }

                if ( sock.Available == 0 )
                    continue;

                IPEndPoint ep = null;
                byte[] packet = null;

                try
                {
                    if ( sock.Available == 0 )
                        continue;

                    packet = sock.Receive( ref ep );
                }
                catch
                {
                    continue;
                }

                UdpData udpPkt = new UdpData()
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
