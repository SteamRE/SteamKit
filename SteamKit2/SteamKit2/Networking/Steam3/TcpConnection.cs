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

        private IPEndPoint destination;
        private Socket socket;
        private NetFilterEncryption netFilter;
        private Thread netThread;
        private NetworkStream netStream;
        private BinaryReader netReader;
        private BinaryWriter netWriter;

        private CancellationTokenSource cancellationToken;
        private ManualResetEvent connectionFree;
        private object netLock, connectLock;

        public TcpConnection()
        {
            netLock = new object();
            connectLock = new object();
            connectionFree = new ManualResetEvent(true);
        }

        public override IPEndPoint CurrentEndPoint
        {
            get { return destination; }
        }

        private void Shutdown()
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(true);
                }
            }
            catch
            {
                // Shutdown is throwing when the remote end closes the connection before SteamKit attempts to
                // so this should be safe as a no-op
                // see: https://bitbucket.org/VoiDeD/steamre/issue/41/socketexception-thrown-when-closing
            }
        }

        private void Release( bool userRequestedDisconnect )
        {
            lock (netLock)
            {
                cancellationToken.Dispose();
                cancellationToken = null;

                if (netWriter != null)
                {
                    netWriter.Dispose();
                    netWriter = null;
                }

                if (netReader != null)
                {
                    netReader.Dispose();
                    netReader = null;
                }

                if (netStream != null)
                {
                    netStream.Dispose();
                    netStream = null;
                }

                socket.Close();
                socket = null;

                netFilter = null;
            }

            OnDisconnected( new DisconnectedEventArgs( userRequestedDisconnect ) );

            connectionFree.Set();
        }

        private void ConnectCompleted(bool success)
        {
            // Always discard result if our request was cancelled
            if (cancellationToken.IsCancellationRequested)
            {
                DebugLog.WriteLine("TcpConnection", "Connection request to {0} was cancelled", destination);
                if (success) Shutdown();
                Release( userRequestedDisconnect: true );
                return;
            }
            else if (!success)
            {
                DebugLog.WriteLine("TcpConnection", "Timed out while connecting to {0}", destination);
                Release( userRequestedDisconnect: false );
                return;
            }

            DebugLog.WriteLine("TcpConnection", "Connected to {0}", destination);

            try
            {
                lock (netLock)
                {
                    netStream = new NetworkStream(socket, false);
                    netReader = new BinaryReader(netStream);
                    netWriter = new BinaryWriter(netStream);
                    netFilter = null;

                    netThread = new Thread(NetLoop);
                    netThread.Name = "TcpConnection Thread";
                }

                netThread.Start();

                OnConnected(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("TcpConnection", "Exception while setting up connection to {0}: {1}", destination, ex);
                Release( userRequestedDisconnect: false );
            }
        }

        private void TryConnect(Object sender)
        {
            int timeout = (int)sender;
            if (cancellationToken.IsCancellationRequested)
            {
                DebugLog.WriteLine("TcpConnection", "Connection to {0} cancelled by user", destination);
                Release( userRequestedDisconnect: true );
                return;
            }

            var asyncResult = socket.BeginConnect(destination, null, null);

            if (WaitHandle.WaitAny(new WaitHandle[] { asyncResult.AsyncWaitHandle, cancellationToken.Token.WaitHandle }, timeout) == 0)
            {
                try
                {
                    socket.EndConnect(asyncResult);
                    ConnectCompleted(true);
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception while completing connection request to {0}: {1}", destination, ex);
                    ConnectCompleted(false);
                }
            }
            else
            {
                ConnectCompleted(false);
            }
        }

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public override void Connect(IPEndPoint endPoint, int timeout)
        {
            lock (connectLock)
            {
                DebugLog.WriteLine("TcpConnection", "Connecting to {0}...", endPoint);
                Disconnect();

                connectionFree.Reset();

                lock (netLock)
                {
                    Debug.Assert(cancellationToken == null);
                    cancellationToken = new CancellationTokenSource();

                    destination = endPoint;
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    ThreadPool.QueueUserWorkItem(TryConnect, timeout);
                }
            }
        }

        public override void Disconnect()
        {
            lock (connectLock)
            {
                lock (netLock)
                {
                    if (cancellationToken != null)
                    {
                        cancellationToken.Cancel();
                    }
                }

                connectionFree.WaitOne();
            }
        }

        // this is now a steamkit meme
        /// <summary>
        /// Nets the loop.
        /// </summary>
        void NetLoop()
        {
            // poll for readable data every 100ms
            const int POLL_MS = 100;

            while (!cancellationToken.IsCancellationRequested)
            {
                bool canRead = false;

                try
                {
                    canRead = socket.Poll(POLL_MS * 1000, SelectMode.SelectRead);
                }
                catch (SocketException ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception while polling: {0}", ex);
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
                    packData = ReadPacket();

                    // decrypt the data off the wire if needed
                    if (netFilter != null)
                    {
                        packData = netFilter.ProcessIncoming(packData);
                    }
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception occurred while reading packet: {0}", ex);
                    break;
                }

                try
                {
                    OnNetMsgReceived(new NetMsgEventArgs(packData, destination));
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Unexpected exception propogated back to NetLoop: {0}", ex);
                }
            }

            // Thread is shutting down, ensure socket is shut down and disposed
            bool userShutdown = cancellationToken.IsCancellationRequested;

            if ( userShutdown )
            {
                Shutdown();
            }
            Release( userShutdown );
        }

        byte[] ReadPacket()
        {
            // the tcp packet header is considerably less complex than the udp one
            // it only consists of the packet length, followed by the "VT01" magic
            uint packetLen = 0;
            uint packetMagic = 0;

            try
            {
                packetLen = netReader.ReadUInt32();
                packetMagic = netReader.ReadUInt32();
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
            byte[] packData = netReader.ReadBytes((int)packetLen);

            if (packData.Length != packetLen)
            {
                throw new IOException("Connection lost while reading packet payload");
            }

            return packData;
        }

        public override void Send(IClientMsg clientMsg)
        {
            lock (netLock)
            {
                if (socket == null || netStream == null)
                {
                    DebugLog.WriteLine("TcpConnection", "Attempting to send client message when not connected: {0}", clientMsg.MsgType);
                    return;
                }

                byte[] data = clientMsg.Serialize();

                if (netFilter != null)
                {
                    data = netFilter.ProcessOutgoing(data);
                }

                try
                {
                    netWriter.Write((uint)data.Length);
                    netWriter.Write(TcpConnection.MAGIC);
                    netWriter.Write(data);
                }
                catch (IOException ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception while writing data: {0}", ex);
                }
            }
        }

        public override IPAddress GetLocalIP()
        {
            lock (netLock)
            {
                if (socket == null)
                {
                    return IPAddress.None;
                }

                try
                {
                    return NetHelpers.GetLocalIP(socket);
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("TcpConnection", "Socket exception trying to read bound IP: {0}", ex);
                    return IPAddress.None;
                }
            }
        }

        public override void SetNetEncryptionFilter(NetFilterEncryption filter)
        {
            lock (netLock)
            {
                if (socket != null)
                {
                    netFilter = filter;
                }
            }
        }
    }
}
