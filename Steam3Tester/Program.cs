using System;
using System.Collections.Generic;
using System.Text;
using SteamLib;
using System.Net;

namespace Steam3Tester
{
    class Program
    {
        static void Main( string[] args )
        {
            
            UdpConnection udpConn = new UdpConnection();

            udpConn.SendChallengeReq( new IPEndPoint( IPAddress.Parse( UdpConnection.CMServers[ 1 ] ), 27014 ) );


            udpConn.ChallengeReceived += ( obj, e ) =>
                {
                    Console.WriteLine( "Got challenge response. Load: " + e.Data.ServerLoad );

                    udpConn.SendConnect( e.Data.ChallengeValue, e.Sender );
                };

            udpConn.AcceptReceived += ( obj, e ) =>
                {
                    Console.WriteLine( "Connection accepted." );
                };
        }
    }
}
