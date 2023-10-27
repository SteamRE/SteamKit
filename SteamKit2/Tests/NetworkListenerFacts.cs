using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            Assert.IsNotNull(data);
            Assert.IsTrue( data.Length > 0 );
        }
    }


    [TestClass]
    public class NetworkListenerFacts
    {
        [TestMethod]
        public void NetworkListenerCallsOnOutgoingMessage()
        {
            var listener = new TestNetworkListener();

            var steamClient = new SteamClient();
            steamClient.DebugNetworkListener = listener;

            // Steam client has to be lied to because Send() has an assert for a connection
            typeof( CMClient ).GetProperty( "IsConnected" ).SetValue( steamClient, true, null );

            var clientMsg = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayedNoDataBlob);
            steamClient.Send(clientMsg);

            Assert.AreEqual(EMsg.ClientGamesPlayedNoDataBlob, listener.LastOutgoingMessage);
        }
    }
}
