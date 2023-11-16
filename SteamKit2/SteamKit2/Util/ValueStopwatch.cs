using System;
using System.Diagnostics;

namespace SteamKit2.Util
{
    readonly struct ValueStopwatch
    {
        private static readonly double s_timestampToTicks = TimeSpan.TicksPerSecond / ( double )Stopwatch.Frequency;

        private readonly long _startTimestamp;

        private ValueStopwatch( long startTimestamp )
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew() => new( Stopwatch.GetTimestamp() );

        public TimeSpan GetElapsedTime()
        {
            if ( _startTimestamp == 0 )
            {
                throw new InvalidOperationException( "An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time." );
            }

            long end = Stopwatch.GetTimestamp();
            long timestampDelta = end - _startTimestamp;
            long ticks = ( long )( s_timestampToTicks * timestampDelta );
            return new TimeSpan( ticks );
        }
    }
}
