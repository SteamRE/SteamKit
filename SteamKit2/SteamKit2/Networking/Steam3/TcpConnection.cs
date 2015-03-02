﻿/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SteamKit2
{
    class TcpConnection : Connection
    {
        const uint MAGIC = 0x31305456; // "VT01"

        private class SocketState
        {
            public IPEndPoint Destination { get; private set; }
            public Socket Socket { get; private set; }
            public CancellationTokenSource CTS { get; private set; }
            public NetFilterEncryption NetFilter { get; private set; }
            public ConcurrentQueue<byte[]> Outbound { get; private set; }
            public Thread WorkThread { get; private set; }
            public NetworkStream NetStream { get; private set; }
            public BinaryReader NetReader { get; private set; }
            public BinaryWriter NetWriter { get; private set; }

            public SocketState(IPEndPoint destination, Socket socket, CancellationTokenSource cts)
            {
                this.Destination = destination;
                this.Socket = socket;
                this.CTS = cts;
                this.Outbound = new ConcurrentQueue<byte[]>();
            }

            public void AttachNetFilter(NetFilterEncryption netFilter)
            {
                this.NetFilter = netFilter;
            }

            public void AttachWorkThread(Thread thread)
            {
                NetStream = new NetworkStream(Socket, false);

                NetReader = new BinaryReader(NetStream);
                NetWriter = new BinaryWriter(NetStream);

                this.WorkThread = thread;
            }

            ~SocketState()
            {
                if (NetWriter != null) NetWriter.Dispose();
                if (NetReader != null) NetReader.Dispose();
                if (NetStream != null) NetStream.Dispose();
                CTS.Dispose();
                Socket.Dispose();
            }
        }

        private List<SocketState> activeStates;
        private SocketState currentState;
        private object netLock = new object();

        public TcpConnection()
        {
            this.activeStates = new List<SocketState>();
        }

        private void CancelAllState()
        {
            foreach (var state in activeStates)
            {
                state.CTS.Cancel();
            }

            activeStates.Clear();
        }

        private SocketState AttachConnectionState(IPEndPoint destination, Socket socket, CancellationTokenSource cts)
        {
            var state = new SocketState(destination, socket, cts);

            activeStates.Add(state);

            return state;
        }

        private void DetachConnectionState(SocketState state)
        {
            Debug.Assert(this.currentState != state);

            lock (netLock)
            {
                activeStates.Remove(state);
            }

            state.Socket.Close();
        }

        private bool FaultConnectionState(SocketState state)
        {
            bool shouldNotifyUser = true;

            lock (netLock)
            {
                shouldNotifyUser = !state.CTS.IsCancellationRequested;
                state.CTS.Cancel();
                activeStates.Remove(state);
                if (this.currentState == state) this.currentState = null;
            }

            return shouldNotifyUser;
        }

        private void NotifyConnectionFailed()
        {
            // dispatch callback so that the user can reconnect
            OnDisconnected(EventArgs.Empty);
        }

        private void GracefulShutdown(SocketState state)
        {
            try
            {
                // cleanup socket
                if (state.Socket.Connected)
                {
                    state.Socket.Shutdown(SocketShutdown.Both);
                    state.Socket.Disconnect(true);
                }
                state.Socket.Close();
            }
            catch
            {
                // Shutdown is throwing when the remote end closes the connection before SteamKit attempts to
                // so this should be safe as a no-op
                // see: https://bitbucket.org/VoiDeD/steamre/issue/41/socketexception-thrown-when-closing
            }
        }

        private void ConnectCompleted(SocketState state, bool success)
        {
            // Always discard result if our request was cancelled
            if (state.CTS.IsCancellationRequested)
            {
                DebugLog.WriteLine("TcpConnection", "Connection request to {0} was cancelled", state.Destination);
                if (success) GracefulShutdown(state);
                DetachConnectionState(state);
                return;
            }
            else if (!success)
            {
                DebugLog.WriteLine("TcpConnection", "Timed out while connecting to {0}", state.Destination);
                DetachConnectionState(state);
                NotifyConnectionFailed();
                return;
            }

            this.currentState = state;

            var netThread = new Thread(NetLoop);
            netThread.Name = "TcpConnection Thread";
            state.AttachWorkThread(netThread);
            netThread.Start(state);
        }

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public override void Connect(IPEndPoint endPoint, int timeout)
        {
            Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var _cts = new CancellationTokenSource();
            SocketState _state;

            lock (netLock)
            {
                CancelAllState();
                this.currentState = null;
                _state = AttachConnectionState(endPoint, _socket, _cts);
            }

            DebugLog.WriteLine("TcpConnection", "Connecting to {0}...", _state.Destination);

            ThreadPool.QueueUserWorkItem(sender =>
            {
                var state = (SocketState)sender;
                if (state.CTS.IsCancellationRequested)
                {
                    DebugLog.WriteLine("TcpConnection", "Connection to {0} cancelled by user", state.Destination);
                    DetachConnectionState(state);
                    return;
                }

                var asyncResult = state.Socket.BeginConnect(state.Destination, null, null);

                if (WaitHandle.WaitAny(new WaitHandle[] { asyncResult.AsyncWaitHandle, state.CTS.Token.WaitHandle }, timeout) == 0)
                {
                    try
                    {
                        state.Socket.EndConnect(asyncResult);
                        ConnectCompleted(state, true);
                    }
                    catch (Exception ex)
                    {
                        DebugLog.WriteLine("TcpConnection", "Socket exception while completing connection request to {0}: {1}", state.Destination, ex);
                        ConnectCompleted(state, false);
                    }
                }
                else
                {
                    ConnectCompleted(state, false);
                }
            }, _state);
        }

        // this is now a steamkit meme
        /// <summary>
        /// Nets the loop.
        /// </summary>
        void NetLoop(object param)
        {
            // poll for readable data every 100ms
            const int POLL_MS = 100;
            SocketState state = param as SocketState;

            while (!state.CTS.IsCancellationRequested)
            {
                try
                {
                    byte[] outbound;
                    while (state.Outbound.TryDequeue(out outbound))
                    {
                        state.NetWriter.Write((uint)outbound.Length);
                        state.NetWriter.Write(TcpConnection.MAGIC);
                        state.NetWriter.Write(outbound);
                        state.NetWriter.Flush();
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception while writing data: {0}", ex);

                    if (FaultConnectionState(state)) NotifyConnectionFailed();
                    break;
                }

                bool canRead = false;

                try
                {
                    canRead = state.Socket.Poll(POLL_MS * 1000, SelectMode.SelectRead);
                }
                catch (SocketException ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception while polling: {0}", ex);

                    if (FaultConnectionState(state)) NotifyConnectionFailed();
                    break;
                }

                if (!canRead)
                {
                    // nothing to read yet
                    continue;
                }

                byte[] packData = null;

                try
                {
                    // read the packet off the network
                    packData = ReadPacket(state);

                    // decrypt the data off the wire if needed
                    if (state.NetFilter != null)
                    {
                        packData = state.NetFilter.ProcessIncoming(packData);
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception occurred while reading packet: {0}", ex);

                    // signal that our connection is dead
                    if (FaultConnectionState(state)) NotifyConnectionFailed();
                    break;
                }

                try
                {
                    OnNetMsgReceived(new NetMsgEventArgs(packData, state.Destination));
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Unexpected exception propogated back to NetLoop: {0}", ex);
                }
            }

            // Thread is shutting down, ensure faulted state (even if user initiated)
            FaultConnectionState(state);
            GracefulShutdown(state);
        }


        byte[] ReadPacket(SocketState state)
        {
            // the tcp packet header is considerably less complex than the udp one
            // it only consists of the packet length, followed by the "VT01" magic
            uint packetLen = 0;
            uint packetMagic = 0;

            try
            {
                packetLen = state.NetReader.ReadUInt32();
                packetMagic = state.NetReader.ReadUInt32();
            }
            catch (IOException ex)
            {
                throw new IOException("Connection lost while reading packet header.", ex);
            }

            if (packetMagic != TcpConnection.MAGIC)
            {
                throw new IOException("Got a packet with invalid magic!");
            }

            // rest of the packet is the physical data
            byte[] packData = state.NetReader.ReadBytes((int)packetLen);

            if (packData.Length != packetLen)
            {
                throw new IOException("Connection lost while reading packet payload");
            }

            return packData;
        }

        public override void Disconnect()
        {
            lock (netLock)
            {
                CancelAllState();
                this.currentState = null;
            }
        }

        public override void Send(IClientMsg clientMsg)
        {
            var current = this.currentState;
            if (current == null || current.CTS.IsCancellationRequested)
            {
                DebugLog.WriteLine("TcpConnection", "Attempting to send client message when not connected: {0}", clientMsg.MsgType);
                return;
            }

            byte[] data = clientMsg.Serialize();

            if (current.NetFilter != null)
            {
                data = current.NetFilter.ProcessOutgoing(data);
            }

            current.Outbound.Enqueue(data);
        }

        public override IPAddress GetLocalIP()
        {
            var current = this.currentState;
            if (current == null) return IPAddress.None;

            try
            {
                return NetHelpers.GetLocalIP(current.Socket);
            }
            catch (Exception)
            {
                return IPAddress.None;
            }
        }

        public override void SetNetEncryptionFilter(NetFilterEncryption filter)
        {
            var current = this.currentState;
            if (current == null) return;

            current.AttachNetFilter(filter);
        }
    }
}
