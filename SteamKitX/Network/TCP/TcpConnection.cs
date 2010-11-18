using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SteamKit
{
    // handle TCP
    class TcpConnection : Connection
    {
        public static readonly UInt32 Magic = 0x31305456;

        private Socket netSocket;

        private SocketAsyncEventArgs asyncArgs;
        private byte[] recvBuffer;

        private IPEndPoint localEndPoint;
        private IPEndPoint targetEndPoint;

        private Dictionary<uint, NetPacket> packetMap;

        public TcpConnection()
        {
            netSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            targetEndPoint = null;
            asyncArgs = null;

            recvBuffer = new byte[1500];
            localEndPoint = new IPEndPoint(IPAddress.Any, 0);

            netSocket.Bind(localEndPoint);

            packetMap = new Dictionary<uint, NetPacket>();
        }

        private void StartReceive()
        {
            if (asyncArgs == null)
            {
                asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                asyncArgs.SetBuffer(recvBuffer, 0, recvBuffer.Length);
            }

            bool completedAsync = netSocket.ReceiveAsync(asyncArgs);

            if (!completedAsync)
            {
                IOCompleted(null, asyncArgs);
            }
        }

        private void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive)
            {
                throw new NotImplementedException();
            }

            HandleReceivedPacket(e);

            StartReceive();
        }

        private void HandleReceivedPacket(SocketAsyncEventArgs e)
        {
            MemoryStream ms = new MemoryStream(recvBuffer);
            BinaryReader reader = new BinaryReader(ms);

            UInt32 length = reader.ReadUInt32();
            UInt32 magic = reader.ReadUInt32();

            ms.SetLength(length + 8);

            if (magic != Magic)
            {
                Console.WriteLine("Invalid TcpPacket received");
                return;
            }

            byte[] buffer = new byte[length];
            ms.Read(buffer, 0, (int)length);

            ms = new MemoryStream(buffer);

            if (netFilter != null)
            {
                try
                {
                    ms = netFilter.ProcessIncoming(ms);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Encryption error in packet " + ex.Message);
                    return;
                }
            }

            OnNetMsgReceived(new DataEventArgs((IPEndPoint)e.RemoteEndPoint, ms));
        }


        public override void SetTargetEndPoint(IPEndPoint remoteEndPoint)
        {
            targetEndPoint = remoteEndPoint;
        }

        public override void SendConnect(UInt32 challengeValue)
        {
            netSocket.Connect(targetEndPoint);
            StartReceive();
        }

        public override void SendMessage(IClientMsg clientmsg)
        {
            MemoryStream data = clientmsg.serialize();

            if (netFilter != null)
            {
                data = new MemoryStream(netFilter.ProcessOutgoing(data.ToArray()));
            }

            MemoryStream ms = new MemoryStream((int)data.Length + 8);

            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((UInt32)data.Length);
            writer.Write(Magic);

            data.CopyTo(ms);

            byte[] buf = ms.ToArray();

            Console.WriteLine("Sending message " + clientmsg.GetType());

            netSocket.Send(buf);
        }

        public override IPAddress GetLocalIP()
        {
            return NetHelper.GetLocalIP(netSocket);
        }
    }
}