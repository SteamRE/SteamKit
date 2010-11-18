using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SteamKit
{
    // handle UDP, socket, sequences, acks, resending, splitting, takes IClientMsg and builds udppacket
    class UdpConnection : Connection
    {
        private Socket netSocket;

        private SocketAsyncEventArgs asyncArgs;
        private byte[] recvBuffer;

        private IPEndPoint localEndPoint;
        private IPEndPoint targetEndPoint;

        private uint outSeq;
        private uint outSeqAck;
        private uint inSeq;
        private uint remoteConnID;

        private List<UdpPacket> sendPacketHistory;
        private DateTime nextResend;

        private Dictionary<uint, NetPacket> packetMap;

        public UdpConnection()
        {
            netSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            targetEndPoint = null;
            asyncArgs = null;

            ResetSequences();

            recvBuffer = new byte[1500];
            localEndPoint = new IPEndPoint(IPAddress.Any, 0);

            netSocket.Bind(localEndPoint);

            sendPacketHistory = new List<UdpPacket>();
            packetMap = new Dictionary<uint, NetPacket>();

            StartReceive();
            Scheduler.Instance.Think += Think;
        }

        private void StartReceive()
        {
            if (asyncArgs == null)
            {
                asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                asyncArgs.SetBuffer(recvBuffer, 0, recvBuffer.Length);
            }

            asyncArgs.RemoteEndPoint = localEndPoint;

            bool completedAsync = netSocket.ReceiveFromAsync(asyncArgs);

            if (!completedAsync)
            {
                IOCompleted(null, asyncArgs);
            }
        }

        private void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.ReceiveFrom)
            {
                throw new NotImplementedException();
            }

            HandleReceivedPacket(e);

            StartReceive();
        }

        private void HandleReceivedPacket(SocketAsyncEventArgs e)
        {
            if (targetEndPoint != null && !e.RemoteEndPoint.Equals(targetEndPoint))
            {
                Console.WriteLine("Got packet from wrong target EndPoint");
                return;
            }

            MemoryStream recvms = new MemoryStream(recvBuffer);
            recvms.SetLength(e.BytesTransferred);

            UdpPacket udpPkt = new UdpPacket(recvms);

            if (!udpPkt.IsValid)
            {
                Console.WriteLine("Invalid UdpPacket received");
                return;
            }
            else if (targetEndPoint == null && udpPkt.Header.PacketType != EUdpPacketType.Challenge)
            {
                Console.WriteLine("Ignoring packet of type " + udpPkt.Header.PacketType + " because no endpoint is set");
                return;
            }
            else if (remoteConnID > 0 && udpPkt.Header.SourceConnID != remoteConnID)
            {
                Console.WriteLine("Wrong connection ID received, ignoring");
                return;
            }
            else if (udpPkt.Header.PacketType != EUdpPacketType.Challenge && udpPkt.Header.SeqThis <= inSeq)
            {
                Console.WriteLine("Got packet with sequence we've already seen " + udpPkt.Header.SeqThis);

                // do we always ack this if the server resends a packet we acked but they lost the ack?
                SendGenericAck();
                return;
            }

            outSeqAck = udpPkt.Header.SeqAck;
            inSeq = udpPkt.Header.SeqThis;

            Console.WriteLine("out: " + outSeq + " / " + outSeqAck + " -- in: " + inSeq);
            Console.WriteLine(udpPkt.Header.PacketType);

            sendPacketHistory.RemoveAll(delegate(UdpPacket x) { return x.Header.SeqThis <= outSeqAck; });

            switch (udpPkt.Header.PacketType)
            {
                case EUdpPacketType.Challenge:
                    RecvChallenge(udpPkt, (IPEndPoint)e.RemoteEndPoint);
                    break;

                case EUdpPacketType.Accept:
                    RecvAccept(udpPkt, (IPEndPoint)e.RemoteEndPoint);
                    break;

                case EUdpPacketType.Data:
                    RecvData(udpPkt, (IPEndPoint)e.RemoteEndPoint);
                    break;

                case EUdpPacketType.Datagram:
                    RecvDatagram(udpPkt, (IPEndPoint)e.RemoteEndPoint);
                    break;

                case EUdpPacketType.Disconnect:
                    RecvDisconnect(udpPkt, (IPEndPoint)e.RemoteEndPoint);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        void RecvChallenge(UdpPacket udpPkt, IPEndPoint endPoint)
        {
            ChallengeData cr = new ChallengeData();
            cr.deserialize(udpPkt.Payload);

            OnChallengeReceived(new ChallengeEventArgs(endPoint, cr));
        }

        void RecvAccept(UdpPacket udpPkt, IPEndPoint endPoint)
        {
            remoteConnID = udpPkt.Header.SourceConnID;

            OnAcceptReceived(new NetworkEventArgs(endPoint));
        }

        void RecvDatagram(UdpPacket udpPkt, IPEndPoint endPoint)
        {
            OnDatagramReceived(new DataEventArgs(endPoint, udpPkt.Payload));
        }

        void RecvDisconnect(UdpPacket udpPkt, IPEndPoint endPoint)
        {
            OnDisconnectReceived(new NetworkEventArgs(endPoint));
        }

        void RecvData(UdpPacket udpPkt, IPEndPoint endPoint)
        {
            uint curSeq = outSeq;

            NetPacket netPacket = null;

            if (packetMap.ContainsKey(udpPkt.Header.MsgStartSeq))
                netPacket = packetMap[udpPkt.Header.MsgStartSeq];
            else
            {
                netPacket = new NetPacket(udpPkt.Header.MsgStartSeq, udpPkt.Header.PacketsInMsg);

                packetMap.Add(udpPkt.Header.MsgStartSeq, netPacket);
            }

            netPacket.AddData(udpPkt.Header, udpPkt.Payload);

            if (netPacket.IsCompleted)
                this.RecvNetPacket(udpPkt.Header, netPacket, endPoint);

            // send a datagram ack if we didn't do anything
            if (outSeq == curSeq)
            {
                SendGenericAck();
            }
        }

        void RecvNetPacket(UdpHeader udpHdr, NetPacket netPacket, IPEndPoint endPoint)
        {
            MemoryStream data = netPacket.GetData();

            packetMap.Remove(udpHdr.MsgStartSeq);

            if (netFilter != null)
            {
                try
                {
                    data = netFilter.ProcessIncoming(data);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Encryption error in packet seq " + udpHdr.SeqThis + " : " + ex.Message);
                    return;
                }
            }

            OnNetMsgReceived(new DataEventArgs(endPoint, data));
        }

        private void Think(object sender, EventArgs args)
        {
            if (targetEndPoint != null && DateTime.Now >= nextResend)
            {
                if (outSeq < outSeqAck)
                {
                    foreach (UdpPacket packet in sendPacketHistory)
                    {
                        Console.WriteLine("Resending packet sequence " + packet.Header.SeqThis);

                        SendBuffer(packet.GetData().ToArray());
                    }
                }

                ResetResendTimer();
            }
        }

        private void ResetResendTimer()
        {
            nextResend = DateTime.Now.AddSeconds(1);
        }

        private UdpPacket ConstructPacket(EUdpPacketType type, MemoryStream data)
        {
            UdpPacket packet = new UdpPacket(type);
            packet.Header.SeqThis = outSeq;
            packet.Header.SeqAck = inSeq;
            packet.Header.DestConnID = remoteConnID;

            packet.Header.PacketsInMsg = 1;
            packet.Header.MsgStartSeq = outSeq;

            if (data != null)
            {
                if (netFilter != null)
                {
                    data = new MemoryStream(netFilter.ProcessOutgoing(data.ToArray()));
                }

                packet.SetPayload(data);
            }

            return packet;
        }

        private void SendBuffer(byte[] data)
        {
            netSocket.SendTo(data, targetEndPoint);
        }

        private void SendGenericAck()
        {
            UdpPacket packet = ConstructPacket(EUdpPacketType.Datagram, null);

            packet.Header.PacketsInMsg = 0;
            packet.Header.MsgStartSeq = 0;
            packet.Header.SeqThis = 0;

            Console.WriteLine("Sending generic ack " + packet.Header.SeqThis + " / ack: " + packet.Header.SeqAck);

            SendBuffer(packet.GetData().ToArray());
        }

        private void SendOutgoingSequenced(UdpPacket packet)
        {
            outSeq++;
            
            sendPacketHistory.Add(packet);

            Console.WriteLine("Sending message " + packet.Header.SeqThis + " / ack: " + packet.Header.SeqAck);

            SendBuffer(packet.GetData().ToArray());
        }

        private void ResetSequences()
        {
            outSeq = 1;
            outSeqAck = 0;
            inSeq = 0;
            remoteConnID = 0;
        }

        public override void SetTargetEndPoint(IPEndPoint remoteEndPoint)
        {
            targetEndPoint = remoteEndPoint;
            ResetSequences();
        }

        public override void SendMessage(IClientMsg clientmsg)
        {
            Console.WriteLine("Sending message " + clientmsg.GetType());

            ResetResendTimer();

            MemoryStream data = clientmsg.serialize();

            UdpPacket packet = ConstructPacket(EUdpPacketType.Data, data);
            SendOutgoingSequenced(packet);
        }

        public void SendChallengeRequest(IPEndPoint ipAddr)
        {
            UdpPacket packet = new UdpPacket(EUdpPacketType.ChallengeReq);
            packet.Header.SeqThis = 1;

            netSocket.SendTo(packet.GetData().ToArray(), ipAddr);
        }

        public override void SendConnect(UInt32 challengeValue)
        {
            ConnectData cd = new ConnectData();
            cd.ChallengeValue = challengeValue ^ ConnectData.CHALLENGE_MASK;

            UdpPacket packet = ConstructPacket(EUdpPacketType.Connect, cd.serialize());
            SendOutgoingSequenced(packet);
        }

        public override IPAddress GetLocalIP()
        {
            return NetHelper.GetLocalIP(netSocket);
        }
    }
}
