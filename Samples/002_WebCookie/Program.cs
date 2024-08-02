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

var isRunning = true;
var accessToken = string.Empty;
var refreshToken = string.Empty;

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

    // AccessToken can be used as the steamLoginSecure cookie
    // RefreshToken is required to generate new access tokens
    accessToken = pollResponse.AccessToken;
    refreshToken = pollResponse.RefreshToken;
}

void OnDisconnected( SteamClient.DisconnectedCallback callback )
{
    Console.WriteLine( "Disconnected from Steam" );

    isRunning = false;
}

async void OnLoggedOn( SteamUser.LoggedOnCallback callback )
{
    if ( callback.Result != EResult.OK )
    {
        Console.WriteLine( "Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult );

        isRunning = false;
        return;
    }

    Console.WriteLine( "Successfully logged on!" );

    // This is how you concatenate the cookie, you can set it on the Steam domains and it should work
    // but actual usage of this will be left as an excercise for the reader
    var steamLoginSecure = $"{callback.ClientSteamID}||{accessToken}";

    // The access token expires in 24 hours (at the time of writing) so you will have to renew it.
    // Parse this token with a JWT library to get the expiration date and set up a timer to renew it.
    // To renew you will have to call this:
    // When allowRenewal is set to true, Steam may return new RefreshToken
    var newTokens = await steamClient.Authentication.GenerateAccessTokenForAppAsync( callback.ClientSteamID, refreshToken, allowRenewal: false );

    accessToken = newTokens.AccessToken;

    if ( !string.IsNullOrEmpty( newTokens.RefreshToken ) )
    {
        refreshToken = newTokens.RefreshToken;
    }

    // Do not forget to update steamLoginSecure with the new accessToken!

    // for this sample we'll just log off
    steamUser.LogOff();
}

void OnLoggedOff( SteamUser.LoggedOffCallback callback )
{
    Console.WriteLine( "Logged off of Steam: {0}", callback.Result );
}
