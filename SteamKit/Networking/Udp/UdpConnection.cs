using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using Classless.Hasher;

namespace SteamKit
{


    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class ChallengeData : Serializable<ChallengeData>
    {
        public const uint ChallengeMask = 0xA426DF2B;

        public UInt32 ChallengeValue;
        public UInt32 ServerLoad;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class ConnectData : Serializable<ConnectData>
    {
        public const uint ChallengeMask = 0xA426DF2B;

        public UInt32 ChallengeValue;
    }


    class NetworkEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; private set; }

        public NetworkEventArgs( IPEndPoint sender )
        {
            this.Sender = sender;
        }
    }

    class ChallengeEventArgs : NetworkEventArgs
    {
        public ChallengeData Data { get; private set; }

        public ChallengeEventArgs( IPEndPoint sender, ChallengeData reply )
            : base( sender )
        {
            this.Data = reply;
        }
    }

    class NetMsgEventArgs : DataEventArgs
    {
        public EMsg Msg { get; private set; }
        public bool Proto { get; private set; }

        public NetMsgEventArgs( IPEndPoint sender, EMsg eMsg, byte[] data )
            : base( sender, data )
        {
            this.Msg = MsgUtil.GetMsg(eMsg);
            this.Proto = MsgUtil.IsProtoBuf(eMsg);
        }
    }

    class DataEventArgs : NetworkEventArgs
    {
        public byte[] Data { get; private set; }

        public DataEventArgs( IPEndPoint sender, byte[] data )
            : base( sender )
        {
            this.Data = data;
        }
    }

    class UdpConnection
    {
        UdpSocket udpSock;

        uint seqThis;
        uint seqAcked;

        uint remoteConnID;

        byte[] tempSessionKey;
        NetFilter netFilter;

        Dictionary<uint, NetPacket> packetMap;


        public event EventHandler<ChallengeEventArgs> ChallengeReceived;
        public event EventHandler<NetworkEventArgs> AcceptReceived;
        public event EventHandler<NetMsgEventArgs> NetMsgReceived;
        public event EventHandler<DataEventArgs> DatagramReceived;
        public event EventHandler<NetworkEventArgs> DisconnectReceived;


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

            data = netFilter.ProcessOutgoing( data );

            packet.SetPayload( data );

            byte[] packetData = packet.GetData();

            udpSock.Send( packetData, ipAddr );
        }

        public void SendNetMsg<MsgHdr, Hdr>( ClientMsg<MsgHdr, Hdr> clientMsg, IPEndPoint ipAddr )
            where Hdr : Serializable<Hdr>, IMsgHdr, new()
            where MsgHdr : Serializable<MsgHdr>, IClientMsg, new()
        {
            Console.WriteLine( "Sending EMsg: " + clientMsg.MsgHeader.GetEMsg() );

            SendData( clientMsg.Serialize(), ipAddr );
        }

        public void SendNetMsg<ProtoBuf>(ClientMsgProtobuf<ProtoBuf> clientMsgProto, IPEndPoint ipAddr)
            where ProtoBuf : global::ProtoBuf.IExtensible, new()
        {
            Console.WriteLine("Sending EMsg: " + clientMsgProto.Header.GetEMsg());

            SendData(clientMsgProto.Serialize(), ipAddr);
        }

        public void SendAck( IPEndPoint ipAddr )
        {
            UdpPacket packet = new UdpPacket(EUdpPacketType.Datagram);
            packet.Header.SequenceThis = 0;
            packet.Header.SequenceAcked = seqAcked;
            packet.Header.DestinationConnID = remoteConnID;

            packet.Header.PacketsInMsg = 0;
            packet.Header.MsgStartSequence = 0;

            byte[] packetData = packet.GetData();

            udpSock.Send(packetData, ipAddr);
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

            if ( AcceptReceived != null )
                AcceptReceived( this, new NetworkEventArgs( endPoint ) );
        }

        void RecvDatagram( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            if ( DatagramReceived != null )
                DatagramReceived( this, new DataEventArgs( endPoint, udpPkt.Payload ) );
        }

        void RecvDisconnect( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            if ( DisconnectReceived != null )
                DisconnectReceived( this, new NetworkEventArgs( endPoint ) );
        }

        void RecvData( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            if (udpPkt.Header.SequenceThis <= seqAcked)
            {
                Console.WriteLine("duplicate packet seq " + udpPkt.Header.SequenceThis + " " + seqAcked);
                //return;
            }

            SendAck( endPoint );

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

            try
            {
                data = netFilter.ProcessIncoming( data );
            }
            catch
            {
                return;
            }

            if ( data == null || data.Length < 4 )
                return; // we need at least an EMsg

            EMsg eMsg = ( EMsg )BitConverter.ToUInt32( data, 0 );
            this.RecvNetMsg( eMsg, data, endPoint );
        }

        void RecvNetMsg( EMsg eMsg, byte[] data, IPEndPoint endPoint )
        {
            Console.WriteLine( "Got EMsg: " + MsgUtil.GetMsg(eMsg) + " (Proto:" + MsgUtil.IsProtoBuf(eMsg) + ")" );

            if ( eMsg == EMsg.ChannelEncryptRequest )
            {
                var encRec = ClientMsg<MsgChannelEncryptRequest, MsgHdr>.GetMsgHeader( data );

                Console.WriteLine( "Got encryption request for universe: " + encRec.Universe );

                tempSessionKey = CryptoHelper.GenerateRandomBlock( 32 );

                var encResp = new ClientMsg<MsgChannelEncryptResponse, MsgHdr>();

                byte[] cryptedSessKey = CryptoHelper.RSAEncrypt( tempSessionKey, KeyManager.GetPublicKey( encRec.Universe ) );
                Crc crc = new Crc();

                byte[] keyCrc = crc.ComputeHash( cryptedSessKey );
                Array.Reverse( keyCrc );

                encResp.Write( cryptedSessKey );
                encResp.Write( keyCrc );
                encResp.Write<uint>( 0 );

                crc.Clear();

                this.SendNetMsg( encResp, endPoint );
            }

            if ( eMsg == EMsg.ChannelEncryptResult )
            {
                var encRes = ClientMsg<MsgChannelEncryptResult, MsgHdr>.GetMsgHeader( data );

                if ( encRes.Result == EResult.OK )
                    netFilter = new NetFilterEncryption( tempSessionKey );

                Console.WriteLine( "Crypto result: " + encRes.Result );
            }

            if ( eMsg == EMsg.ClientLoggedOff )
            {
                netFilter = new NetFilter( null );
            }

            if ( MsgUtil.GetMsg(eMsg) == EMsg.Multi)
            {
                if (MsgUtil.IsProtoBuf(eMsg))
                {
                    this.MultiplexMsgMulti(data, endPoint);
                }
                else
                {
                    this.MultiplexMsgMultiOld(data, endPoint);
                }
            }

            if ( NetMsgReceived != null )
                NetMsgReceived( this, new NetMsgEventArgs( endPoint, eMsg, data ) );
        }

        void MultiplexMsgMulti(byte[] data, IPEndPoint endPoint)
        {
            var msgMulti = new ClientMsgProtobuf<CMsgMulti>( data );

            byte[] payload = msgMulti.Proto.message_body;

            if ( msgMulti.Proto.size_unzipped > 0 )
            {
                try
                {
                    payload = PKZipBuffer.Decompress( payload );
                }
                catch { return; }
            }

            MultiplexPayload(payload, endPoint);
        }

        void MultiplexMsgMultiOld( byte[] data, IPEndPoint endPoint )
        {
            var msgMulti = new ClientMsg<MsgMulti, MsgHdr>( data );

            byte[] payload = msgMulti.GetPayload();

            if ( msgMulti.MsgHeader.UnzippedSize != 0 )
            {
                try
                {
                    payload = PKZipBuffer.Decompress( payload );
                }
                catch { return; }
            }

            MultiplexPayload( payload, endPoint );
        }

        private void MultiplexPayload( byte[] payload, IPEndPoint endPoint )
        {
            DataStream ds = new DataStream(payload);

            while (ds.SizeRemaining() != 0)
            {
                uint subSize = ds.ReadUInt32();
                byte[] subData = ds.ReadBytes(subSize);

                EMsg eMsg = (EMsg)BitConverter.ToUInt32(subData, 0);

                this.RecvNetMsg(eMsg, subData, endPoint);
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

                case EUdpPacketType.Datagram:
                    RecvDatagram( udpPkt, e.Packet.EndPoint );
                    break;

                case EUdpPacketType.Disconnect:
                    RecvDisconnect( udpPkt, e.Packet.EndPoint );
                    break;

                default:
                    Console.WriteLine( "Unhandled UDP packet type: " + udpPkt.Header.PacketType + "!!!" );
                    break;
            }
        }


    }
}
