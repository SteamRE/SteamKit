using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace SteamLib
{

    public static class Steam2
    {
        static LoginData loginData;

        public static SteamCallHandle Login( string username, string password, out SteamError err )
        {
            err = new SteamError();

            if ( loginData != null )
            {
                err = new SteamError( ESteamErrorCode.AlreadyLoggedIn );
                return SteamCallHandle.InvalidHandle;
            }

            loginData = new LoginData();
            SteamCallHandle callHandle = new LoginCall( username.ToLower(), password );

            return callHandle;
        }

        public static bool ProcessCall( SteamCallHandle callHandle, ref SteamProgress progress, out SteamError err )
        {
            bool result = callHandle.Process( ref progress, out err );

            if ( !result )
                return result;


            if ( callHandle is LoginCall )
            {
                if ( err.IsError() )
                {
                    loginData = null;
                    return result;
                }

                loginData = callHandle.GetCompletionData() as LoginData;
            }

            return result;
        }


        public static bool GetUserID( out SteamGlobalUserID userId, out SteamError err )
        {
            userId = null;
            err = new SteamError();

            if ( loginData == null || loginData.ClientTGT == null )
            {
                err = new SteamError( ESteamErrorCode.NotLoggedIn );
                return false;
            }

            userId = loginData.ClientTGT.UserID;
            return true;
        }
    }
}