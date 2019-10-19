using System;
using System.Collections.Generic;
using System.Threading;

using SteamKit2;
using SteamKit2.Internal; // brings in our protobuf client messages
using SteamKit2.GC; // brings in the GC related classes
using SteamKit2.GC.Dota.Internal; // brings in dota specific protobuf messages

namespace DotaMatchRequest
{
    class DotaClient
    {
        SteamClient client;

        SteamUser user;
        SteamGameCoordinator gameCoordinator;

        CallbackManager callbackMgr;

        string userName;
        string password;

        uint matchId;

        bool gotMatch;


        public CMsgDOTAMatch Match { get; private set; }


        // dota2's appid
        const int APPID = 570;


        public DotaClient( string userName, string password, uint matchId )
        {
            this.userName = userName;
            this.password = password;

            this.matchId = matchId;

            client = new SteamClient();

            // get our handlers
            user = client.GetHandler<SteamUser>();
            gameCoordinator = client.GetHandler<SteamGameCoordinator>();

            // setup callbacks
            callbackMgr = new CallbackManager( client );

            callbackMgr.Subscribe<SteamClient.ConnectedCallback>( OnConnected );
            callbackMgr.Subscribe<SteamUser.LoggedOnCallback>( OnLoggedOn );
            callbackMgr.Subscribe<SteamGameCoordinator.MessageCallback>( OnGCMessage );
        }


        public void Connect()
        {
            Console.WriteLine( "Connecting to Steam..." );

            // begin the connection to steam
            client.Connect();
        }

        public void Wait()
        {
            while ( !gotMatch )
            {
                // continue running callbacks until we get match details
                callbackMgr.RunWaitCallbacks( TimeSpan.FromSeconds( 1 ) );
            }
        }


        // called when the client successfully (or unsuccessfully) connects to steam
        void OnConnected( SteamClient.ConnectedCallback callback )
        {
            Console.WriteLine( "Connected! Logging '{0}' into Steam...", userName );

            // we've successfully connected, so now attempt to logon
            user.LogOn( new SteamUser.LogOnDetails
            {
                Username = userName,
                Password = password,
            } );
        }

        // called when the client successfully (or unsuccessfully) logs onto an account
        void OnLoggedOn( SteamUser.LoggedOnCallback callback )
        {
            if ( callback.Result != EResult.OK )
            {
                // logon failed (password incorrect, steamguard enabled, etc)
                // an EResult of AccountLogonDenied means the account has SteamGuard enabled and an email containing the authcode was sent
                // in that case, you would get the auth code from the email and provide it in the LogOnDetails

                Console.WriteLine( "Unable to logon to Steam: {0}", callback.Result );

                gotMatch = true; // we didn't actually get the match details, but we need to jump out of the callback loop
                return;
            }

            Console.WriteLine( "Logged in! Launching DOTA..." );

            // we've logged into the account
            // now we need to inform the steam server that we're playing dota (in order to receive GC messages)

            // steamkit doesn't expose the "play game" message through any handler, so we'll just send the message manually
            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>( EMsg.ClientGamesPlayed );

            playGame.Body.games_played.Add( new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID( APPID ), // or game_id = APPID,
            } );

            // send it off
            // notice here we're sending this message directly using the SteamClient
            client.Send( playGame );

            // delay a little to give steam some time to establish a GC connection to us
            Thread.Sleep( 5000 );

            // inform the dota GC that we want a session
            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>( ( uint )EGCBaseClientMsg.k_EMsgGCClientHello );
            clientHello.Body.engine = ESourceEngine.k_ESE_Source2;
            gameCoordinator.Send( clientHello, APPID );
        }

        // called when a gamecoordinator (GC) message arrives
        // these kinds of messages are designed to be game-specific
        // in this case, we'll be handling dota's GC messages
        void OnGCMessage( SteamGameCoordinator.MessageCallback callback )
        {
            // setup our dispatch table for messages
            // this makes the code cleaner and easier to maintain
            var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { ( uint )EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { ( uint )EDOTAGCMsg.k_EMsgGCMatchDetailsResponse, OnMatchDetails },
            };

            Action<IPacketGCMsg> func;
            if ( !messageMap.TryGetValue( callback.EMsg, out func ) )
            {
                // this will happen when we recieve some GC messages that we're not handling
                // this is okay because we're handling every essential message, and the rest can be ignored
                return;
            }

            func( callback.Message );
        }

        // this message arrives when the GC welcomes a client
        // this happens after telling steam that we launched dota (with the ClientGamesPlayed message)
        // this can also happen after the GC has restarted (due to a crash or new version)
        void OnClientWelcome( IPacketGCMsg packetMsg )
        {
            // in order to get at the contents of the message, we need to create a ClientGCMsgProtobuf from the packet message we recieve
            // note here the difference between ClientGCMsgProtobuf and the ClientMsgProtobuf used when sending ClientGamesPlayed
            // this message is used for the GC, while the other is used for general steam messages
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>( packetMsg );

            Console.WriteLine( "GC is welcoming us. Version: {0}", msg.Body.version );

            Console.WriteLine( "Requesting details of match {0}", matchId );

            // at this point, the GC is now ready to accept messages from us
            // so now we'll request the details of the match we're looking for

            var requestMatch = new ClientGCMsgProtobuf<CMsgGCMatchDetailsRequest>( ( uint )EDOTAGCMsg.k_EMsgGCMatchDetailsRequest );
            requestMatch.Body.match_id = matchId;

            gameCoordinator.Send( requestMatch, APPID );
        }

        // this message arrives after we've requested the details for a match
        void OnMatchDetails( IPacketGCMsg packetMsg )
        {
            var msg = new ClientGCMsgProtobuf<CMsgGCMatchDetailsResponse>( packetMsg );

            EResult result = ( EResult )msg.Body.result;
            if ( result != EResult.OK )
            {
                Console.WriteLine( "Unable to request match details: {0}", result );
            }

            gotMatch = true;
            Match = msg.Body.match;

            // we've got everything we need, we can disconnect from steam now
            client.Disconnect();
        }


        // this is a utility function to transform a uint emsg into a string that can be used to display the name
        static string GetEMsgDisplayString( uint eMsg )
        {
            Type[] eMsgEnums =
            {
                typeof( EGCBaseClientMsg ),
                typeof( EDOTAGCMsg ),
                typeof( EGCBaseMsg ),
                typeof( EGCItemMsg ),
                typeof( ESOMsg ),
            };

            foreach ( var enumType in eMsgEnums )
            {
                if ( Enum.IsDefined( enumType, ( int )eMsg ) )
                    return Enum.GetName( enumType, ( int )eMsg );
                
            }

            return eMsg.ToString();
        }
    }
}
