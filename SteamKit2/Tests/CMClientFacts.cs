using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using SteamKit2.Internal;

namespace Tests
{
    [TestClass]
    public class CMClientFacts
    {
        [TestMethod]
        public void GetPacketMsgReturnsPacketMsgForCryptoHandshake()
        {
            var messages = new[]
            {
                EMsg.ChannelEncryptRequest,
                EMsg.ChannelEncryptResponse,
                EMsg.ChannelEncryptResult
            };

            foreach (var emsg in messages)
            {
                var msgHdr = new MsgHdr { Msg = emsg };

                var data = Serialize(msgHdr);

                var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
                Assert.IsInstanceOfType<PacketMsg>(packetMsg);
            }
        }

        [TestMethod]
        public void GetPacketMsgReturnsPacketClientMsgProtobufForMessagesWithProtomask()
        {
            var msg = MsgUtil.MakeMsg(EMsg.ClientLogOnResponse, protobuf: true);
            var msgHdr = new MsgHdrProtoBuf { Msg = msg };

            var data = Serialize(msgHdr);
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.IsInstanceOfType<PacketClientMsgProtobuf>( packetMsg );
        }

        [TestMethod]
        public void GetPacketMsgReturnsPacketClientMsgForOtherMessages()
        {
            var msg = MsgUtil.MakeMsg(EMsg.ClientLogOnResponse, protobuf: false);
            var msgHdr = new ExtendedClientMsgHdr { Msg = msg };

            var data = Serialize(msgHdr);
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.IsInstanceOfType<PacketClientMsg>( packetMsg );
        }

        [TestMethod]
        public void GetPacketMsgFailsWithNull()
        {
            var msg = MsgUtil.MakeMsg(EMsg.ClientLogOnResponse, protobuf: true);
            var msgHdr = new MsgHdrProtoBuf { Msg = msg };

            var data = Serialize(msgHdr);
            Array.Copy(BitConverter.GetBytes(-1), 0, data, 4, 4);
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.IsNull(packetMsg);
        }

        [TestMethod]
        public void GetPacketMsgFailsWithTinyArray()
        {
            var data = new byte[3];
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.IsNull(packetMsg);
        }

        static byte[] Serialize(ISteamSerializableHeader hdr)
        {
            using (var ms = new MemoryStream())
            {
                hdr.Serialize(ms);
                return ms.ToArray();
            }
        }

        class DummyCMClient : CMClient
        {
            public DummyCMClient()
                : base( SteamConfiguration.CreateDefault(), "Dummy" )
            {
            }

            public void DummyDisconnect()
            {
                Disconnect();
                OnClientDisconnected( true );
            }

            public void HandleClientMsg( IClientMsg clientMsg )
                => OnClientMsgReceived( GetPacketMsg( clientMsg.Serialize(), this ) );
        }
    }
}
