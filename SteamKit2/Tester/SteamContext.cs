using System;
using System.Collections.Generic;
using System.Text;
using SteamKit2;
using System.Net;
using System.Threading;

namespace Tester
{
    static class SteamContext
    {
        public static SteamClient SteamClient { get; private set; }

        public static SteamUser SteamUser { get; private set; }
        public static SteamFriends SteamFriends { get; private set; }

        public static SteamUser.LogOnDetails LoginDetails { get; private set; }


        static SteamContext()
        {
            LoginDetails = new SteamUser.LogOnDetails();
        }

        public static bool InitializeSteam2( string userName, string password )
        {
            LoginDetails.Username = userName;
            LoginDetails.Password = password;

            GeneralDSClient gdsClient = new GeneralDSClient();
            gdsClient.Connect( GeneralDSClient.GDServers[ 0 ] );

            IPEndPoint[] authServers = gdsClient.GetServerList( EServerType.ProxyASClientAuthentication, userName );
            gdsClient.Disconnect();

            AuthServerClient asClient = new AuthServerClient();
            asClient.Connect( authServers[ 0 ] );

            bool bRet = asClient.Login( userName, password, out LoginDetails.ClientTGT, out LoginDetails.ServerTGT, out LoginDetails.AccRecord );
            asClient.Disconnect();

            return bRet;
        }

        public static void InitializeSteam3()
        {
            SteamClient = new SteamClient();

            SteamUser = SteamClient.GetHandler<SteamUser>( SteamUser.NAME );
            SteamFriends = SteamClient.GetHandler<SteamFriends>( SteamFriends.NAME );

            SteamClient.Connect();
        }

        public static void ShutdownSteam3()
        {
            SteamClient.Disconnect();
        }
    }
}
