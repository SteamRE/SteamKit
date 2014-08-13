// File Name:      DotaBot.cs
// Project:           DotaBot
// Copyright (c) christian stewart 2014
// 
// All rights reserved.

using System;
using System.Threading;
using System.Linq;
using Appccelerate.SourceTemplates.Log4Net;
using Appccelerate.StateMachine;
using log4net;
using log4net.Util;
using SteamKit2;
using SteamKit2.GC.Dota.Internal;
using Timer = System.Timers.Timer;
using Newtonsoft.Json;

namespace DotaBot
{
    /// <summary>
    ///     Instance of a Dota 2 bot.
    /// </summary>
    public class DotaBot
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (DotaBot));
        private SteamClient client;
        private SteamUser.LogOnDetails details;
        private DotaGCHandler dota;

        private CMsgPracticeLobbyListResponseEntry foundLobby;
        private ulong channelId;
        private SteamFriends friends;
        public ActiveStateMachine<States, Events> fsm;

        protected bool isRunning = false;
        private CallbackManager manager;

        private Thread procThread;
        private bool reconnect;
        private Timer reconnectTimer = new Timer(5000);
        private SteamUser user;
		private uint[] adminIds = { 52661068, 69038686, 15218457 };

        public DotaBot(bool reconnect, SteamUser.LogOnDetails details)
        {
            this.reconnect = reconnect;
            this.details = details;
            reconnectTimer.Elapsed += (sender, args) =>
            {
                reconnectTimer.Stop();
                fsm.Fire(Events.AttemptReconnect);
            };
            fsm = new ActiveStateMachine<States, Events>();
            fsm.AddExtension(new StateMachineLogExtension<States, Events>());
            fsm.DefineHierarchyOn(States.Connecting)
                .WithHistoryType(HistoryType.None);
            fsm.DefineHierarchyOn(States.Connected)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.Dota);
            fsm.DefineHierarchyOn(States.Dota)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.DotaConnect)
                .WithSubState(States.DotaMenu)
                .WithSubState(States.DotaJoinLobby)
                .WithSubState(States.DotaLobby);
            fsm.DefineHierarchyOn(States.Disconnected)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.DisconnectNoRetry)
                .WithSubState(States.DisconnectRetry);
            fsm.DefineHierarchyOn(States.DotaJoinLobby)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.DotaJoinFind)
                .WithSubState(States.DotaJoinEnter);
            fsm.In(States.Connecting)
                .ExecuteOnEntry(InitAndConnect)
                .On(Events.Connected).Goto(States.Connected)
                .On(Events.Disconnected).Goto(States.DisconnectRetry)
                .On(Events.LogonFailSteamGuard).Goto(States.Disconnected) //.Execute(() => reconnect = false)
                .On(Events.LogonFailBadCreds).Goto(States.Disconnected); //.Execute(() => reconnect = false);
            fsm.In(States.Connected)
                .ExecuteOnEntry(SetOnlinePresence)
                .ExecuteOnExit(DisconnectAndCleanup)
                .On(Events.Disconnected).If(ShouldReconnect).Goto(States.Connecting)
                .Otherwise().Goto(States.Disconnected);
            fsm.In(States.Disconnected)
                .ExecuteOnEntry(DisconnectAndCleanup)
                .ExecuteOnExit(ClearReconnectTimer)
                .On(Events.AttemptReconnect).Goto(States.Connecting);
            fsm.In(States.DisconnectRetry)
                .ExecuteOnEntry(StartReconnectTimer);
            fsm.In(States.Dota)
                .ExecuteOnExit(DisconnectDota)
                .On(Events.DotaJoinedLobby).Goto(States.DotaLobby);
            fsm.In(States.DotaConnect)
                .ExecuteOnEntry(ConnectDota)
                .On(Events.DotaGCReady).Goto(States.DotaJoinLobby);
			fsm.In (States.DotaJoinFind)
                .ExecuteOnEntry (FindLobby)
				.On (Events.DotaFailedLobby).Goto (States.DotaMenu)
                .On(Events.DotaFoundLobby).Goto(States.DotaJoinEnter);
            fsm.In(States.DotaJoinEnter)
                .ExecuteOnEntry(EnterLobby)
                .On(Events.DotaFailedLobby).Goto(States.DotaMenu);
			fsm.In (States.DotaLobby)
				.On (Events.DotaLeftLobby).Goto (States.DotaMenu);
            fsm.Initialize(States.Connecting);
        }

        public void Start()
        {
            fsm.Start();
        }

        private void ClearReconnectTimer()
        {
            reconnectTimer.Stop();
        }

        private void DisconnectDota()
        {
            dota.CloseDota();
        }

        private void leaveLobby()
        {
            dota.LeaveLobby();
            dota.LeaveChatChannel(channelId);
        }

		const string password = "dr12345";
        private void FindLobby()
        {
			dota.LeaveLobby ();
			log.Debug("Sent a request for lobby list, password "+password);
            dota.PracticeLobbyList(password);
        }

        private void EnterLobby()
        {
            dota.JoinLobby(foundLobby.id, password);
            //dota.JoinBroadcastChannel();
            dota.JoinChatChannel("Lobby_" + foundLobby.id);
        }

        private void StartReconnectTimer()
        {
            reconnectTimer.Start();
        }

        private static void SteamThread(object state)
        {
            DotaBot bot = state as DotaBot;
            while (bot.isRunning)
            {
                bot.manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        private bool ShouldReconnect()
        {
            return reconnect;
        }

        private void SetOnlinePresence()
        {
            friends.SetPersonaState(EPersonaState.Online);
            friends.SetPersonaName("Kapparino Bot");
        }

        private void InitAndConnect()
        {
            if (client == null)
            {
                client = new SteamClient();
                user = client.GetHandler<SteamUser>();
                friends = client.GetHandler<SteamFriends>();
                dota = client.GetHandler<DotaGCHandler>();
                manager = new CallbackManager(client);
                isRunning = true;
                new Callback<SteamClient.ConnectedCallback>(c =>
                {
                    if (c.Result != EResult.OK)
                    {
                        fsm.FirePriority(Events.Disconnected);
                        isRunning = false;
                        return;
                    }

                    user.LogOn(details);
                }, manager);
                new Callback<SteamClient.DisconnectedCallback>(c => { fsm.Fire(Events.Disconnected); }, manager);
                new Callback<SteamUser.LoggedOnCallback>(c =>
                {
                    if (c.Result != EResult.OK)
                    {
                        if (c.Result == EResult.AccountLogonDenied)
                        {
                            fsm.Fire(Events.LogonFailSteamGuard);
                            return;
                        }
                        fsm.Fire(Events.LogonFailBadCreds);
                    }
                    else
                    {
                        fsm.Fire(Events.Connected);
                    }
                }, manager);
                new Callback<DotaGCHandler.GCWelcomeCallback>(c =>
                {
                    log.Debug("GC welcomed the bot, version " + c.Version);
                    fsm.Fire(Events.DotaGCReady);
                }, manager);
                new Callback<DotaGCHandler.UnhandledDotaGCCallback>(
                    c => { log.Debug("Unknown GC message: " + c.Message.MsgType); }, manager);
                new Callback<DotaGCHandler.PracticeLobbyJoinResponse>(c => { 
                    log.Debug("Received practice lobby join response " + c.result.result);
                    fsm.Fire(Events.DotaJoinedLobby);
                }, manager);
                new Callback<DotaGCHandler.PracticeLobbyListResponse>(c =>
                {
                    log.DebugFormat("Practice lobby list response, {0} lobbies.", c.result.lobbies.Count);
					log.Debug(JsonConvert.SerializeObject(c.result.lobbies));
                    var aLobby = c.result.lobbies.FirstOrDefault(m=>m.members.Count < 10);
                    if (aLobby != null)
                    {
                        this.foundLobby = aLobby;
                        log.Debug("Selected lobby "+aLobby.id);
                        fsm.Fire(Events.DotaFoundLobby);
                    }else{
                        fsm.Fire(Events.DotaFailedLobby);
                    }
                }, manager);
                new Callback<SteamFriends.FriendsListCallback>(c=>{
					log.Debug(c.FriendList);
                }, manager);
                new Callback<DotaGCHandler.PracticeLobbySnapshot>(c=>{
					log.DebugFormat("Lobby snapshot received with state: {0}", c.lobby.state);
                    log.Debug(JsonConvert.SerializeObject(c.lobby));
				}, manager);
                new Callback<DotaGCHandler.PingRequest>(c =>
                {
                    log.Debug("GC Sent a ping request. Sending pong!");
                    dota.Pong();
                }, manager);
                new Callback<DotaGCHandler.JoinChatChannelResponse>(c =>
                {
                    log.DebugFormat("Chat Channel response code {0} received. {1} users in channel.", c.result.response, c.result.members.Count);
                    this.channelId = c.result.channel_id;
                }, manager);
                new Callback<DotaGCHandler.ChatMessage>(c =>
                {
                    log.DebugFormat("Chat message received in channel {0} from {1}: {2}", c.result.channel_id, c.result.persona_name, c.result.text);
                    if (c.result.text.StartsWith("!"))
                    {
                        string cmdMsg = c.result.text.Substring(1);
                        string command = cmdMsg.Split(' ').FirstOrDefault();
                        string[] parms = cmdMsg.Split(' ').Skip(1).ToArray();
                        switch (command)
                        {
                            case "about":
                                {
                                    dota.SendChannelMessage(c.result.channel_id, "I am a D2Modd.in lobby bot made by quantum and ilian000.");
                                    break;
                                }
                            case "leave":
                                {
                                    if (adminIds.Contains(c.result.account_id))
                                    {
                                        dota.SendChannelMessage(c.result.channel_id, "Goodbye my friend!");
                                        leaveLobby();

                                    }
                                    else
                                    {
                                        dota.SendChannelMessage(c.result.channel_id, "No way, scrub.");
                                    }
                                    break;
                                }
                            case "say":
                                {
                                    if (adminIds.Contains(c.result.account_id))
                                    {
                                        dota.SendChannelMessage(c.result.channel_id, String.Join(" ", parms.ToArray()));
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }

                        }
                    }
                    
                }, manager);
                new Callback<DotaGCHandler.OtherJoinedChannel>(c =>
                {
                    log.DebugFormat("User with name {0} joined channel {1}.", c.result.persona_name, c.result.channel_id);
                }, manager);
                new Callback<DotaGCHandler.OtherLeftChannel>(c =>
                {
                    log.DebugFormat("User with steamid {0} left channel {1}.", c.result.steam_id, c.result.channel_id);
                }, manager);
                new Callback<DotaGCHandler.CacheUnsubscribed>(c =>
                {
                    log.Debug("Left lobby.");
					fsm.Fire(Events.DotaLeftLobby);
                }, manager);
                new Callback<DotaGCHandler.Popup>(c =>
                {
                    log.DebugFormat("Received message from GC: {0}", c.result.id);
                }, manager);
                new Callback<DotaGCHandler.LiveLeagueGameUpdate>(c =>
                {
                    log.DebugFormat("There are {0} tournaments running at the moment...", c.result.live_league_games);
                }, manager);
            }
            client.Connect();
            procThread = new Thread(SteamThread);
            procThread.Start(this);
        }

        private void ConnectDota()
        {
            log.Debug("Attempting to connect to Dota...");
            dota.LaunchDota();
        }

        private void DisconnectAndCleanup()
        {
            if (client != null)
            {
                if (user != null)
                {
                    user.LogOff();
                    user = null;
                }
                if (client.IsConnected) client.Disconnect();
                client = null;
            }
            isRunning = false;
        }
    }
}
