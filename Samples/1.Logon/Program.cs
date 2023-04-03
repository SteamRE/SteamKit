using System;

using SteamKit2;

//
// Sample 1: Logon
//
// the first act of business before being able to use steamkit2's features is to
// logon to the steam network
//
// interaction with steamkit is done through client message handlers and the results
// come back through a callback queue controlled by a steamclient instance 
//
// your code must create a CallbackMgr, and instances of Callback<T>. Callback<T> maps a specific
// callback type to a function, whilst CallbackMgr routes the callback objects to the functions that
// you have specified. a Callback<T> is bound to a specific callback manager.
//
//
// WARNING!
// This the old login flow, we keep this sample around because it still currently works
// for simple cases where you do not need to remember password.
// See Sample1a_Authentication for the new flow.
//

if ( args.Length < 2 )
{
    Console.WriteLine( "Sample1: No username and password specified!" );
    return;
}

// save our logon details
var user = args[ 0 ];
var pass = args[ 1 ];

// create our steamclient instance
var steamClient = new SteamClient();
// create the callback manager which will route callbacks to function calls
var manager = new CallbackManager( steamClient );

// get the steamuser handler, which is used for logging on after successfully connecting
var steamUser = steamClient.GetHandler<SteamUser>();

// register a few callbacks we're interested in
// these are registered upon creation to a callback manager, which will then route the callbacks
// to the functions specified
manager.Subscribe<SteamClient.ConnectedCallback>( OnConnected );
manager.Subscribe<SteamClient.DisconnectedCallback>( OnDisconnected );

manager.Subscribe<SteamUser.LoggedOnCallback>( OnLoggedOn );
manager.Subscribe<SteamUser.LoggedOffCallback>( OnLoggedOff );

var isRunning = true;

Console.WriteLine( "Connecting to Steam..." );

// initiate the connection
steamClient.Connect();

// create our callback handling loop
while ( isRunning )
{
    // in order for the callbacks to get routed, they need to be handled by the manager
    manager.RunWaitCallbacks( TimeSpan.FromSeconds( 1 ) );
}

void OnConnected( SteamClient.ConnectedCallback callback )
{
    Console.WriteLine( "Connected to Steam! Logging in '{0}'...", user );

    steamUser.LogOn( new SteamUser.LogOnDetails
    {
        Username = user,
        Password = pass,
    } );
}

void OnDisconnected( SteamClient.DisconnectedCallback callback )
{
    Console.WriteLine( "Disconnected from Steam" );

    isRunning = false;
}

void OnLoggedOn( SteamUser.LoggedOnCallback callback )
{
    if ( callback.Result != EResult.OK )
    {
        if ( callback.Result == EResult.AccountLogonDenied )
        {
            // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
            // then the account we're logging into is SteamGuard protected
            // see sample 5 for how SteamGuard can be handled

            Console.WriteLine( "Unable to logon to Steam: This account is SteamGuard protected." );

            isRunning = false;
            return;
        }

        Console.WriteLine( "Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult );

        isRunning = false;
        return;
    }

    Console.WriteLine( "Successfully logged on!" );

    // at this point, we'd be able to perform actions on Steam

    // for this sample we'll just log off
    steamUser.LogOff();
}

void OnLoggedOff( SteamUser.LoggedOffCallback callback )
{
    Console.WriteLine( "Logged off of Steam: {0}", callback.Result );
}
