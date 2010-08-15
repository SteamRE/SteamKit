using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Steam3Lib
{
    public class UDPConnection
    {
        public static string[] Steam3CMServers =
        {
            "68.142.64.164",
            "68.142.64.165",
            "68.142.91.34",
            "68.142.91.35",
            "68.142.91.36",
            "68.142.116.178",
            "68.142.116.179",

            "69.28.145.170",
            "69.28.145.171",
            "69.28.145.172",
            "69.28.156.250",

            "72.165.61.185",
            "72.165.61.186",
            "72.165.61.187",
            "72.165.61.188",

            "208.111.133.84",
            "208.111.133.85",
            "208.111.158.52",
            "208.111.158.53",
            "208.111.171.82",
            "208.111.171.83",
        };

        UdpClient udpSock;


        public  UDPConnection()
        {
            udpSock = new UdpClient();
        }


        private void RequestChallenge( IPAddress ipAddr )
        {
        }

    }
}
