using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

namespace SteamLib
{
    class CMServer
    {
        public IPEndPoint EndPoint;
        public uint ServerLoad;

        public uint Challenge;


        public CMServer()
        {
            ServerLoad = uint.MaxValue;
        }
    }


    public class CMInterface : Singleton<CMInterface>
    {
        public static string[] CMServers =
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


        UdpConnection udpConn;

        CMServer bestServer;


        // any network action interacting with members must lock this object
        object netLock = new object();


        public CMInterface()
        {
            bestServer = new CMServer();

            udpConn = new UdpConnection();

            udpConn.AcceptReceived += RecvAccept;
            udpConn.ChallengeReceived += RecvChallenge;
            udpConn.NetMsgReceived += RecvNetMsg;

            // find our best server while we wait
            foreach ( string ipStr in CMServers )
            {
                IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( ipStr ), 27014 );
                udpConn.SendChallengeReq( endPoint );
            }
        }


        public void ConnectToCM()
        {
            Reconnect:

            Monitor.Enter( netLock );

            while ( bestServer.EndPoint == null )
            {
                Monitor.Exit( netLock );

                Thread.Sleep( 500 );
                Console.WriteLine( "Waiting for best CM..." );

                goto Reconnect;
            }

            udpConn.SendConnect( bestServer.Challenge, bestServer.EndPoint );

            Monitor.Exit( netLock );
        }


        void SendLogOn( IPEndPoint endPoint )
        {
            // lets send our logon request
            CSteamID steamId = new CSteamID( 0, 0, EUniverse.Public, EAccountType.AnonGameServer );

            var anonLogon = new ClientMsg<MsgClientAnonLogOn, ExtendedClientMsgHdr>();
            anonLogon.Header.SteamID = steamId;

            IPHostEntry hostEntry = Dns.GetHostByName( Dns.GetHostName() );
            byte[] ipBytes = hostEntry.AddressList[ 0 ].GetAddressBytes();
            uint ip = ( ( uint )ipBytes[ 3 ] << 24 ) + ( ( uint )ipBytes[ 2 ] << 16 ) + ( ( uint )ipBytes[ 1 ] << 8 ) + ( ( uint )ipBytes[ 0 ] );

            anonLogon.MsgHeader.PrivateIPObfuscated = ip ^ MsgClientAnonLogOn.ObfuscationMask;

            anonLogon.Write( new byte[ 19 ] ); // unknown stuff

            udpConn.SendNetMsg( anonLogon, endPoint );
        }


        void RecvNetMsg( object sender, NetMsgEventArgs e )
        {
            if ( e.Msg == EMsg.ChannelEncryptResult )
            {

                var encRes = ClientMsg<MsgChannelEncryptResult, MsgHdr>.GetMsgHeader( e.Data );

                if ( encRes.Result == EResult.OK )
                    SendLogOn( e.Sender );

            }
        }

        void RecvChallenge( object sender, ChallengeEventArgs e )
        {
            Monitor.Enter( netLock );

            if ( e.Data.ServerLoad < bestServer.ServerLoad )
            {
                bestServer.EndPoint = e.Sender;
                bestServer.ServerLoad = e.Data.ServerLoad;
                bestServer.Challenge = e.Data.ChallengeValue;

                Console.WriteLine( string.Format( "New CM best! Server: {0}. Load: {1}", bestServer.EndPoint, bestServer.ServerLoad ) );

                Monitor.Exit( netLock );

                return;
            }

            Monitor.Exit( netLock );
        }

        void RecvAccept( object sender, NetworkEventArgs e )
        {
            Console.WriteLine( "Connection accepted!" );

            if ( e.Sender.Port == 27014 )
                SendLogOn( e.Sender );
        }
    }
}
