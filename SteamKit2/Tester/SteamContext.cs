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

        public static LogOnDetails LoginDetails
        {
            get
            {
                return new LogOnDetails()
                {
                    Username = userName,
                    Password = password,

                    ClientTGT = clientTgt,
                    ServerTGT = serverTgt,
                    AccRecord = accRecord,
                };
            }
        }


        static string userName;
        static string password;

        static ClientTGT clientTgt;
        static byte[] serverTgt;
        static Blob accRecord;


        public static bool InitializeSteam2( string userName, string password )
        {
            SteamContext.userName = userName;
            SteamContext.password = password;

            GeneralDSClient gdsClient = new GeneralDSClient();
            gdsClient.Connect( GeneralDSClient.GDServers[ 0 ] );

            IPEndPoint[] authServers = gdsClient.GetServerList( EServerType.ProxyASClientAuthentication, userName );
            gdsClient.Disconnect();

            AuthServerClient asClient = new AuthServerClient();
            asClient.Connect( authServers[ 0 ] );

            bool bRet = asClient.Login( userName, password, out clientTgt, out serverTgt, out accRecord );
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
