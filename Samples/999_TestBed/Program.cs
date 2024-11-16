using System;
using System.Threading;
using SteamKit2;

// This is just a test bed to nethook various packets to later be added in tests

var steamClient = new SteamClient();
var manager = new CallbackManager( steamClient );
using var cts = new CancellationTokenSource();

steamClient.DebugNetworkListener = new NetHookNetworkListener();

var steamUser = steamClient.GetHandler<SteamUser>();
var steamApps = steamClient.GetHandler<SteamApps>();

manager.Subscribe<SteamClient.ConnectedCallback>( OnConnected );
manager.Subscribe<SteamClient.DisconnectedCallback>( OnDisconnected );
manager.Subscribe<SteamUser.LoggedOnCallback>( OnLoggedOn );

Console.WriteLine( "Connecting to Steam..." );

steamClient.Connect();

while ( !cts.IsCancellationRequested )
{
    await manager.RunWaitCallbackAsync( cts.Token );
}

void OnConnected( SteamClient.ConnectedCallback callback )
{
    Console.WriteLine( "Connected to Steam! Logging in..." );

    steamUser.LogOnAnonymous();
}

void OnDisconnected( SteamClient.DisconnectedCallback callback )
{
    Console.WriteLine( "Disconnected from Steam" );

    cts.Cancel();
}

async void OnLoggedOn( SteamUser.LoggedOnCallback callback )
{
    if ( callback.Result != EResult.OK )
    {
        Console.WriteLine( $"Unable to logon to Steam: {callback.Result}" );
        cts.Cancel();
        return;
    }

    Console.WriteLine( "Successfully logged on!" );

    await steamApps.PICSGetProductInfo( new SteamApps.PICSRequest( 480 ), null );
    await steamApps.PICSGetProductInfo( new SteamApps.PICSRequest( 480 ), null, true );

    steamUser.LogOff();
}
