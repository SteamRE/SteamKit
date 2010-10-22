using System;
using System.Collections.Generic;
using System.Text;
using SteamKit;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.IO;

namespace Steam3Tester
{
    class Program
    {
        static void Main( string[] args )
        {
#if true
            // steam2 login code
            SteamError err;
            SteamCallHandle loginHandle = Steam2.Login( "username", "password", out err );

            if ( err.IsError() )
            {
                Console.WriteLine( "Error: {0}", err.ToString() );
                Console.ReadKey();

                return;
            }

            SteamProgress progress = new SteamProgress();
            while ( !loginHandle.Process( ref progress, out err ) )
            {
                if ( progress.IsValid() )
                    Console.WriteLine( "Progress: {0}", progress.Description );

                Thread.Sleep( 100 );
            }

            if ( err.IsError() )
            {
                Console.WriteLine( "Error: {0}", err.ToString() );
                Console.ReadKey();

                return;
            }

            SteamGlobalUserID userId;
            if ( !Steam2.GetUserID( out userId, out err ) )
            {
                Console.WriteLine( "Error: {0}", err.ToString() );
                Console.ReadKey();

                return;
            }

            Console.WriteLine( "UserID: {0} email: {1}", userId.AccountID, SteamGlobal.AccountRecord.GetStringDescriptor( BlobLib.AuthFields.eFieldEmail ) );
            //Console.ReadKey();

            CMInterface cmInterface = CMInterface.Instance;

            cmInterface.ConnectToCM();
#endif

            Console.ReadKey();

            cmInterface.Disconnect();

#if false
            // steam2 server query code
            GDSClient gdsClient = new GDSClient();
            CSDSClient csdsClient = new CSDSClient();

            // ensure latest GDS list
            foreach ( string gdsServer in GDSClient.GDServers )
            {
                ServerCache.AddServer( EServerType.GeneralDirectoryServer, GetAddress( gdsServer ) );

                IPEndPoint[] gdsList = gdsClient.GetServerList( GetAddress( gdsServer ), EServerType.GeneralDirectoryServer, null );
                ServerCache.AddServers( EServerType.GeneralDirectoryServer, gdsList );
            }

            foreach ( IPEndPoint gdsServer in ServerCache.GetServers( EServerType.GeneralDirectoryServer ).GetServers() )
            {
                for ( EServerType serverType = 0 ; ( int )serverType < 30 ; ++serverType )
                {
                    Console.Write( "Asking GDS {0} for {1}... ", gdsServer, serverType );

                    try
                    {
                        IPEndPoint[] serverList = gdsClient.GetServerList( gdsServer, serverType, null );

                        if ( serverList == null )
                            serverList = gdsClient.GetServerList( gdsServer, serverType, "username" );

                        if ( serverList == null )
                        {
                            Console.WriteLine( "Failed!" );
                            continue;
                        }

                        Console.WriteLine( "Got {0} servers.", serverList.Length );

                        ServerCache.AddServers( serverType, serverList );
                    }
                    catch
                    {
                        Console.WriteLine( "Failed!" );
                        continue;
                    }

                }
            }

            foreach ( IPEndPoint csdsServer in ServerCache.GetServers( EServerType.CSDS ).GetServers() )
            {
                for ( EServerType serverType = 0 ; ( int )serverType < 30 ; ++serverType )
                {
                    Console.Write( "Asking CSDS {0} for {1}... ", csdsServer, serverType );
                    try
                    {
                        IPEndPoint[] serverList = csdsClient.GetServerList( csdsServer, serverType, null );

                        if ( serverList == null )
                            serverList = csdsClient.GetServerList( csdsServer, serverType, "username" );

                        if ( serverList == null )
                        {
                            Console.WriteLine( "Failed!" );
                            continue;
                        }

                        Console.WriteLine( "Got {0} servers.", serverList.Length );

                        ServerCache.AddServers( serverType, serverList );
                    }
                    catch
                    {
                        Console.WriteLine( "Failed!" );
                        continue;
                    }
                }
            }


            foreach ( ServerList serverList in ServerCache.GetServerLists() )
            {
                IPEndPoint[] servers = serverList.GetServers();

                File.AppendAllText( "servers.txt", string.Format( "Server type: {0} ({1} servers)\r\n", serverList.Type, servers.Length ) );

                foreach ( IPEndPoint server in servers )
                {
                    File.AppendAllText( "servers.txt", string.Format( "\t{0}\r\n", server ) );

                    DSClient dsClient = new DSClient();

                    for ( EServerType serverType = 0 ; ( int )serverType < 30 ; ++serverType )
                    {
                        try
                        {
                            IPEndPoint[] innerServers = dsClient.GetServerList( server, serverType, null );

                            if ( innerServers != null )
                            {
                                File.AppendAllText( "servers.txt", "\t\tMay be a DS server!\r\n" );
                                break;
                            }
                        }
                        catch { }
                    }
                }

                File.AppendAllText( "servers.txt", "\r\n" );
            }

            
            GDSClient gdsClient = new GDSClient();
            List<IPEndPoint> fullCSDSList = new List<IPEndPoint>();

            foreach ( string gdsServer in GDSClient.GDServers )
            {
                IPEndPoint gdsEndPoint = GetAddress( gdsServer );

                foreach ( IPEndPoint csdsEndPoint in gdsClient.GetServerList( gdsEndPoint, EServerType.CSDS, null ) )
                {
                    if ( HasEP( fullCSDSList, csdsEndPoint ) )
                        continue;

                    fullCSDSList.Add( csdsEndPoint );
                }
            }


            CSDSClient csdsClient = new CSDSClient();
            List<IPEndPoint> fullCSList = new List<IPEndPoint>();

            foreach ( IPEndPoint csdsEndPoint in fullCSDSList )
            {
                IPEndPoint[] csList = csdsClient.GetServerList( csdsEndPoint, EServerType.ConfigServer, null );

                foreach ( IPEndPoint csEndPoint in csList )
                {
                    if ( HasEP( fullCSList, csEndPoint ) )
                        continue;

                    fullCSList.Add( csEndPoint );
                }
            }

            
            IPEndPoint[] csdsList = gdsClient.GetServerList( GetAddress( GDSClient.GDServers[ 0 ] ), EServerType.CSDS, null );

            List<IPEndPoint> fullCSList = new List<IPEndPoint>();

            CSDSClient csdsClient = new CSDSClient();

            foreach ( IPEndPoint csEndPoint in csdsList )
            {
            }
#endif

        }
    }
}