using System;
using System.Diagnostics;

namespace Tests
{
    class TraceAssertListener : TraceListener
    {
        public override void Fail( string message, string detailMessage )
        {
            throw new TraceAssertException( message, detailMessage );
        }

        public override void Write( string message )
        {
        }

        public override void WriteLine( string message )
        {
        }
    }

    class TraceAssertException : Exception
    {
        public string AssertMessage { get; private set; }
        public string AssertDetailedMessage { get; private set; }


        public TraceAssertException( string message, string detailMessage )
        {
            AssertMessage = message;
            AssertDetailedMessage = detailMessage;
        }
    }
}
