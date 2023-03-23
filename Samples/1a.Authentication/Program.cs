using System;
using System.Text.Json;
using SteamKit2;
using SteamKit2.Authentication;

if ( args.Length < 2 )
{
    Console.Error.WriteLine( "Sample1a: No username and password specified!" );
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

async void OnConnected( SteamClient.ConnectedCallback callback )
{
    Console.WriteLine( "Connected to Steam! Logging in '{0}'...", user );

    // Begin authenticating via credentials
    var authSession = await steamClient.Authentication.BeginAuthSessionViaCredentialsAsync( new AuthSessionDetails
    {
        Username = user,
        Password = pass,
        IsPersistentSession = false,
        Authenticator = new UserConsoleAuthenticator(),
    } );

    // Starting polling Steam for authentication response
    var pollResponse = await authSession.PollingWaitForResultAsync();

    // Logon to Steam with the access token we have received
    // Note that we are using RefreshToken for logging on here
    steamUser.LogOn( new SteamUser.LogOnDetails
    {
        Username = pollResponse.AccountName,
        AccessToken = pollResponse.RefreshToken,
    } );

    // This is not required, but it is possible to parse the JWT access token to see the scope and expiration date.
    ParseJsonWebToken( pollResponse.AccessToken, nameof( pollResponse.AccessToken ) );
    ParseJsonWebToken( pollResponse.RefreshToken, nameof( pollResponse.RefreshToken ) );
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



// This is simply showing how to parse JWT, this is not required to login to Steam
void ParseJsonWebToken( string token, string name )
{
    // You can use a JWT library to do the parsing for you
    var tokenComponents = token.Split( '.' );

    // Fix up base64url to normal base64
    var base64 = tokenComponents[ 1 ].Replace( '-', '+' ).Replace( '_', '/' );

    if ( base64.Length % 4 != 0 )
    {
        base64 += new string( '=', 4 - base64.Length % 4 );
    }

    var payloadBytes = Convert.FromBase64String( base64 );

    // Payload can be parsed as JSON, and then fields such expiration date, scope, etc can be accessed
    var payload = JsonDocument.Parse( payloadBytes );

    // For brevity we will simply output formatted json to console
    var formatted = JsonSerializer.Serialize( payload, new JsonSerializerOptions
    {
        WriteIndented = true,
    } );
    Console.WriteLine( $"{name}: {formatted}" );
    Console.WriteLine();
}
