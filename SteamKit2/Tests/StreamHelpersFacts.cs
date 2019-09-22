using System;
using System.Collections.Concurrent;
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
                threads[i] = new Thread(ThreadStart);
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

        ConcurrentBag<Exception> threadExceptions = new ConcurrentBag<Exception>();

        void ThreadStart(object o)
        {
            try
            {
                var threadNumber = (int)o;

                using (var ms = new MemoryStream())
                {
                    var bytes = BitConverter.GetBytes(threadNumber);
                    ms.Write(bytes, 0, bytes.Length);

                    for (var i = 0; i < 1000; i++)
                    {
                        ms.Seek(0, SeekOrigin.Begin);

                        var value = ReadValue(threadNumber, ms).ToString(CultureInfo.InvariantCulture);
                        Assert.Equal(threadNumber.ToString(CultureInfo.InvariantCulture), value);
                    }
                }
            }
            catch (Exception ex)
            {
                threadExceptions.Add(ex);
            }

            IConvertible ReadValue(int threadNumber, Stream s)
            {
                switch (threadNumber % 7)
                {
                    case 0: return s.ReadByte();
                    case 1: return s.ReadInt64();
                    case 2: return s.ReadUInt16();
                    case 3: return s.ReadInt32();
                    case 4: return s.ReadUInt32();
                    case 5: return s.ReadInt64();
                    case 6: return s.ReadUInt64();
                    default: throw new Exception("Unreachable");
                }
            }
        }
    }
}
