using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace SteamLib
{


    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class ChallengeData : Serializable<ChallengeData>
    {
        public const uint ChallengeMask = 0xA426DF2B;

        public UInt32 ChallengeValue;
        public UInt32 ServerLoad;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class ConnectData : Serializable<ConnectData>
    {
        public const uint ChallengeMask = 0xA426DF2B;

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

        uint remoteConnID;

        NetFilter netFilter;

        Dictionary<uint, NetPacket> packetMap;


        public event EventHandler<ChallengeEventArgs> ChallengeReceived;
        public event EventHandler<NetworkEventArgs> AcceptReceived;
        public event EventHandler<DataEventArgs> DataReceived;


        public UdpConnection()
        {
            packetMap = new Dictionary<uint, NetPacket>();

            udpSock = new UdpSocket();

            seqThis = 0;
            seqAcked = 0;

            remoteConnID = 0;

            netFilter = new NetFilter( null );

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
            packet.Header.DestinationConnID = remoteConnID;

            packet.Header.PacketsInMsg = 1;
            packet.Header.MsgStartSequence = seqThis;

            ConnectData cd = new ConnectData();
            cd.ChallengeValue = challengeValue ^ ConnectData.ChallengeMask;

            packet.SetPayload( cd.Serialize() );

            byte[] data = packet.GetData();

            udpSock.Send( data, ipAddr );
        }

        public void SendData( byte[] data, IPEndPoint ipAddr )
        {
            seqThis++;

            UdpPacket packet = new UdpPacket( EUdpPacketType.Data );
            packet.Header.SequenceThis = seqThis;
            packet.Header.SequenceAcked = seqAcked;
            packet.Header.DestinationConnID = remoteConnID;

            packet.Header.PacketsInMsg = 1;
            packet.Header.MsgStartSequence = seqThis;

            packet.SetPayload( data );

            byte[] packetData = packet.GetData();

            udpSock.Send( packetData, ipAddr );
        }

        public void SendNetMsg<MsgHdr, Hdr>( ClientMsg<MsgHdr, Hdr> clientMsg, IPEndPoint ipAddr )
            where Hdr : Serializable<Hdr>, IMsg, new()
            where MsgHdr : Serializable<MsgHdr>, IClientMsg, new()
        {
            SendData( clientMsg.GetData(), ipAddr );
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
            remoteConnID = udpPkt.Header.SourceConnID;

            var logon = new ClientMsg<MsgClientAnonLogOn, ExtendedClientMsgHdr>();

            logon.Write( new byte[ 19 ] );

            SendNetMsg( logon, endPoint );

            if ( AcceptReceived != null )
                AcceptReceived( this, new NetworkEventArgs( endPoint ) );
        }

        void RecvData( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            if ( DataReceived != null )
                DataReceived( this, new DataEventArgs( endPoint, udpPkt.Payload ) );

            NetPacket netPacket = null;

            if ( packetMap.ContainsKey( udpPkt.Header.MsgStartSequence ) )
                netPacket = packetMap[ udpPkt.Header.MsgStartSequence ];
            else
            {
                netPacket = new NetPacket( udpPkt.Header.MsgStartSequence, udpPkt.Header.PacketsInMsg );

                packetMap.Add( udpPkt.Header.MsgStartSequence, netPacket );
            }

            netPacket.AddData( udpPkt.Header, udpPkt.Payload );

            if ( netPacket.IsCompleted )
                this.RecvNetPacket( udpPkt.Header, netPacket, endPoint );
        }

        void RecvNetPacket( UdpHeader udpHdr, NetPacket netPacket, IPEndPoint endPoint )
        {
            byte[] data = netPacket.GetData();

            packetMap.Remove( udpHdr.MsgStartSequence );

            data = netFilter.ProcessIncoming( data );

            if ( data.Length < 4 )
                return;

            EMsg eMsg = ( EMsg )BitConverter.ToUInt32( data, 0 );
            this.RecvNetMsg( eMsg, data, endPoint );
        }

        void RecvNetMsg( EMsg eMsg, byte[] data, IPEndPoint endPoint )
        {
            Console.WriteLine( "Got EMsg: " + eMsg );

            if ( eMsg == EMsg.ChannelEncryptRequest )
            {
                var encRec = ClientMsg<MsgChannelEncryptRequest, MsgHdr>.GetMsgHeader( data );

                Console.WriteLine( "Got encryption request for universe: " + encRec.Universe );

                byte[] sessionKey = CryptoHelper.GenerateRandomBlock( 32 );
                netFilter = new NetFilterEncryption( sessionKey );

                var encResp = new ClientMsg<MsgChannelEncryptResponse, MsgHdr>();

                byte[] cryptedSessKey = CryptoHelper.RSAEncrypt( sessionKey, KeyManager.GetPublicKey( encRec.Universe ) );
                byte[] crc = BitConverter.GetBytes( Crc32.Compute( cryptedSessKey ) );
                byte[] unk = new byte[ 4 ];

                ByteBuffer bb = new ByteBuffer();
                bb.Append( cryptedSessKey );
                bb.Append( crc );
                bb.Append( unk );

                encResp.SetPayload( bb.ToArray() );

                SendNetMsg( encResp, endPoint );
            }

            if ( eMsg == EMsg.ChannelEncryptResult )
            {
                var encRes = ClientMsg<MsgChannelEncryptResult, MsgHdr>.GetMsgHeader( data );

                Console.WriteLine( "Crypto result: " + encRes.Result );
            }

            if ( eMsg == EMsg.ClientLogOnResponse )
            {
                var logonResp = new ClientMsg<MsgClientLogOnResponse, ExtendedClientMsgHdr>( data );
            }
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
