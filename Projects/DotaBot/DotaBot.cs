// File Name:      DotaBot.cs
// Project:           DotaBot
// Copyright (c) christian stewart 2014
// 
// All rights reserved.

using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Threading;
using System.Linq;
using Appccelerate.SourceTemplates.Log4Net;
using Appccelerate.StateMachine;
using log4net;
using log4net.Util;
using SteamKit2;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.Internal;
using Timer = System.Timers.Timer;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
namespace DotaBot
{
    /// <summary>
    ///     Instance of a Dota 2 bot.
    /// </summary>
    public class DotaBot
    {
		private ILog log;
        private SteamClient client;
        private SteamUser.LogOnDetails details;
        private DotaGCHandler dota;

        private CMsgPracticeLobbyListResponseEntry foundLobby;
		private ulong channelId = 0;
		private ulong lobbyChannelId = 0;
        private ulong teamChannelId = 0;
        private SteamFriends friends;
        public ActiveStateMachine<States, Events> fsm;

        protected bool isRunning = false;
        private CallbackManager manager;

        private Thread procThread;
        private bool reconnect;
        private Timer reconnectTimer = new Timer(5000);
        private SteamUser user;
		private uint[] adminIds = { 52661068, 69038686, 15218457 };

	    string password = "dr12345";

        public DotaBot(bool reconnect, SteamUser.LogOnDetails details)
        {
            this.reconnect = reconnect;
            this.details = details;
			this.log = LogManager.GetLogger (details.Username);
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
				.On(Events.DotaGCReady).Goto(States.DotaMenu).Execute(JoinChatChannel);
			fsm.In (States.DotaMenu)
				.On (Events.DotaStartFindLobby).Goto (States.DotaJoinFind);
			fsm.In (States.DotaJoinFind)
                .ExecuteOnEntry (FindLobby)
				.On (Events.DotaFailedLobby).Goto (States.DotaMenu)
                .On(Events.DotaFoundLobby).Goto(States.DotaJoinEnter);
            fsm.In(States.DotaJoinEnter)
                .ExecuteOnEntry(EnterLobby)
                .On(Events.DotaFailedLobby).Goto(States.DotaMenu);
			fsm.In (States.DotaLobby)
                .ExecuteOnEntry(EnterLobbyChat)
				.On (Events.DotaLeftLobby).Goto (States.DotaMenu).Execute(LeaveChatChannel);
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
			if (dota.Lobby != null)
				StatusNotify ("Leaving lobby " + dota.Lobby.lobby_id);
            dota.LeaveLobby();
            LeaveChatChannel();
        }

        private void LeaveChatChannel()
        {
            if (this.lobbyChannelId != 0)
            {
                dota.LeaveChatChannel(lobbyChannelId);
                this.lobbyChannelId = 0;
            }
        }

        private void FindLobby()
        {
			dota.LeaveLobby ();
			StatusNotify("Finding lobbies with password "+password+"...");
            dota.PracticeLobbyList(password);
        }

        private void EnterLobby()
        {
			StatusNotify("Joining lobby "+foundLobby.id+" ("+foundLobby.members[0].player_name+") "+foundLobby.members.Count+" members...");
            dota.JoinLobby(foundLobby.id, password);
            //dota.JoinBroadcastChannel();
        }

        private void EnterLobbyChat()
        {
            dota.JoinChatChannel("Lobby_" + dota.Lobby.lobby_id, DOTAChatChannelType_t.DOTAChannelType_Lobby);
        }

		private void SwitchTeam(DOTA_GC_TEAM team=DOTA_GC_TEAM.DOTA_GC_TEAM_GOOD_GUYS)
		{
			dota.JoinTeam (team, 2);
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

        private const string teamChannel = "Team_526463";
		private void JoinChatChannel()
		{
			log.Debug ("Attempting to join chat channel 'bottest'");
			dota.JoinChatChannel ("bottest");
            dota.JoinChatChannel (teamChannel, DOTAChatChannelType_t.DOTAChannelType_Team);
		}

        private void SetOnlinePresence()
        {
            friends.SetPersonaState(EPersonaState.Online);
            friends.SetPersonaName("Kapparino Bot");
        }

		private void StatusNotify(string message){
			log.DebugFormat("Bot status message: {0}", message);
			if (this.channelId != 0)
				SendChannelMessage (this.channelId, message);
		}
		private void SendChannelMessage(ulong chan, string message){
			log.DebugFormat(channelId + " => BOT SAYS: {0}", message);
			dota.SendChannelMessage (chan, message);
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
						StatusNotify("Can't find any lobbies with password "+password+"...");
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
                    log.Debug("Joined chat "+c.result.channel_name);
					if(c.result.channel_name=="bottest"){
                        this.channelId = c.result.channel_id;
						SendChannelMessage(this.channelId, "Hello!");
					}else if (c.result.channel_name == teamChannel)
					    teamChannelId = c.result.channel_id;
					else
					{
					    this.lobbyChannelId = c.result.channel_id;
                        //SendChannelMessage(this.lobbyChannelId, "Hi, I'm a D2Modd.in lobby bot.");
                        log.Debug("Joined chat channel for lobby successfully");
					}
                }, manager);
                new Callback<DotaGCHandler.ChatMessage>(c =>
                {
                    log.DebugFormat("{0} => {1}: {2}", c.result.channel_id, c.result.persona_name, c.result.text);
                    if (c.result.text.StartsWith("!"))
                    {
						string[] cmdMsg = c.result.text.Substring(1).Split(' ');
						string command = cmdMsg.FirstOrDefault();
						string[] parms = cmdMsg.Skip(1).ToArray();
                        switch (command)
                        {
                            case "about":
                                {
                                    SendChannelMessage(c.result.channel_id, "I am a D2Modd.in lobby bot made by quantum and ilian000."); break;
                                }
							case "info":
								{
									if(dota.Lobby == null){
                                    	SendChannelMessage(c.result.channel_id, "I am not in a lobby.");
										break;
									}
									log.Debug(JsonConvert.SerializeObject(dota.Lobby));
									SendChannelMessage(c.result.channel_id, string.Format("{0}: {1} creator {2} member #1 {3}, custom game {4}", dota.Lobby.lobby_id, dota.Lobby.game_name, dota.Lobby.leader_id, dota.Lobby.members[0].name, dota.Lobby.custom_game_mode));
									break;
								}
                            case "create":
                                {
								    if (dota.Lobby != null)
								    {
								        SendChannelMessage(c.result.channel_id, "Already in lobby "+dota.Lobby.lobby_id+".");
								        return;
								    }
                                    if (parms.Length != 1 && parms.Length != 4)
                                    {
                                        SendChannelMessage(c.result.channel_id, "Usage: !create pass_key [custom_game_id custom_game_mode custom_game_map]");
                                        break;
                                    }
									if (adminIds.Contains(c.result.account_id))
									{
									    var details = new CMsgPracticeLobbySetDetails();
									    if (parms.Length == 4)
									    {
									        ulong game_id;
									        ulong.TryParse(parms[1], out game_id);
									        details.custom_game_id = game_id;
									        details.custom_game_mode = parms[2];
									        details.custom_map_name = parms[3];
									    }
									    details.pass_key = parms[0];
									    details.game_name = "BOT TEST";
									    dota.CreateLobby(parms[0], details);
                                        StatusNotify("Creating lobby with password "+parms[0]+"...");
									}
									else
									{
										SendChannelMessage(c.result.channel_id, "No way, scrub.");
									}
									break;
								} 
                            case "start":
								{
								    if (dota.Lobby == null || dota.Lobby.state != CSODOTALobby.State.UI)
								    {
								        SendChannelMessage(c.result.channel_id, "Not in lobby or lobby is not in UI state.");
								        return;
								    }
									if (adminIds.Contains(c.result.account_id))
									{
                                        dota.LaunchLobby();
									}
									else
									{
										SendChannelMessage(c.result.channel_id, "No way, scrub.");
									}
									break;
								}
							case "join":
								{
									if(parms.Length < 1) break;
									if(dota.Lobby != null) {
										StatusNotify("Already in lobby "+dota.Lobby.lobby_id+"...");
										break;
									}
									if (adminIds.Contains(c.result.account_id))
									{
										password = parms[0];
										fsm.Fire(Events.DotaStartFindLobby);
									}
									else
									{
										SendChannelMessage(c.result.channel_id, "No way, scrub.");
									}
									break;
								}
                            case "leave":
                                {
                                    if (adminIds.Contains(c.result.account_id))
                                    {
                                        leaveLobby();
                                    }
                                    else
                                    {
                                        SendChannelMessage(c.result.channel_id, "No way, scrub.");
                                    }
                                    break;
                                }
                            case "say":
                                {
                                    if (adminIds.Contains(c.result.account_id))
                                    {
                                        SendChannelMessage(c.result.channel_id, String.Join(" ", parms.ToArray()));
									}else{
										SendChannelMessage(c.result.channel_id, "I don't answer to scrubs like you.");
									}
                                    break;
                                }
                            case "saylobby":
                                {
                                    if (lobbyChannelId == 0)
                                    {
                                        SendChannelMessage(c.result.channel_id, "I'm not in a lobby chat.");
                                    }
                                    else if (adminIds.Contains(c.result.account_id))
                                    {
                                        SendChannelMessage(lobbyChannelId, String.Join(" ", parms.ToArray()));
                                    }
                                    else
                                    {
                                        SendChannelMessage(c.result.channel_id, "I don't answer to scrubs like you.");
                                    }
                                    break;
                                }
                            case "sayteam":
                                {
                                    if (teamChannelId == 0)
                                    {
                                        SendChannelMessage(c.result.channel_id, "I'm not in a team chat.");
                                    }
                                    else if (adminIds.Contains(c.result.account_id))
                                    {
                                        SendChannelMessage(teamChannelId, String.Join(" ", parms.ToArray()));
                                    }
                                    else
                                    {
                                        SendChannelMessage(c.result.channel_id, "I don't answer to scrubs like you.");
                                    }
                                    break;
                                }
                            case "radiant":
                            {
                                if (dota.Lobby == null || dota.Lobby.state != CSODOTALobby.State.UI)
                                {
                                    SendChannelMessage(c.result.channel_id, "Not in lobby or lobby is not in UI state.");
                                    return;
                                }
                                if (adminIds.Contains(c.result.account_id))
                                {
                                    SwitchTeam(team:DOTA_GC_TEAM.DOTA_GC_TEAM_GOOD_GUYS); 
                                }
                                else
                                {
                                    SendChannelMessage(c.result.channel_id, "No way, scrub.");
                                }
                                break;
                            }
                            case "dire":
                            {
                                if (dota.Lobby == null || dota.Lobby.state != CSODOTALobby.State.UI)
                                {
                                    SendChannelMessage(c.result.channel_id, "Not in lobby or lobby is not in UI state.");
                                    return;
                                }
                                if (adminIds.Contains(c.result.account_id))
                                {
                                    SwitchTeam(team: DOTA_GC_TEAM.DOTA_GC_TEAM_BAD_GUYS);
                                }
                                else
                                {
                                    SendChannelMessage(c.result.channel_id, "No way, scrub.");
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
                    if (c.result.id == CMsgDOTAPopup.PopupID.KICKED_FROM_LOBBY)
                    {
                        StatusNotify("I was kicked from the lobby.");
                    }
                }, manager);
                new Callback<DotaGCHandler.LiveLeagueGameUpdate>(c =>
                {
                    log.DebugFormat("Tournament games: {0}", c.result.live_league_games);
                }, manager);
				new Callback<DotaGCHandler.PracticeLobbyUpdate> (c => {
					var diffs = Diff.Compare(c.oldLobby, c.lobby);
					var dstrings = new List<string>(diffs.Differences.Count);
					foreach(var diff in diffs.Differences){
						dstrings.Add(string.Format("{0}: {1} => {2}", diff.PropertyName, diff.Object1Value, diff.Object2Value));
					}
					if(dstrings.Count > 0){
						var msg = "Update: "+string.Join(", ", dstrings);
                        if(lobbyChannelId != 0)
						    SendChannelMessage(lobbyChannelId, msg);
                        else
                        {
                            log.Debug("Received lobby update w/o active lobby chat channel.");
                        }
						log.Debug(msg);
					}
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
