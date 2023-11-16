using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class StreamHelpersFacts
    {
        [Fact]
        public void IsThreadSafe()
        {
            const int NumConcurrentThreads = 200;
            var threads = new Thread[NumConcurrentThreads];
            
            for ( var i = 0; i < threads.Length; i++)
            {
                threads[ i ] = new Thread( ThreadStart )
                {
                    Name = $"SK2-Test-{i}"
                };
                threads[i].Start(i);
            }

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            if (!threadExceptions.IsEmpty)
            {
                throw new AggregateException(threadExceptions);
            }
        }

        ConcurrentBag<Exception> threadExceptions = [];

        void ThreadStart(object o)
        {
            try
            {
                var threadNumber = (int)o;

                using var ms = new MemoryStream();
                var bytes = BitConverter.GetBytes( threadNumber );
                ms.Write( bytes, 0, bytes.Length );

                for ( var i = 0; i < 1000; i++ )
                {
                    ms.Seek( 0, SeekOrigin.Begin );

                    var value = ReadValue( threadNumber, ms ).ToString( CultureInfo.InvariantCulture );
                    Assert.Equal( threadNumber.ToString( CultureInfo.InvariantCulture ), value );
                }
            }
            catch (Exception ex)
            {
                threadExceptions.Add(ex);
            }

            static IConvertible ReadValue(int threadNumber, Stream s)
            {
                return ( threadNumber % 7 ) switch
                {
                    0 => s.ReadByte(),
                    1 => s.ReadInt64(),
                    2 => s.ReadUInt16(),
                    3 => s.ReadInt32(),
                    4 => s.ReadUInt32(),
                    5 => s.ReadInt64(),
                    6 => s.ReadUInt64(),
                    _ => throw new UnreachableException(),
                };
            }
        }
    }
}
