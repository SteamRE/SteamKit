/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace SteamKit2
{
    enum UdpConnectionState
    {
        Disconnected,
        ChallengeReqSent,
        ConnectSent,
        Connected,
        Disconnecting
    }

    class UdpConnection : Connection
    {
        /// <summary>
        /// Seconds to wait before sending packets.
        /// </summary>
        private const uint RESEND_DELAY = 3;
        /// <summary>
        /// Seconds to wait before considering the connection dead.
        /// </summary>
        private const uint TIMEOUT_DELAY = 20;

        /// <summary>
        /// Maximum number of packets to resend when RESEND_DELAY is exceeded.
        /// </summary>
        private const uint RESEND_COUNT = 3;
        /// <summary>
        /// Maximum number of packets that we can be waiting on at a time.
        /// </summary>
        private const uint AHEAD_COUNT = 5;

        /// <summary>
        /// Contains information about the state of the connection, used
        /// to filter out packets that are unexpected or not valid given
        /// the state of the connection.
        /// </summary>
        private UdpConnectionState state;

        private Thread netThread;
        private Socket sock;
        private IPEndPoint remoteEndPoint;

        private DateTime timeOut;
        private DateTime nextResend;

        private uint remoteConnId;

        /// <summary>
        /// The next outgoing sequence number to be used.
        /// </summary>
        private uint outSeq;
        /// <summary>
        /// The highest sequence number of outgoing packets.
        /// </summary>
        private uint outSeqSent;
        /// <summary>
        /// The sequence number of the highest packet acknowledged by the server.
        /// </summary>
        private uint outSeqAcked;

        /// <summary>
        /// The sequence number we plan on acknowledging receiving with the next Ack.
        /// All packets below or equal to inSeq *must* have been received.
        /// </summary>
        private uint inSeq;
        /// <summary>
        /// The highest sequence number we've acknowledged receiving.
        /// </summary>
        private uint inSeqAcked;
        /// <summary>
        /// The highest sequence number we've processed.
        /// </summary>
        private uint inSeqHandled;

        private List<UdpPacket> outPackets;
        private Dictionary<uint, UdpPacket> inPackets;

        public UdpConnection()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(localEndPoint);

            state = UdpConnectionState.Disconnected;
        }

        /// <summary>
        /// Connects to the specified CM server.
        /// </summary>
        /// <param name="endPoint">The CM server.</param>
        public override void Connect(IPEndPoint endPoint)
        {
            Disconnect();

            outPackets = new List<UdpPacket>();
            inPackets = new Dictionary<uint, UdpPacket>();

            remoteEndPoint = endPoint;
            remoteConnId = 0;

            outSeq = 1;
            outSeqSent = 0;
            outSeqAcked = 0;

            inSeq = 0;
            inSeqAcked = 0;
            inSeqHandled = 0;

            netThread = new Thread(NetLoop);
            netThread.Name = "UdpConnection Thread";
            netThread.Start();
        }

        /// <summary>
        /// Disconnects this instance, blocking until the
        /// queue of messages is empty or the connection
        /// is otherwise terminated.
        /// </summary>
        public override void Disconnect()
        {
            if ( netThread == null )
                return;

            state = UdpConnectionState.Disconnecting;

            // Graceful shutdown allows for the
            // connection to empty its queue of
            // messages to send
            netThread.Join();
        }

        /// <summary>
        /// Serializes and sends the provided message to the server
        /// in as many packets as is necessary.
        /// </summary>
        /// <param name="clientMsg">The ClientMsg</param>
        public override void Send(IClientMsg clientMsg)
        {
            if ( state != UdpConnectionState.Connected )
                return;

            MemoryStream ms = new MemoryStream();
            clientMsg.Serialize(ms);
            byte[] data = ms.ToArray();

            if ( NetFilter != null )
                data = NetFilter.ProcessOutgoing(data);

            ms = new MemoryStream(data);

            UdpPacket[] packets = new UdpPacket[( (uint) data.Length / UdpPacket.MAX_PAYLOAD ) + 1];

            for ( int i = 0; i < packets.Length; i++ )
            {
                int index = i * (int) UdpPacket.MAX_PAYLOAD;
                int length = Math.Min((int) UdpPacket.MAX_PAYLOAD, data.Length - index);

                UdpPacket packet = new UdpPacket(EUdpPacketType.Data, ms, length);
                packet.Header.MsgSize = (uint) data.Length;

                packets[i] = packet;
            }

            SendSequenced(packets);
        }

        /// <summary>
        /// Sends the packet as a sequenced, reliable packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void SendSequenced(UdpPacket packet)
        {
            packet.Header.SeqThis = outSeq++;
            packet.Header.MsgStartSeq = outSeq;

            packet.Header.PacketsInMsg = 1;

            outPackets.Add(packet);
        }

        /// <summary>
        /// Sends the packets as one sequenced, reliable net message.
        /// </summary>
        /// <param name="packets">The packets that make up the single net message</param>
        private void SendSequenced(UdpPacket[] packets)
        {
            uint msgStart = outSeq;

            foreach ( UdpPacket packet in packets )
            {
                SendSequenced(packet);

                // Correct for any assumptions made for the
                // single-packet case.
                packet.Header.PacketsInMsg = (uint) packets.Length;
                packet.Header.MsgStartSeq = msgStart;
            }
        }

        /// <summary>
        /// Sends a packet immediately.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void SendPacket(UdpPacket packet)
        {
            packet.Header.DestConnID = remoteConnId;
            packet.Header.SeqAck = inSeqAcked = inSeq;

            DebugLog.WriteLine("UdpConnection", "Sent -> {0} Seq {1} Ack {2}; {3} bytes; Message: {4} bytes {5} packets",
                packet.Header.PacketType, packet.Header.SeqThis, packet.Header.SeqAck,
                packet.Header.PayloadSize, packet.Header.MsgSize, packet.Header.PacketsInMsg);

            MemoryStream ms = packet.GetData();

            sock.SendTo(ms.ToArray(), remoteEndPoint);

            // If we've been idle but completely acked
            // for more than two seconds, the next sent
            // packet will trip the resend detection.
            // This fixes that.
            if ( outSeqSent == outSeqAcked )
                nextResend = DateTime.Now.AddSeconds(RESEND_DELAY);

            if ( packet.Header.SeqThis > outSeqSent )
                outSeqSent = packet.Header.SeqThis;
        }

        /// <summary>
        /// Sends a datagram Ack, used when an Ack needs to be
        /// sent but there is no data response to piggy-back on.
        /// </summary>
        private void SendAck()
        {
            SendPacket(new UdpPacket(EUdpPacketType.Datagram));
        }

        /// <summary>
        /// Sends or resends sequenced messages, if necessary. Also
        /// resonsible for throttling the rate at which they are sent.
        /// </summary>
        private void SendPendingMessages()
        {
            if ( DateTime.Now > nextResend && outSeqSent > outSeqAcked )
            {
                DebugLog.WriteLine("UdpConnection", "Sequenced packet resend required");

                // Don't send more than 3 (Steam behavior?), go ahead
                // and roll-back outSeqSent so if we need to resent more
                // it doesn't cause us much trouble
                for ( int i = 0; i < RESEND_COUNT && i < outPackets.Count; i++ )
                {
                    SendPacket(outPackets[i]);
                    outSeqSent = outPackets[i].Header.SeqThis;
                }

                nextResend = DateTime.Now.AddSeconds(RESEND_DELAY);
            }
            else if ( outSeqSent < outSeqAcked + AHEAD_COUNT )
            {
                // I've never seen Steam send more than 4 packets before
                // it gets an Ack, so this limits the number of sequenced
                // packets that can be sent out at one time.
                for ( int i = (int) ( outSeqSent - outSeqAcked ); i < AHEAD_COUNT && i < outPackets.Count; i++ )
                {
                    SendPacket(outPackets[i]);
                    outSeqSent = outPackets[i].Header.SeqThis;
                }
            }
        }

        /// <summary>
        /// Returns the number of message parts in the next message.
        /// </summary>
        /// <returns>Non-zero number of message parts if a message is ready to be handled, 0 otherwise</returns>
        private uint ReadyMessageParts()
        {
            UdpPacket packet;

            // Make sure that the first packet of the next message
            // to handle is present
            if ( !inPackets.TryGetValue(inSeqHandled + 1, out packet) )
                return 0;

            // ...and if relevant, all subparts of the message also
            for ( uint i = 1; i < packet.Header.PacketsInMsg; i++ )
                if ( !inPackets.ContainsKey(inSeqHandled + 1 + i) )
                    return 0;

            return packet.Header.PacketsInMsg;
        }

        /// <summary>
        /// Dispatches up to one message to the rest of SteamKit
        /// </summary>
        /// <returns>True if there are possibly more messages to dispatch, False otherwise</returns>
        private bool DispatchMessage()
        {
            uint numPackets = ReadyMessageParts();

            if ( numPackets == 0 )
                return false;
            
            MemoryStream payload = new MemoryStream();
            for ( uint i = 0; i < numPackets; i++ )
            {
                UdpPacket packet;

                inPackets.TryGetValue(++inSeqHandled, out packet);
                inPackets.Remove(inSeqHandled);

                packet.Payload.WriteTo(payload);
            }

            byte[] data = payload.ToArray();

            if ( NetFilter != null )
                data = NetFilter.ProcessIncoming(data);

            DebugLog.WriteLine("UdpConnection", "Dispatching message; {0} bytes", data.Length);

            OnNetMsgReceived(new NetMsgEventArgs(data, remoteEndPoint));

            return true;
        }

        /// <summary>
        /// Processes incoming packets, maintains connection consistency, and
        /// oversees outgoing packets.
        /// </summary>
        private void NetLoop()
        {
            // Variables that will be used deeper in the function; locating them
            // here avoids recreating them since they don't need to be.
            EndPoint packetSender = (EndPoint) new IPEndPoint(IPAddress.Any, 0);
            byte[] buf = new byte[2048];

            timeOut = DateTime.Now.AddSeconds(TIMEOUT_DELAY);
            nextResend = DateTime.Now.AddSeconds(RESEND_DELAY);            

            // Begin by sending off the challenge request
            SendPacket(new UdpPacket(EUdpPacketType.ChallengeReq));
            state = UdpConnectionState.ChallengeReqSent;

            while ( state != UdpConnectionState.Disconnected )
            {
                // Wait up to 150ms for data, if none is found and the
                // timeout is exceeded, we're done here.
                if ( !sock.Poll(150000, SelectMode.SelectRead) 
                    && DateTime.Now > timeOut )
                {
                    DebugLog.WriteLine("UdpConnection", "Connection timed out");

                    state = UdpConnectionState.Disconnected;
                    break;
                }

                // By using a 10ms wait, we allow for multiple packets
                // sent at the time to all be processed before moving on
                // to processing output and therefore Acks (the more we
                // process at the same time, the fewer acks we have to send)
                while ( sock.Poll(10000, SelectMode.SelectRead) )
                {
                    int length = sock.ReceiveFrom(buf, ref packetSender);                    

                    // Ignore packets that aren't sent by the server
                    // we're connected to.
                    if ( !packetSender.Equals(remoteEndPoint) )
                        continue;

                    // Data from the desired server was received; delay timeout
                    timeOut = DateTime.Now.AddSeconds(TIMEOUT_DELAY);

                    MemoryStream ms = new MemoryStream(buf, 0, length);
                    UdpPacket packet = new UdpPacket(ms);

                    ReceivePacket(packet);
                }

                // Send or resend any sequenced packets; a call
                // to ReceivePacket can set our state to disconnected
                // so don't send anything we have queued in that case
                if ( state != UdpConnectionState.Disconnected )
                    SendPendingMessages();

                // If we received data but had no data to send back,
                // we need to manually Ack (usually tags along with
                // outgoing data); also acks disconnections
                if ( inSeq != inSeqAcked )
                    SendAck();

                // If a graceful shutdown has been requested, nothing in the
                // outgoing queue is discarded. Once it's empty, we exit.
                if ( state == UdpConnectionState.Disconnecting && outPackets.Count == 0 )
                {
                    DebugLog.WriteLine("UdpConnection", "Graceful disconnect completed");

                    state = UdpConnectionState.Disconnected;
                }
            }

            DebugLog.WriteLine("UdpConnection", "Calling OnDisconnected");
            OnDisconnected(EventArgs.Empty);
        }

        /// <summary>
        /// Receives the packet, performs all sanity checks and
        /// then passes it along as necessary.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceivePacket(UdpPacket packet)
        {
            // Check for a malformed packet
            if ( !packet.IsValid )
                return;
            else if ( remoteConnId > 0 && packet.Header.SourceConnID != remoteConnId )
                return;

            DebugLog.WriteLine("UdpConnection", "<- Recv'd {0} Seq {1} Ack {2}; {3} bytes; Message: {4} bytes {5} packets",
                packet.Header.PacketType, packet.Header.SeqThis, packet.Header.SeqAck,
                packet.Header.PayloadSize, packet.Header.MsgSize, packet.Header.PacketsInMsg);

            // Throw away any duplicate messages we've already received, making sure to
            // re-ack it in case it got lost.
            if ( packet.Header.PacketType == EUdpPacketType.Data && packet.Header.SeqThis < inSeq )
            {
                SendAck();
                return;
            }

            // When we get a SeqAck, all packets with sequence
            // numbers below that have been safely received by
            // the server; we are now free to remove our copies
            if ( outSeqAcked < packet.Header.SeqAck )
            {
                outSeqAcked = packet.Header.SeqAck;
                outPackets.RemoveAll(delegate(UdpPacket x) { return x.Header.SeqThis <= outSeqAcked; });

                nextResend = DateTime.Now.AddSeconds(RESEND_DELAY);
            }

            // inSeq should always be the latest value
            // that we can ack, so advance it as far
            // as is possible.
            if ( packet.Header.SeqThis == inSeq + 1 )
                do
                    inSeq++;
                while ( inPackets.ContainsKey(inSeq + 1) );

            switch ( packet.Header.PacketType )
            {
                case EUdpPacketType.Challenge:
                    ReceiveChallenge(packet);
                    break;

                case EUdpPacketType.Accept:
                    ReceiveAccept(packet);
                    break;

                case EUdpPacketType.Data:
                    ReceiveData(packet);
                    break;

                case EUdpPacketType.Disconnect:
                    DebugLog.WriteLine("UdpConnection", "Disconnected by server");
                    state = UdpConnectionState.Disconnected;
                    return;

                case EUdpPacketType.Datagram:
                    break;

                default:
                    DebugLog.WriteLine("UdpConnection", "Received unexpected packet type " + packet.Header.PacketType);
                    break;
            }
        }

        /// <summary>
        /// Receives the challenge and responds with a Connect request
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceiveChallenge(UdpPacket packet)
        {
            if ( state != UdpConnectionState.ChallengeReqSent )
                return;

            ChallengeData cr = new ChallengeData();
            cr.Deserialize(packet.Payload);

            ConnectData cd = new ConnectData();
            cd.ChallengeValue = cr.ChallengeValue ^ ConnectData.CHALLENGE_MASK;

            MemoryStream ms = new MemoryStream();
            cd.Serialize(ms);

            SendSequenced(new UdpPacket(EUdpPacketType.Connect, ms));

            state = UdpConnectionState.ConnectSent;
            inSeqHandled = packet.Header.SeqThis;
        }

        /// <summary>
        /// Receives the notification of connection acception
        /// and sets the connection id that will be used for the
        /// connection's duration.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceiveAccept(UdpPacket packet)
        {
            if ( state != UdpConnectionState.ConnectSent )
                return;

            DebugLog.WriteLine("UdpConnection", "Connection established");
            
            state = UdpConnectionState.Connected;
            remoteConnId = packet.Header.SourceConnID;
            inSeqHandled = packet.Header.SeqThis;
        }

        /// <summary>
        /// Receives typical data packets before dispatching them
        /// for consumption by the rest of SteamKit
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceiveData(UdpPacket packet)
        {
            if ( state != UdpConnectionState.Connected && state != UdpConnectionState.Disconnecting )
                return;
            else if ( inPackets.ContainsKey(packet.Header.SeqThis) )
                return;

            inPackets.Add(packet.Header.SeqThis, packet);

            while ( DispatchMessage() ) ;
        }

        public override IPAddress GetLocalIP()
        {
            return NetHelpers.GetLocalIP(sock);
        }
    }
}
