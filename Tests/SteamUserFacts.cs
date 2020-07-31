using System;
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
        public void LogOnThrowsExceptionIfDetailsNotProvided()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Handler.LogOn(null);
            });
        }

        [Fact]
        public void LogOnThrowsExceptionIfUsernameNotProvided_OnlyPassword()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Password = "def"
                });
            });
        }

        [Fact]
        public void LogOnThrowsExceptionIfUsernameNotProvided_OnlyLoginKey()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    LoginKey = "def"
                });
            });
        }

        [Fact]
        public void LogOnThrowsExceptionIfUsernameNotProvided_OnlyLoginKey_ShouldRememberPassword()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    LoginKey = "def",
                    ShouldRememberPassword = true
                });
            });
        }

        [Fact]
        public void LogOnThrowsExceptionIfPasswordAndLoginKeyNotProvided()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Username = "abc"
                });
            });
        }

        [Fact]
        public void LogOnThrowsExceptionIfLoginKeyProvidedWithoutShouldRememberPassword()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Username = "abc",
                    LoginKey = "def"
                });
            });
        }
        
        [Fact]
        public void LogOnDoesNotThrowExceptionIfUserNameAndPasswordProvided()
        {
            var ex = Record.Exception(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Username = "abc",
                    Password = "def"
                });
            });

            Assert.Null( ex );
        }
        
        [Fact]
        public void LogOnDoesNotThrowExceptionIfUserNameAndLoginKeyProvided()
        {
            var ex = Record.Exception(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Username = "abc",
                    LoginKey = "def",
                    ShouldRememberPassword = true,
                });
            });

            Assert.Null( ex );
        }

        [Fact]
        public void LogOnThrowsExceptionIfLoginKeyIsProvidedWithoutShouldRememberPassword()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Username = "abc",
                    LoginKey = "def"
                });
            });
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
