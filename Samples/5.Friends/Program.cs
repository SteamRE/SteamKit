using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SteamKit2;

//
// Sample 5: Friends
//
// this sample expands upon sample 2 and adds basic interaction
// with the client's persona state and friends list
//

namespace Sample5_CallbackManager
{
    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        static SteamUser steamUser;
        static SteamFriends steamFriends;

        static bool isRunning;

        static string user, pass;


        static void Main( string[] args )
        {
            if ( args.Length < 2 )
            {
                Console.WriteLine( "Sample5: No username and password specified!" );
                return;
            }

            // save our logon details
            user = args[ 0 ];
            pass = args[ 1 ];

            // create our steamclient instance
            steamClient = new SteamClient( System.Net.Sockets.ProtocolType.Tcp );
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager( steamClient );

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();
            // get the steam friends handler, which is used for interacting with friends on the network after logging on
            steamFriends = steamClient.GetHandler<SteamFriends>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            new Callback<SteamClient.ConnectedCallback>( OnConnected, manager );
            new Callback<SteamClient.DisconnectedCallback>( OnDisconnected, manager );

            new Callback<SteamUser.LoggedOnCallback>( OnLoggedOn, manager );
            new Callback<SteamUser.LoggedOffCallback>( OnLoggedOff, manager );

            // we use the following callbacks for friends related activities
            new Callback<SteamUser.AccountInfoCallback>( OnAccountInfo, manager );
            new Callback<SteamFriends.FriendsListCallback>( OnFriendsList, manager );
            new Callback<SteamFriends.PersonaStateCallback>( OnPersonaState, manager );
            new Callback<SteamFriends.FriendAddedCallback>( OnFriendAdded, manager );

            isRunning = true;

            Console.WriteLine( "Connecting to Steam..." );

            // initiate the connection
            steamClient.Connect( false );

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
                Console.WriteLine( "Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult );

                isRunning = false;
                return;
            }

            Console.WriteLine( "Successfully logged on!" );

            // at this point, we'd be able to perform actions on Steam

            // for this sample we wait for other callbacks to perform logic
        }

        static void OnAccountInfo( SteamUser.AccountInfoCallback callback )
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            steamFriends.SetPersonaState( EPersonaState.Online );
        }

        static void OnFriendsList( SteamFriends.FriendsListCallback callback )
        {
            // at this point, the client has received it's friends list

            int friendCount = steamFriends.GetFriendCount();

            Console.WriteLine( "We have {0} friends", friendCount );

            for ( int x = 0 ; x < friendCount ; x++ )
            {
                // steamids identify objects that exist on the steam network, such as friends, as an example
                SteamID steamIdFriend = steamFriends.GetFriendByIndex( x );

                // we'll just display the STEAM_ rendered version
                Console.WriteLine( "Friend: {0}", steamIdFriend.Render() );
            }

            // we can also iterate over our friendslist to accept or decline any pending invites

            foreach ( var friend in callback.FriendList )
            {
                if (friend.Relationship == EFriendRelationship.PendingInvitee)
                {
                    // this user has added us, let's add him back
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }

        static void OnFriendAdded( SteamFriends.FriendAddedCallback callback)
        {
            // someone accepted our friend request, or we accepted one
            Console.WriteLine( "{0} is now a friend", callback.PersonaName );
        }

        static void OnPersonaState( SteamFriends.PersonaStateCallback callback )
        {
            // this callback is received when the persona state (friend information) of a friend changes

            // for this sample we'll simply display the names of the friends
            Console.WriteLine( "State change: {0}", callback.Name );
        }

        static void OnLoggedOff( SteamUser.LoggedOffCallback callback )
        {
            Console.WriteLine( "Logged off of Steam: {0}", callback.Result );
        }
    }
}
