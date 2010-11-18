using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using Classless.Hasher;

namespace SteamKit
{
    class CMServer
    {
        public IPEndPoint EndPoint;
        public uint ServerLoad;

        public uint Challenge;

        public CMServer()
        {
            EndPoint = null;
            ServerLoad = uint.MaxValue;
            Challenge = 0;
        }
    }

    public class CMInterface
    {
        private Connection connection;

        private SteamSession session;

        private CMServer cmServer;
        private bool selected;

        byte[] tempSessionKey;

        private object netLock = new object();

        public delegate void NetMsgHandlerDelegate(MemoryStream data);
        private MultiMap<EMsg, NetMsgHandlerDelegate> msgHandlers;

        public CMInterface(SteamSession session)
        {
            this.session = session;

            connection = new UdpConnection();

            cmServer = new CMServer();
            selected = false;

            msgHandlers = new MultiMap<EMsg, NetMsgHandlerDelegate>();

            connection.ChallengeReceived += RecvChallenge;
            connection.DisconnectReceived += RecvDisconnect;
            connection.AcceptReceived += RecvAccept;
            connection.NetMsgReceived += RecvNetMsg;

            RegisterNetMsg(EMsg.ChannelEncryptRequest, HandleEncryptRequest);
            RegisterNetMsg(EMsg.ChannelEncryptResult, HandleEncryptResult);
            RegisterNetMsg(EMsg.Multi, HandleMultiMsg);

            // challenge all CM servers now
            if (connection is UdpConnection)
            {
                UdpConnection udpConn = connection as UdpConnection;

                foreach (string server in Common.CMServers)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(server), 27017);
                    udpConn.SendChallengeRequest(endPoint);
                }
            }
            else
            {
                cmServer.EndPoint = new IPEndPoint(IPAddress.Parse(Common.CMServers[0]), 27017);
            }
        }

        public void RegisterNetMsg(EMsg msg, NetMsgHandlerDelegate handler)
        {
            msgHandlers.Add(msg, handler);
        }

        public void ConnectToCM()
        {
            for (; ; )
            {
                lock (netLock)
                {
                    if (cmServer.EndPoint != null)
                    {
                        selected = true;
                        break;
                    }
                }

                Console.WriteLine("Waiting for CM..");
                Thread.Sleep(500);
            }

            connection.SetTargetEndPoint(cmServer.EndPoint);
            connection.SendConnect(cmServer.Challenge);
        }

        void RecvNetMsg(object sender, DataEventArgs e)
        {
            byte[] peek = new byte[4];
            e.Data.Read(peek, 0, peek.Length);
            e.Data.Seek(-4, SeekOrigin.Current);

            EMsg eMsg = (EMsg)BitConverter.ToUInt32(peek, 0);
            HandleNetMsg(eMsg, e.Data);
        }

        void HandleNetMsg(EMsg eMsg, MemoryStream data)
        {
            Console.WriteLine("Got EMsg: " + MsgUtil.GetMsg(eMsg) + " (Proto:" + MsgUtil.IsProtoBuf(eMsg) + ")");

            foreach (NetMsgHandlerDelegate del in msgHandlers[MsgUtil.GetMsg(eMsg)])
            {
                del(data);
            }
        }

        void RecvChallenge(object sender, ChallengeEventArgs e)
        {
            if (e.Data.ServerLoad < cmServer.ServerLoad && !selected)
            {
                lock (netLock)
                {
                    cmServer.EndPoint = e.Sender;
                    cmServer.ServerLoad = e.Data.ServerLoad;
                    cmServer.Challenge = e.Data.ChallengeValue;

                    Console.WriteLine(string.Format("New CM best! Server: {0}. Load: {1}", cmServer.EndPoint, cmServer.ServerLoad));
                }
            }
        }

        void RecvAccept(object sender, NetworkEventArgs e)
        {
            Console.WriteLine("Connection accepted!");
        }

        void RecvDisconnect(object sender, NetworkEventArgs e)
        {
            Console.WriteLine("Connection disconnected");
        }


        void SendNetMsg(IClientMsg msg)
        {
            connection.SendMessage(msg);
        }


        void HandleMultiMsg(MemoryStream data)
        {
            var msgMulti = new ClientMsgProtobuf<MsgMulti>(data);

            byte[] payload = msgMulti.Msg.Proto.message_body;

            if (msgMulti.Msg.Proto.size_unzipped > 0)
            {
                try
                {
                    payload = PKZipBuffer.Decompress(payload);
                }
                catch { return; }
            }

            MultiplexPayload(payload);
        }

        private void MultiplexPayload(byte[] payload)
        {
            MemoryStream ms = new MemoryStream(payload);
            BinaryReader reader = new BinaryReader(ms);

            while (ms.Position < ms.Length)
            {
                uint subSize = reader.ReadUInt32();
                byte[] subData = reader.ReadBytes((int)subSize);

                EMsg eMsg = (EMsg)BitConverter.ToUInt32(subData, 0);

                this.HandleNetMsg(eMsg, new MemoryStream(subData));
            }
        }

        void HandleEncryptRequest(MemoryStream data)
        {
            var encRequest = new ClientMsg<MsgChannelEncryptRequest, MsgHdr>(data);
            Console.WriteLine("Got encryption request for universe: " + encRequest.Msg.Universe);

            tempSessionKey = CryptoHelper.GenerateRandomBlock(32);

            var encResp = new ClientMsg<MsgChannelEncryptResponse, MsgHdr>();

            byte[] cryptedSessKey = CryptoHelper.RSAEncrypt(tempSessionKey, KeyDictionary.GetPublicKey(encRequest.Msg.Universe));
            Crc crc = new Crc();

            byte[] keyCrc = crc.ComputeHash(cryptedSessKey);
            Array.Reverse(keyCrc);

            BinaryWriter writer = new BinaryWriter(encResp.Payload);
            writer.Write(cryptedSessKey);
            writer.Write(keyCrc);
            writer.Write((uint)0);

            crc.Clear();

            SendNetMsg(encResp);
        }

        void HandleEncryptResult(MemoryStream data)
        {
            var encResult = new ClientMsg<MsgChannelEncryptResult, MsgHdr>(data);

            if (encResult.Msg.Result == EResult.OK)
                connection.SetNetFilter(new NetFilterEncryption(tempSessionKey));

            Console.WriteLine("Crypto result: " + encResult.Msg.Result);

            switch (session.Type)
            {
                case SessionType.GameServer:
                    {
                        SendAnonLogOn(session.RequestedID);
                    }
                    break;
                case SessionType.User:
                    {
                    }
                    break;
            }
        }


        void SendAnonLogOn(SteamID requested)
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            logon.ProtoHeader.client_steam_id = requested.ConvertToUint64();

            logon.Msg.Proto.obfustucated_private_ip = NetHelper.GetIPAddress(connection.GetLocalIP()) ^ MsgClientLogon.ObfuscationMask;
            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol; // default?
            logon.Msg.Proto.client_os_type = 10; // Windows

            connection.SendMessage(logon);
        }

    }
}
