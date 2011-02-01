using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SteamKit2;
using System.Net;
using System.Threading;

namespace Tester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string userName = "";
            string password = "";

            GeneralDSClient gdsClient = new GeneralDSClient();
            gdsClient.Connect( GeneralDSClient.GDServers[ 0 ] );

            IPEndPoint[] authServers = gdsClient.GetServerList( EServerType.ProxyASClientAuthentication, userName );
            gdsClient.Disconnect();

            AuthServerClient asClient = new AuthServerClient();
            asClient.Connect( authServers[ 0 ] );

            ClientTGT clientTgt;
            byte[] serverTgt;
            Blob accRecord;

            bool bRet = asClient.Login( userName, password, out clientTgt, out serverTgt, out accRecord );
            asClient.Disconnect();


            SteamClient steamClient = new SteamClient();
            steamClient.ChannelEncrypted += ( obj, e ) =>
                {

                    SteamUser user = steamClient.GetHandler<SteamUser>( SteamUser.NAME );

                    var details = new LogOnDetails()
                    {
                        Username = userName,
                        Password = password,

                        ClientTGT = clientTgt,
                        ServerTGT = serverTgt,
                        AccRecord = accRecord,
                    };

                    user.LogOn( details );
                };

            steamClient.Connect();

            while ( true ) Thread.Sleep( 10 ); // spin
        }
    }
}
