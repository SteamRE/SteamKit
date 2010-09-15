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


    public class CMInterface
    {
        static readonly string[] CMServers =
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

        static CMInterface instance;
        public static CMInterface Instance
        {
            get
            {
                if ( instance == null )
                    instance = new CMInterface();

                return instance;
            }
        }


        UdpConnection udpConn;

        CMServer bestServer;

        // any network action interacting with members must lock this object
        object netLock = new object();

        ScheduledFunction<CMInterface> heartBeatFunc;


        internal CMInterface()
        {
            SteamGlobal.Lock();
            SteamGlobal.SteamID = new SteamID( 0 );
            SteamGlobal.SessionID = 0;
            SteamGlobal.Unlock();

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
            SteamGlobal.Lock();

            var heartbeat = new ClientMsg<MsgClientHeartBeat, ExtendedClientMsgHdr>();

            heartbeat.Header.SessionID = SteamGlobal.SessionID;
            heartbeat.Header.SteamID = SteamGlobal.SteamID;

            SteamGlobal.Unlock();

            udpConn.SendNetMsg( heartbeat, this.bestServer.EndPoint );
        }


        void SendLogOn( IPEndPoint endPoint )
        {
            var logon = new ClientMsgProtobuf<CMsgClientLogon>(EMsg.ClientLogon, true);

            logon.ProtoHeader.client_steam_id = new SteamID(0, 0, EUniverse.Public, EAccountType.AnonGameServer).ConvertToUint64();
            
            logon.Proto.obfustucated_private_ip = NetHelpers.GetIPAddress(NetHelpers.GetLocalIP()) ^ MsgClientLogOnWithCredentials.ObfuscationMask;
            logon.Proto.protocol_version = 65565; // default?
            logon.Proto.client_os_type = 10; // Windows

            //var logon = new ClientMsg<MsgClientAnonLogOn, ExtendedClientMsgHdr>();
           // logon.Header.SteamID = new SteamID(0, 0, EUniverse.Public, EAccountType.AnonGameServer);

            /*var logon = new ClientMsg<MsgClientLogOnWithCredentials, ExtendedClientMsgHdr>();
            logon.MsgHeader.ClientSuppliedSteamID = 0;
            logon.MsgHeader.PrivateIPObfuscated = NetHelpers.GetIPAddress( NetHelpers.GetLocalIP() ) ^ MsgClientLogOnWithCredentials.ObfuscationMask;
            */

            udpConn.SendNetMsg( logon, endPoint );
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
            }

            SteamGlobal.Lock();

            SteamGlobal.SteamID = new SteamID( 0 );
            SteamGlobal.SessionID = 0;

            SteamGlobal.Unlock();
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

            if (e.Msg == EMsg.ClientLogOnResponse && e.Proto)
            {
                var logonResp = new ClientMsgProtobuf<CMsgClientLogonResponse>( e.Data );

                Console.WriteLine("Logon response: " + (EResult)logonResp.Proto.eresult);

                SteamGlobal.Lock();
                SteamGlobal.SessionID = logonResp.ProtoHeader.client_session_id;
                SteamGlobal.SteamID = logonResp.ProtoHeader.client_steam_id;
                SteamGlobal.Unlock();

                if ((EResult)logonResp.Proto.eresult == EResult.OK)
                {
                    heartBeatFunc = new ScheduledFunction<CMInterface>();
                    heartBeatFunc.SetFunc(SendHeartbeat);
                    heartBeatFunc.SetObject(this);
                    heartBeatFunc.SetDelay(TimeSpan.FromSeconds(logonResp.Proto.out_of_game_heartbeat_seconds));
                }
            }
            else if ( e.Msg == EMsg.ClientLogOnResponse && !e.Proto )
            {
                var logonResp = new ClientMsg<MsgClientLogOnResponse, ExtendedClientMsgHdr>( e.Data );

                Console.WriteLine( "Logon Response: " + logonResp.MsgHeader.Result );

                SteamGlobal.Lock();
                SteamGlobal.SessionID = logonResp.Header.SessionID;
                SteamGlobal.SteamID = logonResp.Header.SteamID;
                SteamGlobal.Unlock();

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
