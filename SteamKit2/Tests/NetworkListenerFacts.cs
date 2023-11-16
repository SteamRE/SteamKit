using SteamKit2;
using SteamKit2.Internal;
using Xunit;

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

            // Steam client has to be lied to because Send() has an assert for a connection
            typeof( CMClient ).GetProperty( "IsConnected" ).SetValue( steamClient, true, null );

            var clientMsg = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayedNoDataBlob);
            steamClient.Send(clientMsg);

            Assert.Equal(EMsg.ClientGamesPlayedNoDataBlob, listener.LastOutgoingMessage);
        }
    }
}
