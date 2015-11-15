using System.Net;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class SmartCMServerListFacts
    {
        public SmartCMServerListFacts()
        {
            serverList = new SmartCMServerList();
        }

        readonly SmartCMServerList serverList;
        
        [Fact]
        public void TryAdd_ReturnsTrue_WhenAdded()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            var added = serverList.TryAdd( endPoint );
            Assert.True( added, "TryAdd should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 1, addresses.Length );
            Assert.Equal( endPoint, addresses[ 0 ] );
        }

        [Fact]
        public void TryAddRange_ReturnsTrue_WhenAdded()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            var added = serverList.TryAddRange( new[] { endPoint } );
            Assert.True( added, "TryAddRange should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 1, addresses.Length );
            Assert.Equal( endPoint, addresses[ 0 ] );
        }

        [Fact]
        public void TryAdd_ReturnsFalse_WhenEqualEndPointAlreadyAdded()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.TryAdd( endPoint );

            var added = serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            Assert.False( added, "TryAdd should not have added the equal IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 1, addresses.Length );
            Assert.Equal( endPoint, addresses[ 0 ] );
        }

        [Fact]
        public void TryAddRange_ReturnsFalse_WhenAnyEqualEndPointAlreadyAdded()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.TryAdd( endPoint );

            var added = serverList.TryAddRange( new[] { new IPEndPoint( IPAddress.Loopback, 27015 ), new IPEndPoint( IPAddress.Loopback, 27016 ) } );
            Assert.False( added, "TryAddRange should not have added the equal IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 1, addresses.Length );
            Assert.Equal( endPoint, addresses[ 0 ] );
        }

        [Fact]
        public void TryAdd_ReturnsTrue_AddingEndPointWithDifferentAddress()
        {
            serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );

            var endPoint = new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27015 );

            var added = serverList.TryAdd( endPoint );
            Assert.True( added, "TryAdd should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 2, addresses.Length );
            Assert.Equal( endPoint, addresses[ 1 ] );
        }
        
        [Fact]
        public void TryAddRange_ReturnsTrue_AddingEndPointWithDifferentAddress()
        {
            serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );

            var endPoint = new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27015 );

            var added = serverList.TryAddRange( new[] { endPoint } );
            Assert.True( added, "TryAddRange should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 2, addresses.Length );
            Assert.Equal( endPoint, addresses[ 1 ] );
        }

        [Fact]
        public void TryAdd_ReturnsTrue_AddingEndPointWithDifferentPort()
        {
            serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );

            var endPoint = new IPEndPoint( IPAddress.Loopback, 27016 );

            var added = serverList.TryAdd( endPoint );
            Assert.True( added, "TryAdd should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 2, addresses.Length );
            Assert.Equal( endPoint, addresses[ 1 ] );
        }

        [Fact]
        public void TryAddRange_ReturnsTrue_AddingEndPointWithDifferentPort()
        {
            serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );

            var endPoint = new IPEndPoint( IPAddress.Loopback, 27016 );

            var added = serverList.TryAddRange( new[] { endPoint } );
            Assert.True( added, "TryAddRange should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 2, addresses.Length );
            Assert.Equal( endPoint, addresses[ 1 ] );
        }

        [Fact]
        public void TryAdd_ReturnsTrue_AddingEndPointWithDifferentAddressAndPort()
        {
            serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );

            var endPoint = new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27016 );

            var added = serverList.TryAdd( endPoint );
            Assert.True( added, "TryAdd should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 2, addresses.Length );
            Assert.Equal( endPoint, addresses[ 1 ] );
        }

        [Fact]
        public void TryAddRange_ReturnsTrue_AddingEndPointWithDifferentAddressAndPort()
        {
            serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );

            var endPoint = new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27016 );

            var added = serverList.TryAddRange( new[] { endPoint } );
            Assert.True( added, "TryAddRange should have added the IPEndPoint to the list." );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 2, addresses.Length );
            Assert.Equal( endPoint, addresses[ 1 ] );
        }

        [Fact]
        public void TryAdd_ReturnsTrue_AddingAVarietyOfEndPointsWithDifferentAddressAndPort()
        {
            serverList.TryAdd(new IPEndPoint(IPAddress.Loopback, 27015));
            Assert.Equal(1, serverList.GetAllEndPoints().Length);

            var endPoints = new[]
            {
                new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27015 ),
                new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27016 ),
                new IPEndPoint( IPAddress.Parse( "192.168.1.1" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "10.10.0.1" ), 27017 )
            };

            var added = serverList.TryAddRange(endPoints);
            Assert.True(added, "TryAddRange should have added the IPEndPoints to the list.");
            Assert.Equal( 5, serverList.GetAllEndPoints().Length );
        }

        [Fact]
        public void TryAddRange_ReturnsTrue_AddingDuplicateEndPointsInSingleRange()
        {
            serverList.TryAdd( new IPEndPoint( IPAddress.Loopback, 27015 ) );

            var endPoints = new[]
            {
                new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27015 ),
                new IPEndPoint( IPAddress.Parse( "192.168.0.1" ), 27015 )
            };

            var added = serverList.TryAddRange( endPoints );
            Assert.True( added, "TryAddRange should have added the IPEndPoints to the list." );
            Assert.Equal( 2, serverList.GetAllEndPoints().Length );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsNull_IfListIsEmpty()
        {
            var endPoint = serverList.GetNextServerCandidate();
            Assert.Null( endPoint );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.TryAdd( endPoint );

            var nextEndPoint = serverList.GetNextServerCandidate();
            Assert.Equal( endPoint, nextEndPoint );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers_EvenIfAllServersAreBad()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.TryAdd( endPoint );
            serverList.TryMark( endPoint, ServerQuality.Bad );

            var nextEndPoint = serverList.GetNextServerCandidate();
            Assert.Equal( endPoint, nextEndPoint );
        }

        // Warning: This test is dependent on random values from the system and may, if you are unlucky enough, falsely report a failure.
        [Fact]
        public void GetNextServerCandidate_IsBiasedTowardsGoodServers()
        {
            var goodEndPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            var neutralEndPoint = new IPEndPoint( IPAddress.Loopback, 27016 );
            var badEndPoint = new IPEndPoint( IPAddress.Loopback, 27017 );

            serverList.TryAdd( goodEndPoint );
            serverList.TryAdd( neutralEndPoint );
            serverList.TryAdd( badEndPoint );

            const int numTimesToMark = 5;

            for( int i = 0; i < numTimesToMark; i++ )
            {
                serverList.TryMark( goodEndPoint, ServerQuality.Good );
                serverList.TryMark(badEndPoint, ServerQuality.Bad);
            }

            var numTimesGotGoodServer = 0;
            var numTimesGotNeutralServer = 0;
            var numTimesGotBadServer = 0;

            const int numTimesToGetServer = 1000000;

            for ( int i = 0; i < numTimesToGetServer; i++ )
            {
                var nextServer = serverList.GetNextServerCandidate();

                if ( nextServer == goodEndPoint )
                {
                    numTimesGotGoodServer++;
                }
                else if ( nextServer == neutralEndPoint )
                {
                    numTimesGotNeutralServer++;
                }
                else if ( nextServer == badEndPoint )
                {
                    numTimesGotBadServer++;
                }
                else
                {
                    Assert.True( false, "Got server that was not added to the server list." );
                }
            }

            Assert.True( numTimesGotGoodServer > numTimesGotNeutralServer, "Should get good servers more times than neutral servers" );
            Assert.True( numTimesGotGoodServer > numTimesGotBadServer, "Should get good servers more times than bad servers" );
            Assert.True( numTimesGotNeutralServer > numTimesGotBadServer, "Should get neutral servers more times than bad servers" );
        }

        [Fact]
        public void TryMark_ReturnsTrue_IfServerInList()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.TryAdd( endPoint );

            var marked = serverList.TryMark( endPoint, ServerQuality.Good );
            Assert.True( marked );
        }

        [Fact]
        public void TryMark_ReturnsFalse_IfServerNotInList()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.TryAdd( endPoint );

            var marked = serverList.TryMark( new IPEndPoint( IPAddress.Loopback, 27016 ), ServerQuality.Good );
            Assert.False( marked );
        }

        [Fact]
        public void Clear_RemovesAllServers()
        {
            for (int i = 0; i < 20; i++)
            {
                serverList.TryAdd(new IPEndPoint(IPAddress.Loopback, 27015 + i));
            }
            Assert.Equal(20, serverList.GetAllEndPoints().Length);

            serverList.Clear();

            Assert.Equal(0, serverList.GetAllEndPoints().Length);
        }
    }
}
