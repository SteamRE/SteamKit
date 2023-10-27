using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    class TestListener : IDebugListener
    {
        public void WriteLine( string category, string msg )
        {
            Assert.AreEqual( "category", category );
            Assert.AreEqual( "msg", msg );
        }
    }

    [DoNotParallelize]
    [TestClass]
    public class DebugLogFacts
    {
        [TestInitialize]
        public void Startup()
        {
            DebugLog.ClearListeners();
        }

        [TestCleanup]
        public void Cleanup()
        {
            DebugLog.Enabled = false;
            DebugLog.ClearListeners();
        }

        [TestMethod]
        public void DebugLogActionListenerLogsMessage()
        {
            DebugLog.Enabled = true;

            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.AreEqual( "category", category );
                Assert.AreEqual( "msg", msg );
            } );

            DebugLog.WriteLine( "category", "msg" );
        }

        [TestMethod]
        public void DebugLogDebugListenerLogsMessage()
        {
            DebugLog.Enabled = true;

            DebugLog.AddListener( new TestListener() );

            DebugLog.WriteLine( "category", "msg" );
        }

        [TestMethod]
        public void DebugLogDoesntLogWhenDisabled()
        {
            DebugLog.Enabled = false;

            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.Fail( "Listener action called when it shouldn't have been" );
            } );

            DebugLog.WriteLine( "category", "msg" );
        }

        [TestMethod]
        public void DebugLogAddsAndRemovesListener()
        {
            var testListener = new TestListener();

            DebugLog.AddListener( testListener );

            Assert.IsTrue( DebugLog.listeners.Contains( testListener ) );

            DebugLog.RemoveListener( testListener );

            Assert.IsFalse( DebugLog.listeners.Contains( testListener ) );
        }

        [TestMethod]
        public void DebugLogClearsListeners()
        {
            var testListener = new TestListener();

            DebugLog.AddListener( testListener );

            Assert.IsTrue( DebugLog.listeners.Contains( testListener ) );

            DebugLog.ClearListeners();

            Assert.IsFalse( DebugLog.listeners.Contains( testListener ) );
        }

        [TestMethod]
        public void DebugLogCanWriteSafelyWithoutParams()
        {
            DebugLog.Enabled = true;
            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.AreEqual( "category", category );
                Assert.AreEqual( "msg{0}msg", msg );
            } );

            DebugLog.WriteLine( "category", "msg{0}msg" );
        }

        [TestMethod]
        public void DebugLogFormatsParams()
        {
            DebugLog.Enabled = true;
            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.AreEqual( "category", category );
                Assert.AreEqual( "msg1msg2", msg );
            } );

            var msgText = "msg";
            var integer = 2;
            DebugLog.WriteLine( "category", "msg{0}{1}{2}", 1, msgText, integer );
        }

        [TestMethod]
        public void GeneratedCMClientIDPrefixed()
        {
            DebugLog.Enabled = true;

            string category = default;
            string message = default;

            DebugLog.AddListener( ( cat, msg ) =>
            {
                category = cat;
                message = msg;
            } );

            var client = new SteamClient();
            client.LogDebug( "MyCategory", "My {0}st message", 1 );
            Assert.AreEqual( client.ID + "/MyCategory", category );
            Assert.AreEqual( "My 1st message", message );
        }

        [TestMethod]
        public void CustomCMClientIDPrefixed()
        {
            DebugLog.Enabled = true;

            string category = default;
            string message = default;

            DebugLog.AddListener( ( cat, msg ) =>
            {
                category = cat;
                message = msg;
            } );

            var client = new SteamClient("My Custom Client");
            client.LogDebug( "MyCategory", "My {0}st message", 1 );
            Assert.AreEqual( "My Custom Client/MyCategory", category );
            Assert.AreEqual( "My 1st message", message );
        }
    }
}
