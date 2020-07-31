using Xunit;
using SteamKit2;
using SteamKit2.Internal;

namespace Tests
{
    class TestNetworkListener : IDebugNetworkListener
    {
        public EMsg LastOutgoingMessage;

        public void OnIncomingNetworkMessage(EMsg msgType, byte[] data)
        {
            //
        }

        public void OnOutgoingNetworkMessage(EMsg msgType, byte[] data)
        {
            LastOutgoingMessage = msgType;

            Assert.NotNull(data);
            Assert.NotEmpty(data);
        }
    }
    
    public class NetworkListenerFacts
    {
        [Fact]
        public void NetworkListenerCallsOnOutgoingMessage()
        {
            var listener = new TestNetworkListener();

            var steamClient = new SteamClient();
            steamClient.DebugNetworkListener = listener;

            var clientMsg = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayedNoDataBlob);
            steamClient.Send(clientMsg);

            Assert.Equal(EMsg.ClientGamesPlayedNoDataBlob, listener.LastOutgoingMessage);
        }
    }
}
