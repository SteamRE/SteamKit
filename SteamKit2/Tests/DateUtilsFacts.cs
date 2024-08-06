using System;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class DateUtilsFacts
    {
        [Fact]
        public void RoundtripsDatetimeUtc()
        {
            const ulong Timestamp = 1722966409;

            var date = DateUtils.DateTimeFromUnixTime( Timestamp );

            Assert.Equal( DateTimeKind.Utc, date.Kind );
            Assert.Equal( "2024-08-06T17:46:49.0000000Z", date.ToString( "o" ) );

            Assert.Equal( Timestamp, DateUtils.DateTimeToUnixTime( date ) );
        }
    }
}
