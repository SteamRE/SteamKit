using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [DoNotParallelize]
    [TestClass]
    public class SimpleConsoleDebugListenerFacts
    {
        [TestMethod]
        public void WritesOutputToConsole()
        {
            var originalOutWriter = Console.Out;

            try
            {
                var sb = new StringBuilder();
                using var writer = new StringWriter( sb );
                Console.SetOut( writer );

                var listener = new SimpleConsoleDebugListener();
                listener.WriteLine( "some cat", "a message" );
                writer.Flush();

                Assert.AreEqual( "[some cat]: a message", sb.ToString().TrimEnd() );
            }
            finally
            {
                Console.SetOut( originalOutWriter );
            }
        }
    }
}
