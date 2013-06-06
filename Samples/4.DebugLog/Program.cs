using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SteamKit2;

//
// Sample 4: DebugLog
//
// sometimes is may be necessary to peek under the hood of SteamKit2
// to debug or diagnose some issues
//
// to help with this, SK2 includes a component named the DebugLog
//
// internal SK2 components will ocassionally make use of the DebugLog
// to share diagnostic information
//
// in order to use the DebugLog, a listener must first be registered with it
//
// by default, SK2 does not install any listeners, user code must install one
//
// additionally, the DebugLog is disabled by default in release builds
// but it may be enabled with the DebugLog.Enabled member
//
// you'll note that while this sample project is relatively similar to
// Sample 2, the console output becomes very verbose
//

namespace Sample4_DebugLog
{
    // define our debuglog listener
    class MyListener : IDebugListener
    {
        public void WriteLine( string category, string msg )
        {
            // this function will be called when internal steamkit components write to the debuglog

            // for this example, we'll print the output to the console
            Console.WriteLine( "MyListener - {0}: {1}", category, msg );
        }
    }

    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        static SteamUser steamUser;

        static bool isRunning;

        static string user, pass;


        static void Main( string[] args )
        {
            // install our debug listeners for this example

            // install an instance of our custom listener
            DebugLog.AddListener( new MyListener() );

            // install a listener as an anonymous method
            // this call is commented as it would be redundant to install a second listener that also displays messages to the console
            // DebugLog.AddListener( ( category, msg ) => Console.WriteLine( "AnonymousMethod - {0}: {1}", category, msg ) );


            if ( args.Length < 2 )
            {
                Console.WriteLine( "Sample4: No username and password specified!" );
                return;
            }

            // save our logon details
            user = args[ 0 ];
            pass = args[ 1 ];

            // create our steamclient instance
            steamClient = new SteamClient();
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager( steamClient );

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            new Callback<SteamClient.ConnectedCallback>( OnConnected, manager );
            new Callback<SteamClient.DisconnectedCallback>( OnDisconnected, manager );

            new Callback<SteamUser.LoggedOnCallback>( OnLoggedOn, manager );
            new Callback<SteamUser.LoggedOffCallback>( OnLoggedOff, manager );

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
    }
}
