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

        SteamID steamId;
        int sessionId;

        // any network action interacting with members must lock this object
        object netLock = new object();

        ScheduledFunction<CMInterface> heartBeatFunc;


        public CMInterface()
        {
            steamId = new SteamID( 0 );
            bestServer = new CMServer();

            udpConn = new UdpConnection();

            udpConn.AcceptReceived += RecvAccept;
            udpConn.ChallengeReceived += RecvChallenge;
            udpConn.NetMsgReceived += RecvNetMsg;
            udpConn.DisconnectReceived += RecvDisconnect;

            // find our best server while we wait
            foreach ( string ipStr in CMServers )
            {
                IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( ipStr ), 27017 );
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

        void SendHeartbeat( CMInterface o )
        {
            lock ( netLock )
            {
                var heartbeat = new ClientMsg<MsgClientHeartBeat, ExtendedClientMsgHdr>();

                heartbeat.Header.SessionID = this.sessionId;
                heartbeat.Header.SteamID = this.steamId;

                udpConn.SendNetMsg( heartbeat, this.bestServer.EndPoint );
            }
        }


        void SendLogOn( IPEndPoint endPoint )
        {
            // lets send our logon request
            SteamID steamId = new SteamID( 0, 0, EUniverse.Public, EAccountType.AnonGameServer );

            var anonLogon = new ClientMsg<MsgClientAnonLogOn, ExtendedClientMsgHdr>();
            anonLogon.Header.SteamID = steamId;

            udpConn.SendNetMsg( anonLogon, endPoint );
        }

        void RecvDisconnect( object sender, NetworkEventArgs e )
        {
            lock ( netLock )
            {
                if ( this.heartBeatFunc != null )
                {
                    this.heartBeatFunc.Disable();
                    this.heartBeatFunc = null;
                }

                this.steamId = new SteamID( 0 );
                this.sessionId = 0;
            }
        }

        void RecvNetMsg( object sender, NetMsgEventArgs e )
        {
            if ( e.Msg == EMsg.ChannelEncryptResult )
            {

                var encRes = ClientMsg<MsgChannelEncryptResult, MsgHdr>.GetMsgHeader( e.Data );

                if ( encRes.Result == EResult.OK )
                    SendLogOn( e.Sender );
                else
                    Console.WriteLine( "Failed crypto handshake: " + encRes.Result );
            }

            if ( e.Msg == EMsg.ClientLogOnResponse )
            {
                var logonResp = new ClientMsg<MsgClientLogOnResponse, ExtendedClientMsgHdr>( e.Data );

                Console.WriteLine( "Logon Response: " + logonResp.MsgHeader.Result );

                lock ( netLock )
                {
                    this.sessionId = logonResp.Header.SessionID;
                    this.steamId = logonResp.Header.SteamID;
                }

                if ( logonResp.MsgHeader.Result == EResult.OK )
                {
                    heartBeatFunc = new ScheduledFunction<CMInterface>();
                    heartBeatFunc.SetFunc( SendHeartbeat );
                    heartBeatFunc.SetObject( this );
                    heartBeatFunc.SetDelay( TimeSpan.FromSeconds( logonResp.MsgHeader.OutOfGameHeartbeatRateSec ) );
                }

            }

            if ( e.Msg == EMsg.ClientLoggedOff )
            {
                lock ( netLock )
                {
                    if ( this.heartBeatFunc != null )
                    {
                        this.heartBeatFunc.Disable();
                        this.heartBeatFunc = null;
                    }
                }
            }
        }

        void RecvChallenge( object sender, ChallengeEventArgs e )
        {
            lock ( netLock )
            {
                if ( e.Data.ServerLoad < bestServer.ServerLoad )
                {
                    bestServer.EndPoint = e.Sender;
                    bestServer.ServerLoad = e.Data.ServerLoad;
                    bestServer.Challenge = e.Data.ChallengeValue;

                    Console.WriteLine( string.Format( "New CM best! Server: {0}. Load: {1}", bestServer.EndPoint, bestServer.ServerLoad ) );

                    return;
                }
            }
        }

        void RecvAccept( object sender, NetworkEventArgs e )
        {
            Console.WriteLine( "Connection accepted!" );
        }
    }
}
