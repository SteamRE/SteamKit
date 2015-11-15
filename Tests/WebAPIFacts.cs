using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests
{
    public class WebAPIFacts
    {
        [Fact]
        public void WebAPIHasDefaultTimeout()
        {
            var iface = WebAPI.GetInterface( "ISteamWhatever" );

            Assert.Equal( iface.Timeout, 1000 * 100 );
        }
    }
}
