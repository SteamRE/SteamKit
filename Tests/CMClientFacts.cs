using System;
using System.IO;
using SteamKit2;
using SteamKit2.Internal;
using Xunit;

namespace Tests
{
    public class CMClientFacts
    {
        [Fact]
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
                Assert.IsAssignableFrom<PacketMsg>(packetMsg);
            }
        }

        [Fact]
        public void GetPacketMsgReturnsPacketClientMsgProtobufForMessagesWithProtomask()
        {
            var msg = MsgUtil.MakeMsg(EMsg.ClientLogOnResponse, protobuf: true);
            var msgHdr = new MsgHdrProtoBuf { Msg = msg };

            var data = Serialize(msgHdr);
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.IsAssignableFrom<PacketClientMsgProtobuf>(packetMsg);
        }

        [Fact]
        public void GetPacketMsgReturnsPacketClientMsgForOtherMessages()
        {
            var msg = MsgUtil.MakeMsg(EMsg.ClientLogOnResponse, protobuf: false);
            var msgHdr = new ExtendedClientMsgHdr { Msg = msg };

            var data = Serialize(msgHdr);
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.IsAssignableFrom<PacketClientMsg>(packetMsg);
        }

        [Fact]
        public void GetPacketMsgFailsWithNull()
        {
            var msg = MsgUtil.MakeMsg(EMsg.ClientLogOnResponse, protobuf: true);
            var msgHdr = new MsgHdrProtoBuf { Msg = msg };

            var data = Serialize(msgHdr);
            Array.Copy(BitConverter.GetBytes(-1), 0, data, 4, 4);
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.Null(packetMsg);
        }

        [Fact]
        public void GetPacketMsgFailsWithTinyArray()
        {
            var data = new byte[3];
            var packetMsg = CMClient.GetPacketMsg(data, DebugLogContext.Instance);
            Assert.Null(packetMsg);
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
