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
    public class ChallengeData : Serializable<ChallengeData>
    {
        public UInt32 ChallengeValue;
        public UInt32 ServerLoad;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class ConnectData : Serializable<ConnectData>
    {
        public UInt32 ChallengeValue;
    }


    public class NetworkEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; private set; }

        public NetworkEventArgs( IPEndPoint sender )
        {
            this.Sender = sender;
        }
    }

    public class ChallengeEventArgs : NetworkEventArgs
    {
        public ChallengeData Data { get; private set; }

        public ChallengeEventArgs( IPEndPoint sender, ChallengeData reply )
            : base( sender )
        {
            this.Data = reply;
        }
    }

    public class DataEventArgs : NetworkEventArgs
    {
        public byte[] Data { get; private set; }

        public DataEventArgs( IPEndPoint sender, byte[] data )
            : base( sender )
        {
            this.Data = data;
        }
    }

    public class UdpConnection
    {
        public static string[] CMServers =
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
        public event EventHandler<NetworkEventArgs> AcceptReceived;
        public event EventHandler<DataEventArgs> DataReceived;


        public UdpConnection()
        {
            udpSock = new UdpSocket();

            seqThis = 0;
            seqAcked = 0;

            udpSock.PacketReceived += RecvUdpPacket;
        }


        public void SendChallengeReq( IPEndPoint ipAddr )
        {
            UdpPacket packet = new UdpPacket( EUdpPacketType.ChallengeReq );
            packet.Header.SequenceThis = 1;
            packet.Header.SequenceAcked = seqAcked;

            byte[] data = packet.GetData();

            udpSock.Send( data, ipAddr );
        }

        public void SendConnect( UInt32 challengeValue, IPEndPoint ipAddr )
        {
            seqThis++;

            UdpPacket packet = new UdpPacket( EUdpPacketType.Connect );
            packet.Header.SequenceThis = seqThis;
            packet.Header.SequenceAcked = seqAcked;

            packet.Header.PacketsInMsg = 1;
            packet.Header.MsgStartSequence = seqThis;

            ConnectData cd = new ConnectData();
            cd.ChallengeValue = challengeValue ^ 0xA426DF2B; // challenge obfuscation mask

            packet.SetPayload( cd.Serialize() );

            byte[] data = packet.GetData();

            udpSock.Send( data, ipAddr );
        }


        void RecvChallenge( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            int size = Marshal.SizeOf( typeof( ChallengeData ) );

            if ( udpPkt.Payload.Length < size )
                return;

            ChallengeData cr = ChallengeData.Deserialize( udpPkt.Payload );

            if ( ChallengeReceived != null )
                ChallengeReceived( this, new ChallengeEventArgs( endPoint, cr ) );
        }

        void RecvAccept( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            if ( AcceptReceived != null )
                AcceptReceived( this, new NetworkEventArgs( endPoint ) );
        }

        void RecvData( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            if ( DataReceived != null )
                DataReceived( this, new DataEventArgs( endPoint, udpPkt.Payload ) );
        }

        void RecvUdpPacket( object sender, PacketReceivedEventArgs e )
        {
            UdpPacket udpPkt = new UdpPacket( e.Packet.Data );

            if ( !udpPkt.IsValid )
                return;

            seqAcked = udpPkt.Header.SequenceThis;

            switch ( udpPkt.Header.PacketType )
            {
                case EUdpPacketType.Challenge:
                    RecvChallenge( udpPkt, e.Packet.EndPoint );
                    break;

                case EUdpPacketType.Accept:
                    RecvAccept( udpPkt, e.Packet.EndPoint );
                    break;

                case EUdpPacketType.Data:
                    RecvData( udpPkt, e.Packet.EndPoint );
                    break;

                default:
                    break; // unsupported type!!
            }
        }


    }
}
