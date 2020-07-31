using System;
using System.IO;
using System.Text;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class SimpleConsoleDebugListenerFacts : IDisposable
    {
        public SimpleConsoleDebugListenerFacts()
        {
            originalOutWriter = Console.Out;
            sb = new StringBuilder();
            writer = new StringWriter(sb);
            Console.SetOut(writer);
        }

        readonly StringBuilder sb;
        readonly StringWriter writer;
        readonly TextWriter originalOutWriter;

        [Fact]
        public void WritesOutputToConsole()
        {
            var listener = new SimpleConsoleDebugListener();
            listener.WriteLine("some cat", "a message");
            writer.Flush();

            Assert.Equal("[some cat]: a message", sb.ToString().TrimEnd());
        }

        public void Dispose()
        {
            Console.SetOut(originalOutWriter);
            writer.Dispose();
        }
    }
}
