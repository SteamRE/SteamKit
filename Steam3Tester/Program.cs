using System;
using System.Collections.Generic;
using System.Text;
using Steam3Lib;
using System.Net;

namespace Steam3Tester
{
    class Program
    {
        static void Main( string[] args )
        {
            UdpConnection udpConn = new UdpConnection();

            udpConn.SendChallengeReq( new IPEndPoint( IPAddress.Parse( UdpConnection.CMServers[ 0 ] ), 27017 ) );


            udpConn.ChallengeReceived += ( obj, e ) =>
                {
                    Console.WriteLine( "Got challenge response. Load: " + e.Data.ServerLoad );

                    udpConn.SendConnect( e.Data.ChallengeValue, e.Sender );
                };

            udpConn.AcceptReceived += ( obj, e ) =>
                {
                    Console.WriteLine( "Connection accepted." );
                };
            udpConn.DataReceived += ( obj, e ) =>
                {
                    Console.WriteLine( "Got data!" );
                };
        }
    }
}
