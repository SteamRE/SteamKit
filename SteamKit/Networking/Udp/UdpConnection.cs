using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using Classless.Hasher;

namespace SteamKit
{
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

        public NetMsgEventArgs( IPEndPoint sender, EMsg eMsg, MemoryStream data )
            : base( sender, data )
        {
            this.Msg = MsgUtil.GetMsg(eMsg);
            this.Proto = MsgUtil.IsProtoBuf(eMsg);
        }
    }

    class DataEventArgs : NetworkEventArgs
    {
        public MemoryStream Data { get; private set; }

        public DataEventArgs( IPEndPoint sender, MemoryStream data )
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
            packet.Header.SeqThis = 1;
            packet.Header.SeqAck = seqAcked;

            byte[] data = packet.GetData().ToArray();

            udpSock.Send( data, ipAddr );
        }

        public void SendConnect( UInt32 challengeValue, IPEndPoint ipAddr )
        {
            seqThis++;

            UdpPacket packet = new UdpPacket( EUdpPacketType.Connect );
            packet.Header.SeqThis = seqThis;
            packet.Header.SeqAck = seqAcked;
            packet.Header.DestConnID = remoteConnID;

            packet.Header.PacketsInMsg = 1;
            packet.Header.MsgStartSeq = seqThis;

            ConnectData cd = new ConnectData();
            cd.ChallengeValue = challengeValue ^ ConnectData.CHALLENGE_MASK;

            packet.SetPayload( cd.serialize() );

            byte[] data = packet.GetData().ToArray();

            udpSock.Send( data, ipAddr );
        }

        public void SendData( byte[] data, IPEndPoint ipAddr )
        {
            seqThis++;

            UdpPacket packet = new UdpPacket( EUdpPacketType.Data );
            packet.Header.SeqThis= seqThis;
            packet.Header.SeqAck = seqAcked;
            packet.Header.DestConnID = remoteConnID;

            packet.Header.PacketsInMsg = 1;
            packet.Header.MsgStartSeq = seqThis;

            data = netFilter.ProcessOutgoing( data );

            packet.SetPayload( new MemoryStream( data ) );

            byte[] packetData = packet.GetData().ToArray();

            udpSock.Send( packetData, ipAddr );
        }

        public void SendNetMsg( IClientMsg clientMsg, IPEndPoint endPoint )
        {
            SendData( clientMsg.serialize().ToArray(), endPoint );
        }

        public void SendAck(IPEndPoint ipAddr)
        {
            UdpPacket packet = new UdpPacket(EUdpPacketType.Datagram);
            packet.Header.SeqThis = 0;
            packet.Header.SeqAck = seqAcked;
            packet.Header.DestConnID = remoteConnID;

            packet.Header.PacketsInMsg = 0;
            packet.Header.MsgStartSeq = 0;

            byte[] packetData = packet.GetData().ToArray();

            udpSock.Send(packetData, ipAddr);
        }

        void RecvChallenge( UdpPacket udpPkt, IPEndPoint endPoint )
        {
            ChallengeData cr = new ChallengeData();
            cr.deserialize( udpPkt.Payload );

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
            uint curSeq = seqThis;

            NetPacket netPacket = null;

            if ( packetMap.ContainsKey( udpPkt.Header.MsgStartSeq ) )
                netPacket = packetMap[ udpPkt.Header.MsgStartSeq ];
            else
            {
                netPacket = new NetPacket(udpPkt.Header.MsgStartSeq, udpPkt.Header.PacketsInMsg);

                packetMap.Add(udpPkt.Header.MsgStartSeq, netPacket);
            }

            netPacket.AddData( udpPkt.Header, udpPkt.Payload );

            if ( netPacket.IsCompleted )
                this.RecvNetPacket( udpPkt.Header, netPacket, endPoint );

            // send a datagram ack if we didn't do anything
            if (seqThis == curSeq)
            {
                SendAck(endPoint);
            }
        }

        void RecvNetPacket( UdpHeader udpHdr, NetPacket netPacket, IPEndPoint endPoint )
        {
            MemoryStream data = netPacket.GetData();

            packetMap.Remove( udpHdr.MsgStartSeq );

            try
            {
                data = netFilter.ProcessIncoming( data );
            }
            catch
            {
                return;
            }

            if ( data.Length < 4 )
                return; // we need at least an EMsg

            byte[] peek = new byte[4];
            data.Read(peek, 0, peek.Length);
            data.Seek(0, SeekOrigin.Begin);

            EMsg eMsg = ( EMsg )BitConverter.ToUInt32( peek, 0 );
            this.RecvNetMsg( eMsg, data, endPoint );
        }

        void RecvNetMsg( EMsg eMsg, MemoryStream data, IPEndPoint endPoint )
        {
            Console.WriteLine( "Got EMsg: " + MsgUtil.GetMsg(eMsg) + " (Proto:" + MsgUtil.IsProtoBuf(eMsg) + ")" );

            if ( eMsg == EMsg.ChannelEncryptRequest )
            {
                var encRec = new ClientMsg<MsgChannelEncryptRequest, MsgHdr>( data );

                Console.WriteLine( "Got encryption request for universe: " + encRec.Msg.Universe );

                tempSessionKey = CryptoHelper.GenerateRandomBlock( 32 );

                var encResp = new ClientMsg<MsgChannelEncryptResponse, MsgHdr>();

                byte[] cryptedSessKey = CryptoHelper.RSAEncrypt( tempSessionKey, KeyManager.GetPublicKey( encRec.Msg.Universe ) );
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
                var encRes = new ClientMsg<MsgChannelEncryptResult, MsgHdr>( data );

                if ( encRes.Msg.Result == EResult.OK )
                    netFilter = new NetFilterEncryption( tempSessionKey );

                Console.WriteLine( "Crypto result: " + encRes.Msg.Result );
            }

            if ( eMsg == EMsg.ClientLoggedOff )
            {
                netFilter = new NetFilter( null );
            }

            if ( MsgUtil.GetMsg(eMsg) == EMsg.Multi && MsgUtil.IsProtoBuf(eMsg))
            {
                this.MultiplexMsgMulti(data, endPoint);
            }

            if ( NetMsgReceived != null )
                NetMsgReceived( this, new NetMsgEventArgs( endPoint, eMsg, data ) );
        }

        void MultiplexMsgMulti(MemoryStream data, IPEndPoint endPoint)
        {
            var msgMulti = new ClientMsgProtobuf<MsgMulti>( data );

            byte[] payload = msgMulti.Msg.Proto.message_body;

            if ( msgMulti.Msg.Proto.size_unzipped > 0 )
            {
                try
                {
                    payload = PKZipBuffer.Decompress( payload );
                }
                catch { return; }
            }

            MultiplexPayload(payload, endPoint);
        }

        private void MultiplexPayload( byte[] payload, IPEndPoint endPoint )
        {
            DataStream ds = new DataStream(payload);

            while (ds.SizeRemaining() != 0)
            {
                uint subSize = ds.ReadUInt32();
                byte[] subData = ds.ReadBytes(subSize);

                EMsg eMsg = (EMsg)BitConverter.ToUInt32(subData, 0);

                this.RecvNetMsg(eMsg, new MemoryStream(subData), endPoint);
            }
        }

        void RecvUdpPacket( object sender, PacketReceivedEventArgs e )
        {
            UdpPacket udpPkt = new UdpPacket( new MemoryStream(e.Packet.Data) );

            if ( !udpPkt.IsValid )
                return;

            seqAcked = udpPkt.Header.SeqThis;

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
