using Xunit;
using SteamKit2;

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
                Assert.Equal( "msg{0}msg", msg);
            } );

            DebugLog.WriteLine( "category", "msg{0}msg" );
        }
    }
}
