using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SteamKit2;

//
// Sample 3: Extending SteamKit2
//
// this sample introduces the method through which SK2 can be extended
// with custom message handling and additional features
//
// this sample extends Sample 2 by making use of a custom handler and a custom callback
//
// of interest are the calls to SteamClient.AddHandler and the MyHandler.cs file
//

namespace Sample3_Extending
{
    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        static SteamUser steamUser;
        static MyHandler myHandler;

        static bool isRunning;

        static string user, pass;


        static void Main( string[] args )
        {
            if ( args.Length < 2 )
            {
                Console.WriteLine( "Sample3: No username and password specified!" );
                return;
            }

            // save our logon details
            user = args[ 0 ];
            pass = args[ 1 ];

            // create our steamclient instance
            steamClient = new SteamClient();

            // add our custom handler to our steamclient
            steamClient.AddHandler( new MyHandler() );

            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager( steamClient );

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();
            // now get an instance of our custom handler
            myHandler = steamClient.GetHandler<MyHandler>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            new Callback<SteamClient.ConnectedCallback>( OnConnected, manager );
            new Callback<SteamClient.DisconnectedCallback>( OnDisconnected, manager );

            new Callback<SteamUser.LoggedOnCallback>( OnLoggedOn, manager );
            new Callback<SteamUser.LoggedOffCallback>( OnLoggedOff, manager );

            // handle our own custom callback
            new Callback<MyHandler.MyCallback>( OnMyCallback, manager );

            isRunning = true;

            Console.WriteLine( "Connecting to Steam..." );

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while ( isRunning )
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks( TimeSpan.FromSeconds( 1 ) );
            }
        }

        static void OnConnected( SteamClient.ConnectedCallback callback )
        {
            if ( callback.Result != EResult.OK )
            {
                Console.WriteLine( "Unable to connect to Steam: {0}", callback.Result );

                isRunning = false;
                return;
            }

            Console.WriteLine( "Connected to Steam! Logging in '{0}'...", user );

            steamUser.LogOn( new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
            } );
        }

        static void OnDisconnected( SteamClient.DisconnectedCallback callback )
        {
            Console.WriteLine( "Disconnected from Steam" );

            isRunning = false;
        }

        static void OnLoggedOn( SteamUser.LoggedOnCallback callback )
        {
            if ( callback.Result != EResult.OK )
            {
                if ( callback.Result == EResult.AccountLogonDenied )
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 6 for how SteamGuard can be handled

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

        static void OnLoggedOff( SteamUser.LoggedOffCallback callback )
        {
            Console.WriteLine( "Logged off of Steam: {0}", callback.Result );
        }

        static void OnMyCallback( MyHandler.MyCallback callback )
        {
            // this will be called when our custom callback gets posted
            Console.WriteLine( "OnMyCallback: {0}", callback.Result );
        }
    }
}
