using SteamKit2;
using SteamKit2.Discovery;
using System;
using System.IO;

//
// Sample 7: ServerList
//
// this sample will give an example of how the server list can be used to
// optimize your chance of a successful connection.

namespace Sample7_ServerList
{
    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        static SteamUser steamUser;

        static bool isRunning;

        static string user, pass;


        static void Main( string[] args )
        {
            if ( args.Length < 2 )
            {
                Console.WriteLine( "Sample7: No username and password specified!" );
                return;
            }

            // save our logon details
            user = args[ 0 ];
            pass = args[ 1 ];

            var cellid = 0u;

            // if we've previously connected and saved our cellid, load it.
            if ( File.Exists( "cellid.txt" ) )
            {
                if ( !uint.TryParse( File.ReadAllText( "cellid.txt"), out cellid ) )
                {
                    Console.WriteLine( "Error parsing cellid from cellid.txt. Continuing with cellid 0." );
                    cellid = 0;
                }
                else
                {
                    Console.WriteLine( $"Using persisted cell ID {cellid}" );
                }
            }

            var configuration = SteamConfiguration.Create( b =>
                b.WithCellID( cellid )
                 .WithServerListProvider( new FileStorageServerListProvider("servers_list.bin") ) );

            // create our steamclient instance
            steamClient = new SteamClient( configuration );
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager( steamClient );

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            manager.Subscribe<SteamClient.ConnectedCallback>( OnConnected );
            manager.Subscribe<SteamClient.DisconnectedCallback>( OnDisconnected );

            manager.Subscribe<SteamUser.LoggedOnCallback>( OnLoggedOn );
            manager.Subscribe<SteamUser.LoggedOffCallback>( OnLoggedOff );

            Console.CancelKeyPress += ( s, e ) =>
            {
                e.Cancel = true;

                Console.WriteLine( "Received {0}, disconnecting...", e.SpecialKey );
                steamUser.LogOff();
            };

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
                    // see sample 5 for how SteamGuard can be handled

                    Console.WriteLine( "Unable to logon to Steam: This account is SteamGuard protected." );

                    isRunning = false;
                    return;
                }

                Console.WriteLine( "Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult );

                isRunning = false;
                return;
            }

            // save the current cellid somewhere. if we lose our saved server list, we can use this when retrieving
            // servers from the Steam Directory.
            File.WriteAllText( "cellid.txt", callback.CellID.ToString() );

            Console.WriteLine( "Successfully logged on! Press Ctrl+C to log off..." );
        }

        static void OnLoggedOff( SteamUser.LoggedOffCallback callback )
        {
            Console.WriteLine( "Logged off of Steam: {0}", callback.Result );
        }
    }
}
