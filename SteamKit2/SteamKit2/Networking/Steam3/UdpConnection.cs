/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SteamKit2.Internal;

namespace SteamKit2
{
    class UdpConnection : IConnection
    {
        private enum State
        {
            Disconnected,
            ChallengeReqSent,
            ConnectSent,
            Connected,
            Disconnecting
        }

        /// <summary>
        /// Seconds to wait before sending packets.
        /// </summary>
        private const uint RESEND_DELAY = 3;
        /// <summary>
        /// Seconds to wait before considering the connection dead.
        /// </summary>
        private const uint TIMEOUT_DELAY = 60;

        /// <summary>
        /// Maximum number of packets to resend when RESEND_DELAY is exceeded.
        /// </summary>
        private const uint RESEND_COUNT = 3;
        /// <summary>
        /// Maximum number of packets that we can be waiting on at a time.
        /// </summary>
        private const uint AHEAD_COUNT = 5;

        /// <summary>
        /// Contains information about the state of the connection, used to filter out packets that are
        /// unexpected or not valid given the state of the connection.
        /// </summary>
        private volatile int state;

        private Thread? netThread;
        private Socket sock;

        private DateTime timeOut;
        private DateTime nextResend;

        private static uint sourceConnId = 512;
        private uint remoteConnId;

        /// <summary>
        /// The next outgoing sequence number to be used.
        /// </summary>
        private uint outSeq;
        /// <summary>
        /// The highest sequence number of an outbound packet that has been sent.
        /// </summary>
        private uint outSeqSent;
        /// <summary>
        /// The sequence number of the highest packet acknowledged by the server.
        /// </summary>
        private uint outSeqAcked;

        /// <summary>
        /// The sequence number we plan on acknowledging receiving with the next Ack. All packets below or equal
        /// to inSeq *must* have been received, but not necessarily handled.
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

        [NotNull] private List<UdpPacket>? outPackets;
        [NotNull] private Dictionary<uint, UdpPacket>? inPackets;

        private ILogContext log;

        public UdpConnection(ILogContext log)
        {
            this.log = log ?? throw new ArgumentNullException( nameof( log ) );

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            sock.Bind(localEndPoint);

            state = (int)State.Disconnected;
        }

        public event EventHandler<NetMsgEventArgs>? NetMsgReceived;

        public event EventHandler? Connected;

        public event EventHandler<DisconnectedEventArgs>? Disconnected;

        public EndPoint? CurrentEndPoint { get; private set; }

        public ProtocolTypes ProtocolTypes => ProtocolTypes.Udp;

        /// <summary>
        /// Connects to the specified CM server.
        /// </summary>
        /// <param name="endPoint">The endPoint to connect to</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public void Connect(EndPoint endPoint, int timeout)
        {
            outPackets = new List<UdpPacket>();
            inPackets = new Dictionary<uint, UdpPacket>();

            CurrentEndPoint = null;
            remoteConnId = 0;

            outSeq = 1;
            outSeqSent = 0;
            outSeqAcked = 0;

            inSeq = 0;
            inSeqAcked = 0;
            inSeqHandled = 0;

            netThread = new Thread(NetLoop);
            netThread.Name = "UdpConnection Thread";
            netThread.Start(endPoint);
        }

        /// <summary>
        /// Disconnects this instance, blocking until the queue of messages is empty or the connection
        /// is otherwise terminated.
        /// </summary>
        public void Disconnect( bool userInitiated )
        {
            if ( netThread == null )
                return;

            // if we think we aren't already disconnected, apply disconnecting unless we read back disconnected
            if ( state != (int)State.Disconnected && Interlocked.Exchange(ref state, (int)State.Disconnecting) == (int)State.Disconnected )
            {
                state = (int)State.Disconnected;
            }

            // only notify if we actually applied the disconnecting state
            if ( state == (int)State.Disconnecting ) {
                // Play nicely and let the server know that we're done. Other party is expected to Ack this,
                // so it needs to be sent sequenced.
                SendSequenced(new UdpPacket(EUdpPacketType.Disconnect));
            }

            // Advance this the same way that steam does, when a socket gets reused.
            sourceConnId += 256;

            Disconnected?.Invoke( this, new DisconnectedEventArgs( userInitiated ) );
        }

        /// <summary>
        /// Serializes and sends the provided data to the server in as many packets as is necessary.
        /// </summary>
        /// <param name="data">The data to send to the server</param>
        public void Send( byte[] data )
        {
            if ( state != (int)State.Connected )
                return;

            SendData( new MemoryStream( data ) );
        }

        /// <summary>
        /// Sends the data sequenced as a single message, splitting it into multiple parts if necessary.
        /// </summary>
        /// <param name="ms">The data to send.</param>
        private void SendData( MemoryStream ms )
        {
            UdpPacket[] packets = new UdpPacket[ ( ms.Length / UdpPacket.MAX_PAYLOAD ) + 1 ];

            for ( int i = 0 ; i < packets.Length ; i++ )
            {
                long index = i * UdpPacket.MAX_PAYLOAD;
                long length = Math.Min( UdpPacket.MAX_PAYLOAD, ms.Length - index );

                packets[ i ] = new UdpPacket( EUdpPacketType.Data, ms, length );
                packets[ i ].Header.MsgSize = ( uint )ms.Length;
            }

            SendSequenced( packets );
        }

        /// <summary>
        /// Sends the packet as a sequenced, reliable packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void SendSequenced(UdpPacket packet)
        {
            lock ( outPackets )
            {
                packet.Header.SeqThis = outSeq;
                packet.Header.MsgStartSeq = outSeq;
                packet.Header.PacketsInMsg = 1;

                outPackets.Add( packet );

                outSeq++;
            }
        }

        /// <summary>
        /// Sends the packets as one sequenced, reliable net message.
        /// </summary>
        /// <param name="packets">The packets that make up the single net message</param>
        private void SendSequenced(UdpPacket[] packets)
        {
            lock ( outPackets )
            {
                uint msgStart = outSeq;

                foreach ( UdpPacket packet in packets )
                {
                    SendSequenced( packet );

                    // Correct for any assumptions made for the single-packet case.
                    packet.Header.PacketsInMsg = ( uint )packets.Length;
                    packet.Header.MsgStartSeq = msgStart;
                }
            }
        }

        /// <summary>
        /// Sends a packet immediately.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void SendPacket(UdpPacket packet)
        {
            packet.Header.SourceConnID = sourceConnId;
            packet.Header.DestConnID = remoteConnId;
            packet.Header.SeqAck = inSeqAcked = inSeq;

            log.LogDebug("UdpConnection", "Sent -> {0} Seq {1} Ack {2}; {3} bytes; Message: {4} bytes {5} packets",
                packet.Header.PacketType, packet.Header.SeqThis, packet.Header.SeqAck,
                packet.Header.PayloadSize, packet.Header.MsgSize, packet.Header.PacketsInMsg);

            byte[] data = packet.GetData();

            try
            {
                sock.SendTo( data, CurrentEndPoint );
            }
            catch ( SocketException e )
            {
                log.LogDebug( "UdpConnection", "Critical socket failure: " + e.SocketErrorCode );

                state = ( int )State.Disconnected;
                return;
            }

            // If we've been idle but completely acked for more than two seconds, the next sent
            // packet will trip the resend detection. This fixes that.
            if ( outSeqSent == outSeqAcked )
                nextResend = DateTime.Now.AddSeconds(RESEND_DELAY);

            // Sending should generally carry on from the packet most recently sent, even if it was a
            // resend (who knows what else was lost).
            if ( packet.Header.SeqThis > 0 )
                outSeqSent = Math.Max( outSeqSent, packet.Header.SeqThis );
        }

        /// <summary>
        /// Sends a datagram Ack, used when an Ack needs to be sent but there is no data response to piggy-back on.
        /// </summary>
        private void SendAck()
        {
            SendPacket(new UdpPacket(EUdpPacketType.Datagram));
        }

        /// <summary>
        /// Sends or resends sequenced messages, if necessary. Also responsible for throttling
        /// the rate at which they are sent.
        /// </summary>
        private void SendPendingMessages()
        {
            lock ( outPackets )
            {
                if ( DateTime.Now > nextResend && outSeqSent > outSeqAcked )
                {
                    // If we can't clear the send queue during a Disconnect, clear out the pending messages
                    if ( state == ( int )State.Disconnecting )
                    {
                        outPackets.Clear();
                    }

                    log.LogDebug( "UdpConnection", "Sequenced packet resend required" );

                    // Don't send more than 3 (Steam behavior?)
                    for ( int i = 0; i < RESEND_COUNT && i < outPackets.Count; i++ )
                        SendPacket( outPackets[ i ] );

                    nextResend = DateTime.Now.AddSeconds( RESEND_DELAY );
                }
                else if ( outSeqSent < outSeqAcked + AHEAD_COUNT )
                {
                    // I've never seen Steam send more than 4 packets before it gets an Ack, so this limits the
                    // number of sequenced packets that can be sent out at one time.
                    for ( int i = ( int )( outSeqSent - outSeqAcked ); i < AHEAD_COUNT && i < outPackets.Count; i++ )
                        SendPacket( outPackets[ i ] );
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

            // Make sure that the first packet of the next message to handle is present
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
        /// <returns>True if a message was dispatched, false otherwise</returns>
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

            log.LogDebug("UdpConnection", "Dispatching message; {0} bytes", data.Length);

            NetMsgReceived?.Invoke( this, new NetMsgEventArgs( data, CurrentEndPoint! ) );

            return true;
        }

        /// <summary>
        /// Processes incoming packets, maintains connection consistency, and oversees outgoing packets.
        /// </summary>
        private void NetLoop(object param)
        {
            // Variables that will be used deeper in the function; locating them here avoids recreating
            // them since they don't need to be.
            var userRequestedDisconnect = false;
            EndPoint packetSender = new IPEndPoint(IPAddress.Any, 0);
            byte[] buf = new byte[2048];

            CurrentEndPoint = param as EndPoint;

            if ( CurrentEndPoint != null )
            {
                timeOut = DateTime.Now.AddSeconds(TIMEOUT_DELAY);
                nextResend = DateTime.Now.AddSeconds(RESEND_DELAY);

                if ( Interlocked.CompareExchange(ref state, (int)State.ChallengeReqSent, (int)State.Disconnected) != (int)State.Disconnected )
                {
                    state = (int)State.Disconnected;
                    userRequestedDisconnect = true;
                }
                else
                {
                    // Begin by sending off the challenge request
                    SendPacket(new UdpPacket(EUdpPacketType.ChallengeReq));
                }
            }

            while ( state != (int)State.Disconnected )
            {
                try
                {
                    // Wait up to 150ms for data, if none is found and the timeout is exceeded, we're done here.
                    if ( !sock.Poll(150000, SelectMode.SelectRead)
                        && DateTime.Now > timeOut )
                    {
                        log.LogDebug("UdpConnection", "Connection timed out");

                        state = (int)State.Disconnected;
                        break;
                    }

                    // By using a 10ms wait, we allow for multiple packets sent at the time to all be processed before moving on
                    // to processing output and therefore Acks (the more we process at the same time, the fewer acks we have to send)
                    while ( sock.Poll(10000, SelectMode.SelectRead) )
                    {
                        int length = sock.ReceiveFrom(buf, ref packetSender);

                        // Ignore packets that aren't sent by the server we're connected to.
                        if ( !packetSender.Equals( CurrentEndPoint ) )
                            continue;

                        // Data from the desired server was received; delay timeout
                        timeOut = DateTime.Now.AddSeconds(TIMEOUT_DELAY);

                        MemoryStream ms = new MemoryStream(buf, 0, length);
                        UdpPacket packet = new UdpPacket(ms);

                        ReceivePacket(packet);
                    }
                }
                catch ( IOException ex )
                {
                    log.LogDebug( "UdpConnection", "Exception occurred while reading packet: {0}", ex );

                    state = ( int )State.Disconnected;
                    break;
                }
                catch ( SocketException e )
                {
                    log.LogDebug( "UdpConnection", "Critical socket failure: " + e.SocketErrorCode );

                    state = ( int )State.Disconnected;
                    break;
                }

                // Send or resend any sequenced packets; a call to ReceivePacket can set our state to disconnected
                // so don't send anything we have queued in that case
                if ( state != ( int )State.Disconnected )
                    SendPendingMessages();

                // If we received data but had no data to send back, we need to manually Ack (usually tags along with
                // outgoing data); also acks disconnections
                if ( inSeq != inSeqAcked )
                    SendAck();

                // If a graceful shutdown has been requested, nothing in the outgoing queue is discarded.
                // Once it's empty, we exit, since the last packet was our disconnect notification.
                if ( state == ( int )State.Disconnecting && outPackets.Count == 0 )
                {
                    log.LogDebug( "UdpConnection", "Graceful disconnect completed" );

                    state = ( int )State.Disconnected;
                    userRequestedDisconnect = true;
                    break;
                }
            }

            if ( sock != null )
            {
                sock.Dispose();
            }

            log.LogDebug("UdpConnection", "Calling OnDisconnected");
            Disconnected?.Invoke( this, new DisconnectedEventArgs( userRequestedDisconnect ) );
        }

        /// <summary>
        /// Receives the packet, performs all sanity checks and then passes it along as necessary.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceivePacket(UdpPacket packet)
        {
            // Check for a malformed packet
            if ( !packet.IsValid )
                return;
            else if ( remoteConnId > 0 && packet.Header.SourceConnID != remoteConnId )
                return;

            log.LogDebug("UdpConnection", "<- Recv'd {0} Seq {1} Ack {2}; {3} bytes; Message: {4} bytes {5} packets",
                packet.Header.PacketType, packet.Header.SeqThis, packet.Header.SeqAck,
                packet.Header.PayloadSize, packet.Header.MsgSize, packet.Header.PacketsInMsg);

            // Throw away any duplicate messages we've already received, making sure to
            // re-ack it in case it got lost.
            if ( packet.Header.PacketType == EUdpPacketType.Data && packet.Header.SeqThis < inSeq )
            {
                SendAck();
                return;
            }

            // When we get a SeqAck, all packets with sequence numbers below that have been safely received by
            // the server; we are now free to remove our copies
            if ( outSeqAcked < packet.Header.SeqAck )
            {
                outSeqAcked = packet.Header.SeqAck;

                // outSeqSent can be less than this in a very rare case involving resent packets.
                if ( outSeqSent < outSeqAcked )
                    outSeqSent = outSeqAcked;

                outPackets.RemoveAll( x => x.Header.SeqThis <= outSeqAcked );
                nextResend = DateTime.Now.AddSeconds(RESEND_DELAY);
            }

            // inSeq should always be the latest value that we can ack, so advance it as far as is possible.
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
                    log.LogDebug("UdpConnection", "Disconnected by server");
                    state = (int)State.Disconnected;
                    return;

                case EUdpPacketType.Datagram:
                    break;

                default:
                    log.LogDebug("UdpConnection", "Received unexpected packet type " + packet.Header.PacketType);
                    break;
            }
        }

        /// <summary>
        /// Receives the challenge and responds with a Connect request
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceiveChallenge(UdpPacket packet)
        {
            if ( Interlocked.CompareExchange( ref state, (int)State.ConnectSent, (int)State.ChallengeReqSent ) != (int)State.ChallengeReqSent )
                return;

            ChallengeData cr = new ChallengeData();
            cr.Deserialize(packet.Payload);

            ConnectData cd = new ConnectData();
            cd.ChallengeValue = cr.ChallengeValue ^ ConnectData.CHALLENGE_MASK;

            MemoryStream ms = new MemoryStream();
            cd.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            SendSequenced(new UdpPacket(EUdpPacketType.Connect, ms));

            inSeqHandled = packet.Header.SeqThis;
        }

        /// <summary>
        /// Receives the notification of an accepted connection and sets the connection id that will be used for the
        /// connection's duration.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceiveAccept(UdpPacket packet)
        {
            if ( Interlocked.CompareExchange( ref state, (int)State.Connected, (int)State.ConnectSent ) != (int)State.ConnectSent )
                return;

            log.LogDebug( "UdpConnection", "Connection established" );
            remoteConnId = packet.Header.SourceConnID;
            inSeqHandled = packet.Header.SeqThis;

            Connected?.Invoke( this, EventArgs.Empty );
        }

        /// <summary>
        /// Receives typical data packets before dispatching them for consumption by the rest of SteamKit
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void ReceiveData(UdpPacket packet)
        {
            // Data packets are unexpected if a valid connection has not been established
            if ( state != (int)State.Connected && state != (int)State.Disconnecting )
                return;

            // If we receive a packet that we've already processed (e.g. it got resent due to a lost ack)
            // or that is already waiting to be processed, do nothing.
            if ( packet.Header.SeqThis <= inSeqHandled || inPackets.ContainsKey(packet.Header.SeqThis) )
                return;

            inPackets.Add(packet.Header.SeqThis, packet);

            while ( DispatchMessage() ) ;
        }

        public IPAddress GetLocalIP()
            => NetHelpers.GetLocalIP(sock);
    }
}
