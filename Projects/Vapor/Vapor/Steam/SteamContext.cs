using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Net;

namespace Vapor
{
    class Steam2Exception : Exception
    {
        public Steam2Exception( string msg )
            : base( msg )
        {
        }
    }

    class Steam3Exception : Exception
    {
        public Steam3Exception( string msg, Exception inner )
            : base( msg, inner )
        {
        }
    }

    static class Steam2
    {
        public static void Initialize( string userName, string password, out ClientTGT clientTgt, out byte[] serverTgt, out Blob accRecord )
        {
            IPEndPoint[] authServerList = GetAuthServerList( userName );

            if ( authServerList == null )
                throw new Steam2Exception( "Unable to get a list of Steam2 authentication servers." );

            ConnectToAuthServer( authServerList, userName, password, out clientTgt, out serverTgt, out accRecord );

        }

        static IPEndPoint[] GetAuthServerList( string userName )
        {
            GeneralDSClient gdsClient = new GeneralDSClient();

            foreach ( IPEndPoint gdsServer in GeneralDSClient.GDServers )
            {
                gdsClient.Disconnect();

                try
                {
                    DebugLog.WriteLine( "Vapor Steam2", "Connecting to GDS Server {0}...", gdsServer );
                    gdsClient.Connect( gdsServer );
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "Vapor Steam2", "Unable to connect to server.\n{0}", ex.ToString() );
                    continue;
                }

                DebugLog.WriteLine( "Vapor Steam2", "Getting auth server list from {0} using username '{1}'...", gdsServer, userName );
                IPEndPoint[] authServerList = gdsClient.GetAuthServerList( userName );

                if ( authServerList == null || authServerList.Length == 0 )
                {
                    DebugLog.WriteLine( "Vapor Steam2", "Unable to get auth server list. Trying next GDS server..." );
                    continue;
                }

                gdsClient.Disconnect();
                return authServerList;
            }

            return null;
        }

        static bool ConnectToAuthServer( IPEndPoint[] authServerList, string userName, string password, out ClientTGT clientTgt, out byte[] serverTgt, out Blob accRecord )
        {
            clientTgt = null;
            serverTgt = null;
            accRecord = null;

            AuthServerClient asClient = new AuthServerClient();

            foreach ( IPEndPoint authServer in authServerList )
            {
                asClient.Disconnect();

                try
                {
                    DebugLog.WriteLine( "Vapor Steam2", "Connecting to auth server {0}...", authServer );
                    asClient.Connect( authServer );
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "Vapor Steam2", "Unable to connect to auth server.\n{0}", ex.ToString() );
                    continue;
                }

                AuthServerClient.LoginResult loginResult = asClient.Login( userName, password, out clientTgt, out serverTgt, out accRecord );

                if ( loginResult != AuthServerClient.LoginResult.LoggedIn )
                    throw new Steam2Exception( "Result: " + loginResult );

                asClient.Disconnect();
                return true;
            }

            return false;
        }
    }

    interface ICallbackHandler
    {
        void HandleCallback( CallbackMsg msg );
    }

    static class Steam3
    {
        public static SteamClient SteamClient { get; private set; }

        public static SteamFriends SteamFriends { get; private set; }
        public static SteamUser SteamUser { get; private set; }

        public static ChatManager ChatManager { get; private set; }


        static List<ICallbackHandler> callbackHandlers;

        public static string UserName { get; set; }
        public static string Password { get; set; }

        public static ClientTGT ClientTGT { get; set; }
        public static byte[] ServerTGT { get; set; }
        public static Blob AccountRecord { get; set; }

        public static string AuthCode { get; set; }

        public static bool AlternateLogon { get; set; }


        static Steam3()
        {
            callbackHandlers = new List<ICallbackHandler>();
        }


        public static void Initialize( bool useUdp )
        {
            SteamClient = new SteamClient( useUdp ? CMClient.ConnectionType.Udp : CMClient.ConnectionType.Tcp );

            SteamFriends = SteamClient.GetHandler<SteamFriends>( SteamFriends.NAME );
            SteamUser = SteamClient.GetHandler<SteamUser>( SteamUser.NAME );

            ChatManager = new ChatManager();
        }

        public static void Connect()
        {
            try
            {
                SteamClient.Connect();
            }
            catch ( Exception ex )
            {
                throw new Steam3Exception( "Unable to connect to CM server.", ex );
            }
        }

        public static void Shutdown()
        {
            SteamUser.LogOff();
            SteamClient.Disconnect();
        }

        public static void AddHandler( ICallbackHandler handler )
        {
            callbackHandlers.Add( handler );
        }
        public static void RemoveHandler( ICallbackHandler handler )
        {
            callbackHandlers.Remove( handler );
        }

        public static void Update()
        {
            CallbackMsg msg = SteamClient.GetCallback();

            if ( msg == null )
                return;

            SteamClient.FreeLastCallback();

            if ( msg.IsType<SteamClient.ConnectCallback>() )
            {
                SteamUser.LogOn( new SteamUser.LogOnDetails()
                    {
                        Username = Steam3.UserName,
                        Password = Steam3.Password,

                        ClientTGT = Steam3.ClientTGT,
                        ServerTGT = Steam3.ServerTGT,
                        AccRecord = Steam3.AccountRecord,

                        AuthCode = Steam3.AuthCode,

                        AccountInstance = ( uint )( Steam3.AlternateLogon ? 2 : 1 ),
                    } );
            }

            List<ICallbackHandler> tempHandlers = new List<ICallbackHandler>( callbackHandlers );

            // push it along to anyone who wants to handle this
            foreach ( ICallbackHandler handler in tempHandlers )
            {
                handler.HandleCallback( msg );
            }
        }
    }
}
