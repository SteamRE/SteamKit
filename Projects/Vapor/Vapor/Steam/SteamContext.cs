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

            if ( !ConnectToAuthServer( authServerList, userName, password, out clientTgt, out serverTgt, out accRecord ) )
                throw new Steam2Exception( "Unable to connect to auth server." );

        }

        static IPEndPoint[] GetAuthServerList( string userName )
        {
            GeneralDSClient gdsClient = new GeneralDSClient();

            foreach ( IPEndPoint gdsServer in GeneralDSClient.GDServers )
            {
                gdsClient.Disconnect();

                try
                {
                    gdsClient.Connect( gdsServer );
                }
                catch { continue; }

                IPEndPoint[] authServerList = gdsClient.GetServerList( EServerType.ProxyASClientAuthentication, userName );

                if ( authServerList == null || authServerList.Length == 0 )
                    continue;

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
                    asClient.Connect( authServer );
                }
                catch { continue; }

                if ( !asClient.Login( userName, password, out clientTgt, out serverTgt, out accRecord ) )
                    continue;

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

        static string userName;
        static string password;

        static ClientTGT clientTgt;
        static byte[] serverTgt;
        static Blob accountRecord;


        static Steam3()
        {
            callbackHandlers = new List<ICallbackHandler>();
        }


        public static void Initialize( string userName, string password, ClientTGT clientTgt, byte[] serverTgt, Blob accRecord )
        {

            Steam3.userName = userName;
            Steam3.password = password;

            Steam3.clientTgt = clientTgt;
            Steam3.serverTgt = serverTgt;
            Steam3.accountRecord = accRecord;

            SteamClient = new SteamClient();

            SteamFriends = SteamClient.GetHandler<SteamFriends>( SteamFriends.NAME );
            SteamUser = SteamClient.GetHandler<SteamUser>( SteamUser.NAME );

            try
            {
                SteamClient.Connect();
            }
            catch ( Exception ex )
            {
                throw new Steam3Exception( "Unable to connect to CM server.", ex );
            }

            ChatManager = new ChatManager();
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

            if ( msg is ConnectedCallback )
            {
                SteamUser.LogOn( new SteamUser.LogOnDetails()
                    {
                        Username = Steam3.userName,
                        Password = Steam3.password,

                        ClientTGT = Steam3.clientTgt,
                        ServerTGT = Steam3.serverTgt,
                        AccRecord = Steam3.accountRecord,
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
