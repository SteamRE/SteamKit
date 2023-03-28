using System;
using QRCoder;
using SteamKit2;
using SteamKit2.Authentication;

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
    // Start an authentication session by requesting a link
    var authSession = await steamClient.Authentication.BeginAuthSessionViaQRAsync( new AuthSessionDetails() );

    // Steam will periodically refresh the challenge url, this callback allows you to draw a new qr code
    authSession.ChallengeURLChanged = () =>
    {
        Console.WriteLine();
        Console.WriteLine( "Steam has refreshed the challenge url" );

        DrawQRCode( authSession );
    };

    // Draw current qr right away
    DrawQRCode( authSession );

    // Starting polling Steam for authentication response
    // This response is later used to logon to Steam after connecting
    var pollResponse = await authSession.PollingWaitForResultAsync();

    Console.WriteLine( $"Logging in as '{pollResponse.AccountName}'..." );

    // Logon to Steam with the access token we have received
    steamUser.LogOn( new SteamUser.LogOnDetails
    {
        Username = pollResponse.AccountName,
        AccessToken = pollResponse.RefreshToken,
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

void DrawQRCode( QrAuthSession authSession )
{
    Console.WriteLine( $"Challenge URL: {authSession.ChallengeURL}" );
    Console.WriteLine();

    // Encode the link as a QR code
    using var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode( authSession.ChallengeURL, QRCodeGenerator.ECCLevel.L );
    using var qrCode = new AsciiQRCode( qrCodeData );
    var qrCodeAsAsciiArt = qrCode.GetGraphic( 1, drawQuietZones: false );

    Console.WriteLine( "Use the Steam Mobile App to sign in via QR code:" );
    Console.WriteLine( qrCodeAsAsciiArt );
}
