using System;
using System.Collections.Generic;
using System.Net.Http;
using SteamKit2;

//
// Sample 6: WebAPI
//
// this sample will give an example of how the WebAPI utilities can be used to
// interact with the Steam Web APIs
//
// the Steam Web APIs are structured as a set of "interfaces" with methods,
// similar to classes in OO languages.
// as such, the API for interacting with the WebAPI follows a similar methodology


// in order to interact with the Web APIs, you must first acquire an interface
// for a certain API
using ( var steamNews = WebAPI.GetInterface( "ISteamNews" ) )
{
    // the ISteamNews WebAPI has only 1 function: GetNewsForApp,
    // so we'll be using that

    KeyValue kvNews = steamNews.Call( "GetNewsForApp", version: 1, new Dictionary<string, object>
    {
        { "appid", 440 }, // get news for tf2
    } );

    // the return of every WebAPI call is a KeyValue class that contains the result data

    // for this example we'll iterate the results and display the title
    foreach ( KeyValue news in kvNews[ "newsitems" ][ "newsitem" ].Children )
    {
        Console.WriteLine( "News: {0}", news[ "title" ].AsString() );
    }

    // note that the interface functions can throw WebExceptions when the API
    // is otherwise inaccessible (networking issues, server downtime, etc)
    // and these should be handled appropriately
    try
    {
        kvNews = steamNews.Call( "GetNewsForApp", version: 2, new Dictionary<string, object>
        {
            { "appid", 730 }, // get news for cs2
            { "maxlength", 100 },
            { "count", 5 }
        } );
    }
    catch ( Exception ex )
    {
        Console.WriteLine( "Unable to make GetNewsForApp API request: {0}", ex.Message );
    }
}

// for WebAPIs that require an API key, the key can be specified in the GetInterface function
using ( var steamUserAuth = WebAPI.GetInterface( "ISteamUserAuth", "APIKEYGOESHERE" ) )
{
    // as the interface functions are synchronous, it may be beneficial to specify a timeout for calls
    steamUserAuth.Timeout = TimeSpan.FromSeconds( 5 );

    // additionally, if the API you are using requires you to POST,
    // you may specify with the "method" reserved parameter
    try
    {
        steamUserAuth.Call( HttpMethod.Post, "AuthenticateUser", version: 1, new Dictionary<string, object>
        {
            { "someParam", "someValue" }
        } );
    }
    catch ( Exception ex )
    {
        Console.WriteLine( "Unable to make AuthenticateUser API Request: {0}", ex.Message );
    }
}

// async version is available
using ( var steamNews = WebAPI.GetAsyncInterface( "ISteamNews" ) )
{
    var newsArgs = new Dictionary<string, object>
    {
        [ "appid" ] = "440"
    };

    KeyValue results = await steamNews.CallAsync( "GetNewsForApp", version: 1, newsArgs );

    foreach ( KeyValue news in results[ "newsitems" ][ "newsitem" ].Children )
    {
        Console.WriteLine( "News: {0}", news[ "title" ].AsString() );
    }
}
