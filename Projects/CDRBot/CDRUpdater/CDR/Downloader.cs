using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Net;

namespace CDRUpdater
{
    static class Downloader
    {
        public static byte[] DownloadCDR( byte[] oldHash )
        {
            ConfigServerClient csClient = GetCS();

            byte[] cdr = csClient.GetContentDescriptionRecord( oldHash );
            csClient.Disconnect();

            return cdr;
        }

        static ConfigServerClient GetCS()
        {
            foreach ( IPEndPoint gdsServer in GeneralDSClient.GDServers )
            {
                IPEndPoint[] csServerList = null;

                try
                {
                    GeneralDSClient gdsClient = new GeneralDSClient();
                    gdsClient.Connect( gdsServer );

                    csServerList = gdsClient.GetServerList( EServerType.ConfigServer );

                    gdsClient.Disconnect();
                }
                catch { continue; }

                if ( csServerList == null )
                    continue;

                foreach ( IPEndPoint csServer in csServerList )
                {
                    try
                    {
                        ConfigServerClient csClient = new ConfigServerClient();
                        csClient.Connect( csServer );

                        return csClient;
                    }
                    catch { } // try next in list
                }
            }

            throw new Exception( "Unable to connect to any config server!" );
        }
    }
}
