using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class SteamUserFacts : HandlerTestBase<SteamUser>
    {
        [TestMethod]
        public void LogOnPostsLoggedOnCallbackWhenNoConnection()
        {
            Handler.LogOn(new SteamUser.LogOnDetails
            {
                Username = "iamauser",
                Password = "lamepassword"
            });

            var callback = SteamClient.GetCallback( freeLast: true );
            Assert.IsNotNull( callback );
            Assert.IsInstanceOfType<SteamUser.LoggedOnCallback>( callback );

            var loc = (SteamUser.LoggedOnCallback)callback;
            Assert.AreEqual( EResult.NoConnection, loc.Result );
        }

        [TestMethod]
        public void LogOnThrowsExceptionIfDetailsNotProvided()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                Handler.LogOn(null);
            });
        }

        [TestMethod]
        public void LogOnThrowsExceptionIfUsernameNotProvided_OnlyPassword()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Password = "def"
                });
            });
        }

        [TestMethod]
        public void LogOnThrowsExceptionIfUsernameNotProvided_OnlyAccessToken()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    AccessToken = "def"
                });
            });
        }

        [TestMethod]
        public void LogOnThrowsExceptionIfPasswordAndAccessTokenNotProvided()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                Handler.LogOn(new SteamUser.LogOnDetails
                {
                    Username = "abc"
                });
            });
        }

        [TestMethod]
        public void LogOnDoesNotThrowExceptionIfUserNameAndPasswordProvided()
        {
            Handler.LogOn(new SteamUser.LogOnDetails
            {
                Username = "abc",
                Password = "def"
            });

            Assert.IsTrue(true);
        }
        
        [TestMethod]
        public void LogOnDoesNotThrowExceptionIfUserNameAndAccessTokenProvided()
        {
            Handler.LogOn(new SteamUser.LogOnDetails
            {
                Username = "abc",
                AccessToken = "def",
                ShouldRememberPassword = true,
            });

            Assert.IsTrue(true);
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
