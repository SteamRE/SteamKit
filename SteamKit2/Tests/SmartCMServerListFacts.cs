using System.Net;
using SteamKit2;
using Xunit;
using SteamKit2.Discovery;
using System.Collections.Generic;

namespace Tests
{
    public class SmartCMServerListFacts
    {
        public SmartCMServerListFacts()
        {
            serverList = new SmartCMServerList(new NullServerListProvider(), allowDirectoryFetch: false);
        }

        readonly SmartCMServerList serverList;
        
        [Fact]
        public void TryMergeWithList_AddsToHead_AndMovesExisting()
        {
            serverList.GetAllEndPoints();

            var seedList = new[]
            {
                new IPEndPoint( IPAddress.Loopback, 27025 ),
                new IPEndPoint( IPAddress.Loopback, 27035 ),
                new IPEndPoint( IPAddress.Loopback, 27045 ),
                new IPEndPoint( IPAddress.Loopback, 27105 ),
            };
            serverList.ReplaceList( seedList );
            Assert.Equal( 4, seedList.Length );

            var listToReplace = new[]
            {
                new IPEndPoint( IPAddress.Loopback, 27015 ),
                new IPEndPoint( IPAddress.Loopback, 27035 ),
                new IPEndPoint( IPAddress.Loopback, 27105 ),
            };

            serverList.ReplaceList( listToReplace );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 3, addresses.Length );
            Assert.Equal( listToReplace[ 0 ], addresses[ 0 ] );
            Assert.Equal( listToReplace[ 1 ], addresses[ 1 ] );
            Assert.Equal( listToReplace[ 2 ], addresses[ 2 ] );
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
            serverList.GetAllEndPoints();

            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.ReplaceList( new List<IPEndPoint>() { endPoint } );

            var nextEndPoint = serverList.GetNextServerCandidate();
            Assert.Equal( endPoint, nextEndPoint );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers_EvenIfAllServersAreBad()
        {
            serverList.GetAllEndPoints();

            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.ReplaceList( new List<IPEndPoint>() { endPoint } );
            serverList.TryMark( endPoint, ServerQuality.Bad );

            var nextEndPoint = serverList.GetNextServerCandidate();
            Assert.Equal( endPoint, nextEndPoint );
        }

        [Fact]
        public void GetNextServerCandidate_IsBiasedTowardsServerOrdering()
        {
            serverList.GetAllEndPoints();

            var goodEndPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            var neutralEndPoint = new IPEndPoint(IPAddress.Loopback, 27016);
            var badEndPoint = new IPEndPoint(IPAddress.Loopback, 27017);

            serverList.ReplaceList( new List<IPEndPoint>() { badEndPoint, neutralEndPoint, goodEndPoint } );

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
            serverList.ReplaceList( new List<IPEndPoint>() { endPoint } );

            var marked = serverList.TryMark( endPoint, ServerQuality.Good );
            Assert.True( marked );
        }

        [Fact]
        public void TryMark_ReturnsFalse_IfServerNotInList()
        {
            var endPoint = new IPEndPoint( IPAddress.Loopback, 27015 );
            serverList.ReplaceList( new List<IPEndPoint>() { endPoint } );

            var marked = serverList.TryMark( new IPEndPoint( IPAddress.Loopback, 27016 ), ServerQuality.Good );
            Assert.False( marked );
        }
    }
}
