using SteamKit2;
using Xunit;

namespace Tests
{
    public class SteamUserFacts : HandlerTestBase<SteamUser>
    {
        [Fact]
        public void LogOnPostsLoggedOnCallbackWhenNoConnection()
        {
            Handler.LogOn(new SteamUser.LogOnDetails
            {
                Username = "iamauser",
                Password = "lamepassword"
            });

            var callback = SteamClient.GetCallback( freeLast: true );
            Assert.NotNull( callback );
            Assert.IsType<SteamUser.LoggedOnCallback>( callback );

            var loc = (SteamUser.LoggedOnCallback)callback;
            Assert.Equal( EResult.NoConnection, loc.Result );
        }

        [Fact]
        public void LogOnAnonymousPostsLoggedOnCallbackWhenNoConnection()
        {
            Handler.LogOnAnonymous();

            var callback = SteamClient.GetCallback( freeLast: true );
            Assert.NotNull( callback );
            Assert.IsType<SteamUser.LoggedOnCallback>( callback );

            var loc = (SteamUser.LoggedOnCallback)callback;
            Assert.Equal( EResult.NoConnection, loc.Result );
        }
    }
}
