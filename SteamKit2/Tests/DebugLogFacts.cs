using Xunit;
using SteamKit2;
using Xunit.Sdk;

namespace Tests
{
    class TestListener : IDebugListener
    {
        public void WriteLine( string category, string msg )
        {
            Assert.Equal( "category", category );
            Assert.Equal( "msg", msg );
        }
    }

    class DebugLogSetupTeardownAttribute : BeforeAfterTestAttribute
    {
        public override void Before( System.Reflection.MethodInfo methodUnderTest )
        {
            DebugLog.ClearListeners();
        }

        public override void After( System.Reflection.MethodInfo methodUnderTest )
        {
            DebugLog.Enabled = false;
            DebugLog.ClearListeners();
        }
    }

    public class DebugLogFacts
    {
        [Fact, DebugLogSetupTeardownAttribute]
        public void DebugLogActionListenerLogsMessage()
        {
            DebugLog.Enabled = true;

            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.Equal( "category", category );
                Assert.Equal( "msg", msg );
            } );

            DebugLog.WriteLine( "category", "msg" );
        }

        [Fact, DebugLogSetupTeardownAttribute]
        public void DebugLogDebugListenerLogsMessage()
        {
            DebugLog.Enabled = true;

            DebugLog.AddListener( new TestListener() );

            DebugLog.WriteLine( "category", "msg" );
        }

        [Fact, DebugLogSetupTeardownAttribute]
        public void DebugLogDoesntLogWhenDisabled()
        {
            DebugLog.Enabled = false;

            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.True( false, "Listener action called when it shouldn't have been" );
            } );

            DebugLog.WriteLine( "category", "msg" );
        }

        [Fact, DebugLogSetupTeardownAttribute]
        public void DebugLogAddsAndRemovesListener()
        {
            var testListener = new TestListener();

            DebugLog.AddListener( testListener );

            Assert.Contains( testListener, DebugLog.listeners );

            DebugLog.RemoveListener( testListener );

            Assert.DoesNotContain( testListener, DebugLog.listeners );
        }

        [Fact, DebugLogSetupTeardown]
        public void DebugLogClearsListeners()
        {
            var testListener = new TestListener();

            DebugLog.AddListener( testListener );

            Assert.Contains( testListener, DebugLog.listeners );

            DebugLog.ClearListeners();

            Assert.DoesNotContain( testListener, DebugLog.listeners );
        }

        [Fact, DebugLogSetupTeardown]
        public void DebugLogCanWriteSafelyWithoutParams()
        {
            DebugLog.Enabled = true;
            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.Equal( "category", category );
                Assert.Equal( "msg{0}msg", msg );
            } );

            DebugLog.WriteLine( "category", "msg{0}msg" );
        }

        [Fact, DebugLogSetupTeardown]
        public void DebugLogFormatsParams()
        {
            DebugLog.Enabled = true;
            DebugLog.AddListener( ( category, msg ) =>
            {
                Assert.Equal( "category", category );
                Assert.Equal( "msg1msg2", msg );
            } );

            var msgText = "msg";
            var integer = 2;
            DebugLog.WriteLine( "category", "msg{0}{1}{2}", 1, msgText, integer );
        }

        [Fact, DebugLogSetupTeardown]
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
            Assert.Equal( client.ID + "/MyCategory", category );
            Assert.Equal( "My 1st message", message );
        }

        [Fact, DebugLogSetupTeardown]
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
            Assert.Equal( "My Custom Client/MyCategory", category );
            Assert.Equal( "My 1st message", message );
        }
    }
}
