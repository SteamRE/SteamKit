/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SteamKit2
{

    class TcpConnection : Connection
    {
        const uint MAGIC = 0x31305456;

        Socket sock;

        Thread netThread;

        NetworkStream stream;
        BinaryReader reader;
        BinaryWriter writer;

        public override void Connect(IPEndPoint endPoint)
        {
            Disconnect();

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(endPoint);

            stream = new NetworkStream(sock, true);
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);

            netThread = new Thread(NetLoop);
            netThread.Start();
        }

        public override void Disconnect()
        {
            if (sock == null || !sock.Connected)
                return;

            if (sock != null)
            {
                try
                {
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Disconnect(true);
                    sock.Close();

                    sock = null;
                }
                catch { }
            }
        }

        public override void Send(IClientMsg clientMsg)
        {
            if (!sock.Connected)
                return;

            byte[] data = clientMsg.Serialize();

            if (NetFilter != null)
            {
                data = NetFilter.ProcessOutgoing(data);
            }

            ByteBuffer bb = new ByteBuffer(data.Length + 8);
            bb.Append((uint)data.Length);
            bb.Append(TcpConnection.MAGIC);
            bb.Append(data);

            writer.Write(bb.ToArray());
        }

        void NetLoop()
        {
            try
            {
                while (sock.Connected)
                {
                    byte[] packetHeader = reader.ReadBytes(8);

                    if (packetHeader.Length != 8)
                        throw new IOException("Connection lost");

                    DataStream ds = new DataStream(packetHeader);
                    uint packetLen = ds.ReadUInt32();
                    uint packetMagic = ds.ReadUInt32();

                    if (packetMagic != TcpConnection.MAGIC)
                        throw new IOException("RecvCompleted got a packet with invalid magic!");

                    byte[] packData = reader.ReadBytes((int)packetLen);
                    if (packData.Length != packetLen)
                        throw new IOException("Connection lost");

                    if (NetFilter != null)
                        packData = NetFilter.ProcessIncoming(packData);

                    OnNetMsgReceived(new NetMsgEventArgs(packData));
                }
            }
            catch (Exception e)
            {
                DebugLog.WriteLine("TcpConnection", e.ToString());
            }
        }

        public override IPAddress GetLocalIP()
        {
            return NetHelpers.GetLocalIP(sock);
        }
    }
}
