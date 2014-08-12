using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Appccelerate.SourceTemplates.Log4Net;
using Appccelerate.StateMachine;
using log4net;
using log4net.Util;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DotaBot
{
  /// <summary>
  /// Instance of a Dota 2 bot.
  /// </summary>
  public class DotaBot
  {
    public ActiveStateMachine<States, Events> fsm;
    private bool reconnect;
    private SteamClient client = null;
    private SteamUser user = null;
    private SteamFriends friends;
    private SteamUser.LogOnDetails details;
    private CallbackManager manager;
    private DotaGCHandler dota;

    private Timer reconnectTimer = new Timer(5000);

    private CMsgPracticeLobbyListResponseEntry foundLobby;

    protected bool isRunning = false;

    Thread procThread;

    private static readonly ILog log = LogManager.GetLogger(typeof(DotaBot));

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
      fsm.DefineHierarchyOn (States.Dota)
        .WithHistoryType (HistoryType.None)
        .WithInitialSubState (States.DotaConnect)
        .WithSubState (States.DotaMenu)
        .WithSubState (States.DotaJoinLobby)
        .WithSubState (States.DotaLobby);
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
        .ExecuteOnExit(DisconnectAndCleanup)
        .On(Events.Connected).Goto(States.Connected)
        .On(Events.Disconnected).Goto(States.DisconnectRetry)
        .On(Events.LogonFailSteamGuard).Goto(States.Disconnected)//.Execute(() => reconnect = false)
        .On(Events.LogonFailBadCreds).Goto(States.Disconnected);//.Execute(() => reconnect = false);
      fsm.In(States.Connected)
        .ExecuteOnEntry(SetOnlinePresence)
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
      dota.CloseDota ();
    }

    private void FindLobby()
    {
      log.Debug ("Sent a request for lobby list");
      dota.PracticeLobbyList ("pudge");
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

    void SetOnlinePresence()
    {
      friends.SetPersonaState(EPersonaState.Online);
      friends.SetPersonaName("WebLeague #1");
    }

    void InitAndConnect()
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
        new Callback<SteamClient.DisconnectedCallback>(c => {
            log.Debug("Disconnected " + c.Result);
            fsm.Fire(Events.Disconnected);
            }, manager);
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
        new Callback<DotaGCHandler.GCWelcomeCallback>(c => {
            log.DebugExt("GC welcomed the bot, version "+c.Version);
            fsm.Fire(Events.DotaGCReady);
            }, manager);
        new Callback<DotaGCHandler.UnhandledDotaGCCallback>(c =>
            {
            log.DebugExt("Unknown GC message: " + c.Message.MsgType);
            }, manager);
        new Callback<DotaGCHandler.PracticeLobbyJoinResponse>(c =>
            {
            log.DebugExt("Received practice lobby join response " + c.result.result);
            }, manager);
        new Callback<DotaGCHandler.PracticeLobbyListResponse>(c =>
            {
            log.DebugFormatExt("Practice lobby list response, {0} lobbies.", c.result.lobbies.Count);
            foreach (var lobby in c.result.lobbies)
            {
            log.DebugFormatExt("Name: {0} Players: {1} Needs key? {2}", lobby.name, lobby.members.Count, lobby.requires_pass_key);
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

    void ConnectDota()
    {
      log.Debug("Attempting to connect to Dota...");
      dota.LaunchDota();
    }

    void DisconnectAndCleanup()
    {
      if (client != null)
      {
        if (user != null)
        {
          user.LogOff();
          user = null;
        }
        if(client.IsConnected) client.Disconnect();
        client = null;
      }
      isRunning = false;
    }
  }
}
