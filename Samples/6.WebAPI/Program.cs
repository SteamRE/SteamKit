using System;
using System.Collections.Generic;
using System.Net;

using SteamKit2;
using System.Net.Http;

//
// Sample 6: WebAPI
//
// this sample will give an example of how the WebAPI utilities can be used to
// interact with the Steam Web APIs
//
// the Steam Web APIs are structured as a set of "interfaces" with methods,
// similar to classes in OO languages.
// as such, the API for interacting with the WebAPI follows a similar methodology


namespace Sample6_WebAPI
{
    class Program
    {
        static void Main( string[] args )
        {
            // in order to interact with the Web APIs, you must first acquire an interface
            // for a certain API
            using ( dynamic steamNews = WebAPI.GetInterface( "ISteamNews" ) )
            {
                // note the usage of c#'s dynamic feature, which can be used
                // to make the api a breeze to use

                // the ISteamNews WebAPI has only 1 function: GetNewsForApp,
                // so we'll be using that

                // when making use of dynamic, we call the interface function directly
                // and pass any parameters as named arguments
                KeyValue kvNews = steamNews.GetNewsForApp( appid: 440 ); // get news for tf2

                // the return of every WebAPI call is a KeyValue class that contains the result data

                // for this example we'll iterate the results and display the title
                foreach ( KeyValue news in kvNews[ "newsitems" ][ "newsitem" ].Children )
                {
                    Console.WriteLine( "News: {0}", news[ "title" ].AsString() );
                }

                // for functions with multiple versions, the version can be specified by
                // adding a number after the function name when calling the API

                kvNews = steamNews.GetNewsForApp2( appid: 570 );

                // if a number is not specified, version 1 is assumed by default

                // notice that the output of this version differs from the first version
                foreach ( KeyValue news in kvNews[ "newsitems" ].Children )
                {
                    Console.WriteLine( "News: {0}", news[ "title" ].AsString() );
                }

                // note that the interface functions can throw WebExceptions when the API
                // is otherwise inaccessible (networking issues, server downtime, etc)
                // and these should be handled appropriately
                try
                {
                    kvNews = steamNews.GetNewsForApp002( appid: 730, maxlength: 100, count: 5 );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Unable to make GetNewsForApp API request: {0}", ex.Message );
                }
            }

            // for WebAPIs that require an API key, the key can be specified in the GetInterface function
            using ( dynamic steamUserAuth = WebAPI.GetInterface( "ISteamUserAuth", "APIKEYGOESHERE" ) )
            {
                // as the interface functions are synchronous, it may be beneficial to specify a timeout for calls
                steamUserAuth.Timeout = TimeSpan.FromSeconds( 5 );

                // additionally, if the API you are using requires you to POST,
                // you may specify with the "method" reserved parameter
                try
                {
                    steamUserAuth.AuthenticateUser( someParam: "someValue", method: HttpMethod.Post );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Unable to make AuthenticateUser API Request: {0}", ex.Message );
                }
            }

            // if you are using a language that does not have dynamic object support, or you otherwise don't wish to use it
            // you can call interface functions through a Call method
            using ( WebAPI.Interface steamNews = WebAPI.GetInterface( "ISteamNews" ) )
            {
                Dictionary<string, object> newsArgs = new Dictionary<string, object>();
                newsArgs[ "appid" ] = "440";

                KeyValue results = steamNews.Call( "GetNewsForApp", /* version */ 1, newsArgs );

                foreach ( KeyValue news in results[ "newsitems" ][ "newsitem" ].Children )
                {
                    Console.WriteLine( "News: {0}", news[ "title" ].AsString() );
                }
            }
        }
    }
}
