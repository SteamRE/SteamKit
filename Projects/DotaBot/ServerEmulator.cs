// 
// ServerEmulator.cs
// Created by ilian000 on 2014-08-17
// Licenced under the GNU Lesser General Public License v2.1
//

using SteamKit2;
using SteamKit2.Internal;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Threading;
using System.Net;
using SteamKit2.GC;

namespace DotaBot
{
    class ServerEmulator
    {
        protected bool isRunning = true;
        private ILog log = LogManager.GetLogger(typeof(ServerEmulator));
        private Thread procThread;
        private SteamClient client;
        private SteamGameServer server;
        private CallbackManager manager;
        private SteamGameCoordinator gameCoordinator;


        // Server data
        private IPAddress ip;
        private uint port;
        ulong[] playerSteamIds;
        uint GCVersion;
        ulong lobby_id;

        public ServerEmulator(string ip, uint port, ulong[] playerSteamIds, uint GCVersion, ulong lobby_id)
        {
            this.ip = IPAddress.Parse(ip);
            this.port = port;
            this.playerSteamIds = playerSteamIds;
            this.GCVersion = GCVersion;
            this.lobby_id = lobby_id;
            log.DebugFormat("Started Server emulator with ip {0}:{1} and users: {2}", ip, port, String.Join(", ", playerSteamIds));
            client = new SteamClient();
            server = client.GetHandler<SteamGameServer>();
            gameCoordinator = client.GetHandler<SteamGameCoordinator>();
            manager = new CallbackManager(client);

            new Callback<SteamClient.ConnectedCallback>(c =>
            {
                if (c.Result != EResult.OK)
                {
                    isRunning = false;
                    return;
                }

                server.LogOnAnonymous();
            }, manager);

            new Callback<SteamUser.LoggedOnCallback>(c =>
            {
                if (c.Result != EResult.OK)
                {
                    log.ErrorFormat("Failed to login. Reason: {0}", c.Result);
                }
                else
                {
                    log.DebugFormat("Server logged on with SteamID {0}", c.ClientSteamID.ConvertToUInt64());
                    SendServerStatus();
                    SendGameServerUpdate();
                    requestGCSession();
                }
            }, manager);
            new Callback<SteamClient.DisconnectedCallback>(c =>
            {
                isRunning = false;
                log.Debug("Disconnected from steam.");

            }, manager);
            new Callback<SteamGameCoordinator.MessageCallback>(c =>
            {
                log.DebugFormat("Message received from GC: EMsg: {0} AppID: {1} ", c.EMsg, c.AppID);
                if (c.EMsg == (uint)EGCBaseClientMsg.k_EMsgGCServerWelcome)
                {
                    log.Debug("Received welcome message from GC.");
                    SendGCServerInfo();
                    SendLANServerAvailable();
                }
            }, manager);
            client.Connect();
            procThread = new Thread(SteamThread);
            procThread.Start(this);
        }

        private static void SteamThread(object state)
        {
            ServerEmulator s = state as ServerEmulator;
            while (s.isRunning)
            {
                s.manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        private void SendServerStatus()
        {
            server.SendStatus(new SteamGameServer.StatusDetails()
            {
                AppID = 570,
                ServerFlags = EServerFlags.Dedicated,
                Address = ip,
                Port = port,
                GameDirectory = "dota",
                Version = "40",
                QueryPort = port
            });
        }

        private void SendGameServerUpdate()
        {
            var msg = new ClientMsgProtobuf<CMsgGameServerData>(EMsg.AMGameServerUpdate);
            msg.Body.steam_id_gs = client.SteamID.ConvertToUInt64();
            msg.Body.ip = getIp(ip);
            msg.Body.query_port = port;
            msg.Body.game_port = port;
            msg.Body.sourcetv_port = 0;
            msg.Body.name = "D2Modd.in Server";
            msg.Body.app_id = 570;
            msg.Body.gamedir = "dota";
            msg.Body.version = "40";
            msg.Body.product = "dota";
            msg.Body.region = "255";
            foreach (var player in playerSteamIds)
            {
                msg.Body.players.Add(new CMsgGameServerData.Player() { steam_id = player });
            }
            msg.Body.max_players = 32;
            msg.Body.bot_count = 0;
            msg.Body.password = false;
            msg.Body.secure = false;
            msg.Body.dedicated = true;
            msg.Body.os = "w";
            msg.Body.game_data = "";
            msg.Body.game_data_version = 0;
            msg.Body.game_type = "empty";
            msg.Body.map = "dota";
        }

        private uint getIp(IPAddress ip)
        {
            byte[] addrBytes = ip.GetAddressBytes();
            Array.Reverse(addrBytes);
            return BitConverter.ToUInt32(addrBytes, 0);
        }

        private void requestGCSession()
        {
            var req = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCServerHello);
            gameCoordinator.Send(req, 570);
        }

        private void SendGCServerInfo()
        {
            var msg = new ClientGCMsgProtobuf<CMsgGameServerInfo>((uint)EGCBaseMsg.k_EMsgGCGameServerInfo);
            msg.Body.server_public_ip_addr = getIp(ip);
            msg.Body.server_private_ip_addr = getIp(ip);
            msg.Body.server_port = port;
            msg.Body.server_key = "";
            msg.Body.server_hibernation = false;
            msg.Body.server_type = CMsgGameServerInfo.ServerType.GAME;
            msg.Body.server_region = 255;
            msg.Body.server_loadavg = (float)0.12;
            msg.Body.server_tv_broadcast_time = -120;
            msg.Body.server_game_time = 0;
            msg.Body.server_relay_connected_steam_id = 0;
            msg.Body.relay_slots_max = 0;
            msg.Body.relays_connected = 0;
            msg.Body.relay_clients_connected = 0;
            msg.Body.relayed_game_server_steam_id = 0;
            msg.Body.parent_relay_count = 0;
            msg.Body.tv_secret_code = 0; // 16325999627665501
            msg.Body.server_version = GCVersion;
            msg.Body.server_cluster = 0;
            gameCoordinator.Send(msg, 570);
        }

        private void SendLANServerAvailable()
        {
            var msg = new ClientGCMsgProtobuf<CMsgLANServerAvailable>((uint)EGCBaseMsg.k_EMsgGCLANServerAvailable);
            msg.Body.lobby_id = lobby_id;
        }
    }
}
