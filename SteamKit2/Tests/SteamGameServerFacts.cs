using SteamKit2;
using Xunit;

namespace Tests
{
    public class SteamGameServerFacts : HandlerTestBase<SteamGameServer>
    {
        [Fact]
        public void LogOnPostsLoggedOnCallbackWhenNoConnection()
        {
            var asyncJob = Handler.LogOn(new SteamGameServer.LogOnDetails
            {
                Token = "SuperSecretToken"
            });

            var callback = SteamClient.GetCallback( );
            Assert.NotNull( callback );
            Assert.IsType<SteamUser.LoggedOnCallback>( callback );

            var loc = (SteamUser.LoggedOnCallback)callback;
            Assert.Equal( EResult.NoConnection, loc.Result );
            Assert.Equal( asyncJob.JobID, loc.JobID );
        }

        [Fact]
        public void LogOnAnonymousPostsLoggedOnCallbackWhenNoConnection()
        {
            var asyncJob = Handler.LogOnAnonymous();

            var callback = SteamClient.GetCallback( );
            Assert.NotNull( callback );
            Assert.IsType<SteamUser.LoggedOnCallback>( callback );

            var loc = (SteamUser.LoggedOnCallback)callback;
            Assert.Equal( EResult.NoConnection, loc.Result );
            Assert.Equal( asyncJob.JobID, loc.JobID );
        }
    }
}
