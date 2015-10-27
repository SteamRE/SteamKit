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
        public void TryMergeWithList_AddsToHead_AndMovesExisting()
        {
            var seedList = new[]
            {
                new IPEndPoint( IPAddress.Loopback, 27025 ),
                new IPEndPoint( IPAddress.Loopback, 27035 ),
                new IPEndPoint( IPAddress.Loopback, 27045 ),
                new IPEndPoint( IPAddress.Loopback, 27105 ),
            };
            var seeded = serverList.TryAddRange( seedList );
            Assert.True( seeded, "Sanity check" );

            var listToMerge = new[]
            {
                new IPEndPoint( IPAddress.Loopback, 27015 ),
                new IPEndPoint( IPAddress.Loopback, 27035 ),
                new IPEndPoint( IPAddress.Loopback, 27105 ),
            };

            serverList.MergeWithList( listToMerge );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 5, addresses.Length );
            Assert.Equal( listToMerge[ 0 ], addresses[ 0 ] );
            Assert.Equal( listToMerge[ 1 ], addresses[ 1 ] );
            Assert.Equal( seedList[ 1 ], addresses[ 1 ] );
            Assert.Equal( listToMerge[ 2 ], addresses[ 2 ] );
            Assert.Equal( seedList[ 3 ], addresses[ 2 ] );
            Assert.Equal( seedList[ 0 ], addresses[ 3 ] );
            Assert.Equal( seedList[ 2 ], addresses[ 4 ] );
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

        [Fact]
        public void GetNextServerCandidate_IsBiasedTowardsServerOrdering()
        {
            var goodEndPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            var neutralEndPoint = new IPEndPoint(IPAddress.Loopback, 27016);
            var badEndPoint = new IPEndPoint(IPAddress.Loopback, 27017);

            serverList.TryAdd( badEndPoint );
            serverList.TryAdd( neutralEndPoint );
            serverList.TryAdd( goodEndPoint );

            serverList.TryMark( badEndPoint, ServerQuality.Bad );
            serverList.TryMark( goodEndPoint, ServerQuality.Good );

            var nextServerCandidate = serverList.GetNextServerCandidate();
            Assert.Equal( neutralEndPoint, nextServerCandidate );

            serverList.TryMark( badEndPoint, ServerQuality.Good);

            nextServerCandidate = serverList.GetNextServerCandidate();
            Assert.Equal( badEndPoint, nextServerCandidate );
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
