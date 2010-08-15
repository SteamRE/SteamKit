using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace Steam3Lib
{


    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class ChallengeReply : Serializable<ChallengeReply>
    {
        public UInt32 ChallengeValue;
        public UInt32 ServerLoad;
    }

    public class ChallengeEventArgs : EventArgs
    {
        public ChallengeReply Data { get; private set; }
        public ChallengeEventArgs( ChallengeReply reply )
        {
            Data = reply;
        }
    }

    public class UDPConnection
    {
        static string[] CMServers =
        {
            "68.142.64.164",
            "68.142.64.165",
            "68.142.91.34",
            "68.142.91.35",
            "68.142.91.36",
            "68.142.116.178",
            "68.142.116.179",

            "69.28.145.170",
            "69.28.145.171",
            "69.28.145.172",
            "69.28.156.250",

            "72.165.61.185",
            "72.165.61.186",
            "72.165.61.187",
            "72.165.61.188",

            "208.111.133.84",
            "208.111.133.85",
            "208.111.158.52",
            "208.111.158.53",
            "208.111.171.82",
            "208.111.171.83",
        };

        UdpSocket udpSock;

        uint seqThis;
        uint seqAcked;


        public event EventHandler<ChallengeEventArgs> ChallengeReceived;


        public UDPConnection()
        {
            udpSock = new UdpSocket();

            udpSock.PacketReceived += RecvUdpPacket;
        }


        public void Connect( IPEndPoint currentServer )
        {
        }

        public void RequestChallenge( IPEndPoint ipAddr )
        {
            UdpHeader udpPkt = new UdpHeader();

            udpPkt.PacketType = EUdpPacketType.ChallengeReq;
            udpPkt.SequenceThis = 1;

            byte[] data = udpPkt.Serialize();

            udpSock.Send( data, ipAddr );
        }
        public void RequestConnect( IPEndPoint ipAddr )
        {
            /*UDPPktHdr udpPkt = new UDPPktHdr();

            udpPkt.m_EUDPPktType = EUDPPktType.Connect;*/
        }


        private void RecvChallenge( UdpHeader udpHdr, byte[] data, IPEndPoint endPoint )
        {
            int size = Marshal.SizeOf( typeof( ChallengeReply ) );

            if ( data.Length < size )
                return;

            ChallengeReply cr = ChallengeReply.Deserialize( data );

            if ( ChallengeReceived != null )
                ChallengeReceived( this, new ChallengeEventArgs( cr ) );
        }

        private void RecvUdpPacket( object sender, PacketReceivedEventArgs e )
        {
            UdpDataPacket packet = e.Packet;
            int size = packet.Data.Length;

            int headerSize = Marshal.SizeOf( typeof( UdpHeader ) );

            if ( size < headerSize )
                return;

            UdpHeader udpPkt = UdpHeader.Deserialize( packet.Data );

            byte[] payload = new byte[ udpPkt.PayloadSize ];

            Array.Copy( packet.Data, headerSize, payload, 0, payload.Length );

            switch ( udpPkt.PacketType )
            {
                case EUdpPacketType.Challenge:
                    RecvChallenge( udpPkt, payload, e.Packet.EndPoint );
                    break;

                default:
                    break; // unsupported type!!
            }
        }
    }
}
