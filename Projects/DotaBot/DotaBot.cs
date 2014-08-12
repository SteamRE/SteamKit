// File Name:      DotaBot.cs
// Project:           DotaBot
// Copyright (c) christian stewart 2014
// 
// All rights reserved.

using System;
using System.Threading;
using Appccelerate.SourceTemplates.Log4Net;
using Appccelerate.StateMachine;
using log4net;
using log4net.Util;
using SteamKit2;
using SteamKit2.GC.Dota.Internal;
using Timer = System.Timers.Timer;

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
        private SteamFriends friends;
        public ActiveStateMachine<States, Events> fsm;

        protected bool isRunning = false;
        private CallbackManager manager;

        private Thread procThread;
        private bool reconnect;
        private Timer reconnectTimer = new Timer(5000);
        private SteamUser user;

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
                .ExecuteOnExit(DisconnectDota);
            fsm.In(States.DotaConnect)
                .ExecuteOnEntry(ConnectDota)
                .On(Events.DotaGCReady).Goto(States.DotaJoinLobby);
            fsm.In(States.DotaJoinFind)
                .ExecuteOnEntry(FindLobby)
                .On(Events.DotaJoinedLobby).Goto(States.DotaLobby)
                .On(Events.DotaFoundLobby).Goto(States.DotaJoinEnter);
            fsm.In(States.DotaJoinEnter)
                .ExecuteOnEntry(EnterLobby)
                .On(Events.DotaJoinedLobby).Goto(States.DotaLobby)
                .On(Events.DotaFailedLobby).Goto(States.DotaMenu);
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

        private void FindLobby()
        {
            log.Debug("Sent a request for lobby list");
            dota.PracticeLobbyList("pudge");
        }

        private void EnterLobby()
        {
            dota.JoinLobby("workwork");
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
            friends.SetPersonaName("WebLeague #1");
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
                    log.DebugExt("GC welcomed the bot, version " + c.Version);
                    fsm.Fire(Events.DotaGCReady);
                }, manager);
                new Callback<DotaGCHandler.UnhandledDotaGCCallback>(
                    c => { log.DebugExt("Unknown GC message: " + c.Message.MsgType); }, manager);
                new Callback<DotaGCHandler.PracticeLobbyJoinResponse>(
                    c => { log.DebugExt("Received practice lobby join response " + c.result.result); }, manager);
                new Callback<DotaGCHandler.PracticeLobbyListResponse>(c =>
                {
                    log.DebugFormatExt("Practice lobby list response, {0} lobbies.", c.result.lobbies.Count);
                    foreach (var lobby in c.result.lobbies)
                    {
                        log.DebugFormatExt("Name: {0} Players: {1} Needs key? {2}", lobby.name, lobby.members.Count,
                            lobby.requires_pass_key);
                    }
                    if (c.result.lobbies.Count > 0)
                    {
                        log.DebugExt("Found a lobby, picking first one.");
                    }
                });
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