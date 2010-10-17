using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using BlobLib;
using System.IO;

namespace SteamKit
{

    class LoginCall : SteamCallHandle
    {
        string userName;
        string password;

        AuthServerClient asClient;

        IPEndPoint authServer;

        public LoginCall( string userName, string password )
        {
            this.userName = userName;
            this.password = password;

            this.asClient = new AuthServerClient();

            this.FuncCalls.Add( Step1 );
            this.FuncCalls.Add( Step2 );
            this.FuncCalls.Add( Step3 );
            this.FuncCalls.Add( Step4 );
            this.FuncCalls.Add( Step5 );
            this.FuncCalls.Add( Step6 );

            this.Start();
        }


        // find auth server for username
        bool Step1( ref SteamProgress progress, out SteamError error )
        {
            error = new SteamError();
            progress.Description = "Finding auth server...";

            GeneralDSClient gdsClient = new GeneralDSClient();

            IPEndPoint[] authServers = gdsClient.GetServerList( GeneralDSClient.GDServers[ 0 ], EServerType.ProxyASClientAuthentication, this.userName );

            if ( authServers == null )
            {
                error = new SteamError( ESteamErrorCode.NoConnectivity );
                return false;
            }

            if ( authServers.Length == 0 )
            {
                error = new SteamError( ESteamErrorCode.NoAuthServersAvailable );
                return false;
            }

            authServer = authServers[ 0 ];
            return true;
        }

        // connect to auth server
        bool Step2( ref SteamProgress progress, out SteamError error )
        {
            error = new SteamError();
            progress.Description = "Connecting...";

            if ( !asClient.Connect( authServer ) )
            {
                error = new SteamError( ESteamErrorCode.NoConnectivity );
                return false;
            }

            return true;
        }

        // verify protocol and get ip
        bool Step3( ref SteamProgress progress, out SteamError error )
        {
            error = new SteamError();
            progress.Description = "Verifying...";


            if ( !asClient.RequestIP() )
            {
                error = new SteamError( ESteamErrorCode.NoConnectivity );
                return false;
            }

            return true;
        }

        // send username command, and get salt reply
        bool Step4( ref SteamProgress progress, out SteamError error )
        {
            error = new SteamError();
            progress.Description = string.Format( "Logging in '{0}'...", userName );

            if ( !asClient.GetSalt( userName ) )
            {
                error = new SteamError( ESteamErrorCode.NoConnectivity );
                return false;
            }

            return true;
        }

        // do actual login
        bool Step5( ref SteamProgress progress, out SteamError error )
        {
            error = new SteamError();
            progress.Description = string.Format( "Logging in '{0}'...", userName );

            if ( !asClient.SendLogin( password ) )
            {
                error = new SteamError( ESteamErrorCode.LoginFailed );
                return false;
            }

            SteamGlobal.username = userName;
            SteamGlobal.password = password;

            return true;
        }

        // get acc info
        bool Step6( ref SteamProgress progress, out SteamError error )
        {
            error = new SteamError();
            progress.Description = string.Format( "Getting account information..." );

            ClientTGT clientTgt;
            byte[] serverTgt;
            byte[] accountRec;

            if ( !asClient.GetAccountInfo( out clientTgt, out serverTgt, out accountRec ) )
            {
                error = new SteamError( ESteamErrorCode.LoginFailed );
                return false;
            }

            SteamGlobal.ClientTGT = clientTgt;
            SteamGlobal.ServerTGT = serverTgt;

            BlobParser.SetKey( SteamGlobal.ClientTGT.AccountRecordKey );
            Blob blob = BlobParser.ParseBlob( accountRec );

            SteamGlobal.AccountRecord = blob;

            Steam2.LoginState = LoginState.LoggedIn;

            return true;
        }

        public override string ToString()
        {
            return string.Format( "LoginCall( \"{0}\" )", userName );
        }
    }
}
