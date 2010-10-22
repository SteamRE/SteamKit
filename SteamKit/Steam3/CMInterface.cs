using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;

namespace SteamKit
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

        public void Disconnect()
        {
            SteamGlobal.Lock();

            var clientlogoff = new ClientMsgProtobuf<MsgClientLogOff>();

            clientlogoff.ProtoHeader.client_session_id = SteamGlobal.SessionID;
            clientlogoff.ProtoHeader.client_steam_id = SteamGlobal.SteamID;

            SteamGlobal.Unlock();

            udpConn.SendNetMsg(clientlogoff, this.bestServer.EndPoint);
        }

        void SendHeartbeat( CMInterface o )
        {
            SteamGlobal.Lock();

            var heartbeat = new ClientMsgProtobuf<MsgClientHeartBeat>();

            heartbeat.ProtoHeader.client_session_id = SteamGlobal.SessionID;
            heartbeat.ProtoHeader.client_steam_id = SteamGlobal.SteamID;

            /*
            var heartbeat = new ClientMsg<MsgClientHeartBeat, ExtendedClientMsgHdr>();

            heartbeat.Header.SessionID = SteamGlobal.SessionID;
            heartbeat.Header.SteamID = SteamGlobal.SteamID;
            */

            SteamGlobal.Unlock();

            udpConn.SendNetMsg( heartbeat, this.bestServer.EndPoint );
        }

        void SendUserLogOn(IPEndPoint endPoint)
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            BlobLib.Blob accountRecord = SteamGlobal.AccountRecord;
            ClientTGT clientTGT = SteamGlobal.ClientTGT;

            SteamGlobalUserID userid = clientTGT.UserID;
            MicroTime creationTime = MicroTime.Deserialize( SteamGlobal.AccountRecord.GetDescriptor( BlobLib.AuthFields.eFieldTimestampCreation ) );

            SteamID steamid = new SteamID();
            steamid.SetFromSteam2( userid, EUniverse.Public );

            logon.ProtoHeader.client_steam_id = steamid.ConvertToUint64();
            logon.ProtoHeader.client_session_id = 0;

            logon.Msg.Proto.obfustucated_private_ip = NetHelpers.GetIPAddress(NetHelpers.GetLocalIP()) ^ MsgClientLogon.ObfuscationMask;

            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol; // default?
            logon.Msg.Proto.client_os_type = 10; // Windows
            logon.Msg.Proto.client_language = "english"; // yoko
            logon.Msg.Proto.rtime32_account_creation = creationTime.ToUnixTime();

            logon.Msg.Proto.cell_id = 10; // TODO
            logon.Msg.Proto.client_package_version = 1385; // TODO

            logon.Msg.Proto.email_address = accountRecord.GetStringDescriptor( BlobLib.AuthFields.eFieldEmail );

            //logon.Proto.login_key = ""; // todo
            //logon.Proto.machine_id = File.ReadAllBytes(@"C:\steamre\machineid.bin"); // todo

            logon.Msg.Proto.account_name = SteamGlobal.username;
            logon.Msg.Proto.password = SteamGlobal.password;

            // private IP inside serverTGT
            byte[] privateIP = BitConverter.GetBytes( NetHelpers.GetIPAddress( NetHelpers.GetLocalIP() ) );

            byte[] serverTGTMagic = new byte[ SteamGlobal.ServerTGT.Length + 4 ];
            Array.Copy( privateIP, 0, serverTGTMagic, 0, 4 );
            Array.Copy( SteamGlobal.ServerTGT, 0, serverTGTMagic, 4, SteamGlobal.ServerTGT.Length );

            logon.Msg.Proto.steam2_auth_ticket = serverTGTMagic;

            udpConn.SendNetMsg(logon, endPoint);
        }

        void SendAnonLogOn( IPEndPoint endPoint )
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            logon.ProtoHeader.client_steam_id = new SteamID(0, 0, EUniverse.Public, EAccountType.AnonGameServer).ConvertToUint64();
            
            logon.Msg.Proto.obfustucated_private_ip = NetHelpers.GetIPAddress(NetHelpers.GetLocalIP()) ^ MsgClientLogon.ObfuscationMask;
            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol; // default?
            logon.Msg.Proto.client_os_type = 10; // Windows

            //var logon = new ClientMsg<MsgClientAnonLogOn, ExtendedClientMsgHdr>();
           // logon.Header.SteamID = new SteamID(0, 0, EUniverse.Public, EAccountType.AnonGameServer);

            /*var logon = new ClientMsg<MsgClientLogOnWithCredentials, ExtendedClientMsgHdr>();
            logon.MsgHeader.ClientSuppliedSteamID = 0;
            logon.MsgHeader.PrivateIPObfuscated = NetHelpers.GetIPAddress( NetHelpers.GetLocalIP() ) ^ MsgClientLogOnWithCredentials.ObfuscationMask;
            */

            udpConn.SendNetMsg( logon, endPoint );
        }

        void SendGSServer(IPEndPoint endPoint)
        {
            var gsserver = new ClientMsgProtobuf<MsgGSServerType>();

            SteamGlobal.Lock();
            gsserver.ProtoHeader.client_session_id = SteamGlobal.SessionID;
            gsserver.ProtoHeader.client_steam_id = SteamGlobal.SteamID;
            SteamGlobal.Unlock();

            gsserver.Msg.Proto.app_id_served = 4000;
            gsserver.Msg.Proto.flags = 0;
            gsserver.Msg.Proto.game_dir = "garrysmod";
            gsserver.Msg.Proto.game_ip_address = 0;
            gsserver.Msg.Proto.game_port = gsserver.Msg.Proto.game_query_port = 27015;
            gsserver.Msg.Proto.game_version = "1.0.0.97";

            udpConn.SendNetMsg( gsserver, endPoint );
        }

        void SendClientKeyResponse(uint uniqueid, IPEndPoint endPoint)
        {
            var response = new ClientMsg<MsgClientNewLoginKeyAccepted, ExtendedClientMsgHdr>();

            SteamGlobal.Lock();
            response.Header.SessionID = SteamGlobal.SessionID;
            response.Header.SteamID = SteamGlobal.SteamID;
            SteamGlobal.Unlock();

            response.Msg.UniqueID = uniqueid;

            udpConn.SendNetMsg(response, endPoint);
        }

        void SendAppOwnershipRequest(uint appid, IPEndPoint endPoint)
        {
            var ownershipReq = new ClientMsgProtobuf<MsgClientGetAppOwnershipTicket>();

            SteamGlobal.Lock();
            ownershipReq.ProtoHeader.client_session_id = SteamGlobal.SessionID;
            ownershipReq.ProtoHeader.client_steam_id = SteamGlobal.SteamID;
            SteamGlobal.Unlock();

            ownershipReq.Msg.Proto.app_id = appid;

            udpConn.SendNetMsg(ownershipReq, endPoint);
        }

        void SendAuthList(IPEndPoint endPoint)
        {
            var authlist = new ClientMsgProtobuf<MsgClientAuthList>();

            SteamGlobal.Lock();
            authlist.ProtoHeader.client_session_id = SteamGlobal.SessionID;
            authlist.ProtoHeader.client_steam_id = SteamGlobal.SteamID;
            SteamGlobal.Unlock();

            authlist.Msg.Proto.tokens_left = 10; // ??

            udpConn.SendNetMsg(authlist, endPoint);
        }

        void SendFriendsDataRequest(IPEndPoint endPoint)
        {
            var friendsdatareq = new ClientMsgProtobuf<MsgClientRequestFriendData>();

            SteamGlobal.Lock();
            friendsdatareq.ProtoHeader.client_session_id = SteamGlobal.SessionID;
            friendsdatareq.ProtoHeader.client_steam_id = SteamGlobal.SteamID;
            SteamGlobal.Unlock();

            friendsdatareq.Msg.Proto.persona_state_requested = 1106; // friend flags.. are they in OSW?

            foreach (CMsgClientFriendsList.Friend friend in SteamGlobal.Friends)
            {
                friendsdatareq.Msg.Proto.friends.Add(friend.ulfriendid);
            }

            udpConn.SendNetMsg(friendsdatareq, endPoint);
        }

        void SendStatusChange(byte state, IPEndPoint endPoint)
        {
            var statuschange = new ClientMsg<MsgClientChangeStatus, ExtendedClientMsgHdr>();

            SteamGlobal.Lock();
            statuschange.Header.SessionID = SteamGlobal.SessionID;
            statuschange.Header.SteamID = SteamGlobal.SteamID;
            SteamGlobal.Unlock();

            statuschange.Msg.PersonaState = state;

            udpConn.SendNetMsg( statuschange, endPoint );
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
            e.Data.Seek(0, SeekOrigin.Begin);

            if ( e.Msg == EMsg.ChannelEncryptResult )
            {
                var encRes = new ClientMsg<MsgChannelEncryptResult, MsgHdr>( e.Data );

                if (encRes.Msg.Result == EResult.OK)
                {
                    //SendAnonLogOn(e.Sender);
                    SendUserLogOn(e.Sender);
                }
                else
                    Console.WriteLine("Failed crypto handshake: " + encRes.Msg.Result);
            }

            if (e.Msg == EMsg.ClientLogOnResponse && e.Proto)
            {
                var logonResp = new ClientMsgProtobuf<MsgClientLogonResponse>( e.Data );

                RecvLogonResponse( e, (EResult)logonResp.Msg.Proto.eresult, 
                                        logonResp.ProtoHeader.client_steam_id, logonResp.ProtoHeader.client_session_id, 
                                        logonResp.Msg.Proto.out_of_game_heartbeat_seconds );
            }

            if (e.Msg == EMsg.GSStatusReply && e.Proto)
            {
                var statusResp = new ClientMsgProtobuf<MsgGSStatusReply>( e.Data );

                Console.WriteLine( "GS Status: secure: " + statusResp.Msg.Proto.is_secure );
            }

            if (e.Msg == EMsg.ClientSessionToken && e.Proto)
            {
                var stok = new ClientMsgProtobuf<MsgClientSessionToken>(e.Data);

                Console.WriteLine("Session token: " + stok.Msg.Proto.token);
            }

            if (e.Msg == EMsg.ClientGameConnectTokens)
            {
                var tokens = new ClientMsgProtobuf<MsgClientGameConnectTokens>(e.Data);

                Console.WriteLine("Got connect tokens: " + tokens.Msg.Proto.max_tokens_to_keep);

                int i = 0;
                foreach (byte[] ticket in tokens.Msg.Proto.tokens)
                {
                    i++;
                    File.WriteAllBytes("C:\\steamre\\ticket_" + i + ".bin", ticket); 
                }
            }

            if (e.Msg == EMsg.ClientFriendsList && e.Proto)
            {
                var friends = new ClientMsgProtobuf<MsgClientFriendsList>( e.Data );

                if (!friends.Msg.Proto.bincremental)
                {
                    SteamGlobal.Friends = new List<CMsgClientFriendsList.Friend>(friends.Msg.Proto.friends.Count);
                }

                Console.WriteLine("Got friends: " + friends.Msg.Proto.friends.Count);

                foreach (CMsgClientFriendsList.Friend friend in friends.Msg.Proto.friends)
                {
                    Console.WriteLine("Friend " + friend.efriendrelationship + " " + friend.ulfriendid);

                    SteamGlobal.Friends.Add(friend);
                }
            }

            if (e.Msg == EMsg.ClientGetAppOwnershipTicketResponse && e.Proto)
            {
                var ticket = new ClientMsgProtobuf<MsgClientGetAppOwnershipTicketResponse>( e.Data );

                Console.WriteLine("Get app ownership: " + (EResult)ticket.Msg.Proto.eresult + " appid: " + ticket.Msg.Proto.app_id);

                if ((EResult)ticket.Msg.Proto.eresult == EResult.OK && ticket.Msg.Proto.app_id == 7)
                {
                    SteamGlobal.WinUITicket = ticket.Msg.Proto.ticket;

                    SendAuthList(e.Sender);
                }
                else
                {
                    File.WriteAllBytes("C:\\steamre\\owner_" + ticket.Msg.Proto.app_id + ".bin", ticket.Msg.Proto.ticket);
                }
            }

            if (e.Msg == EMsg.ClientPersonaState && e.Proto)
            {
                var persona = new ClientMsgProtobuf<MsgClientPersonaState>( e.Data );

                Console.WriteLine("Persona state flags: " + persona.Msg.Proto.status_flags);

                foreach (CMsgClientPersonaState.Friend friend in persona.Msg.Proto.friends)
                {
                    Console.WriteLine("Friend " + friend.player_name + " state: " + friend.persona_state + " " + friend.steamid_source);
                }
            }

            if (e.Msg == EMsg.ClientFriendMsgIncoming)
            {
                var messageinc = new ClientMsg<MsgClientFriendMsgIncoming, ExtendedClientMsgHdr>( e.Data );

                string msg = Encoding.UTF8.GetString( messageinc.Payload.ToArray() );

                Console.WriteLine("Message: " + messageinc.Msg.SteamID + ": " + msg); 
            }

            if (e.Msg == EMsg.ClientNewLoginKey)
            {
                var loginKey = new ClientMsg<MsgClientNewLoginKey, ExtendedClientMsgHdr>( e.Data );

                SteamGlobal.LoginKey = loginKey.Msg.LoginKey;

                SendClientKeyResponse( loginKey.Msg.UniqueID, e.Sender );
                SendFriendsDataRequest( e.Sender );
                SendStatusChange( 1, e.Sender );
                SendAppOwnershipRequest(630, e.Sender);
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

        void RecvLogonResponse(NetMsgEventArgs e, EResult result, UInt64 steamid, Int32 sessionid, int outofgameheartbeat)
        {
            Console.WriteLine("Logon Response: " + result);

            SteamGlobal.Lock();
            SteamGlobal.SessionID = sessionid;
            SteamGlobal.SteamID = steamid;
            SteamGlobal.Unlock();

            if (result == EResult.OK)
            {
                heartBeatFunc = new ScheduledFunction<CMInterface>();
                heartBeatFunc.SetFunc(SendHeartbeat);
                heartBeatFunc.SetObject(this);
                heartBeatFunc.SetDelay(TimeSpan.FromSeconds(outofgameheartbeat));

                if (SteamGlobal.SteamID.AccountType == EAccountType.Individual && SteamGlobal.WinUITicket == null)
                {
                    SendAppOwnershipRequest(7, e.Sender);
                }
                else if (SteamGlobal.SteamID.AccountType == EAccountType.AnonGameServer)
                {
                    SendGSServer(e.Sender);
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
