using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class SteamGameServerFacts : HandlerTestBase<SteamGameServer>
    {
        [TestMethod]
        public void LogOnPostsLoggedOnCallbackWhenNoConnection()
        {
            Handler.LogOn(new SteamGameServer.LogOnDetails
            {
                Token = "SuperSecretToken"
            });

            var callback = SteamClient.GetCallback( freeLast: true );
            Assert.IsNotNull( callback );
            Assert.IsInstanceOfType<SteamUser.LoggedOnCallback>( callback );

            var loc = (SteamUser.LoggedOnCallback)callback;
            Assert.AreEqual( EResult.NoConnection, loc.Result );
        }

        [TestMethod]
        public void LogOnAnonymousPostsLoggedOnCallbackWhenNoConnection()
        {
            Handler.LogOnAnonymous();

            var callback = SteamClient.GetCallback( freeLast: true );
            Assert.IsNotNull( callback );
            Assert.IsInstanceOfType<SteamUser.LoggedOnCallback>( callback );

            var loc = (SteamUser.LoggedOnCallback)callback;
            Assert.AreEqual( EResult.NoConnection, loc.Result );
        }
    }
}
