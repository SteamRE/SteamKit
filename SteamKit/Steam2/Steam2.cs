using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace SteamKit
{

    enum LoginState
    {
        NotLoggedIn,
        LoggingIn,
        LoggedIn,
    }

    public static class Steam2
    {
        internal static LoginState LoginState { get; set; }

        static Steam2()
        {
            LoginState = LoginState.NotLoggedIn;
        }

        public static SteamCallHandle Login( string username, string password, out SteamError err )
        {
            err = new SteamError();

            if ( LoginState != LoginState.NotLoggedIn )
            {
                err = new SteamError( ESteamErrorCode.AlreadyLoggedIn );
                return SteamCallHandle.InvalidHandle;
            }

            SteamCallHandle callHandle = new LoginCall( username.ToLower(), password );

            return callHandle;
        }

        public static bool GetUserID( out SteamGlobalUserID userId, out SteamError err )
        {
            userId = null;
            err = new SteamError();

            if ( LoginState != LoginState.LoggedIn )
            {
                err = new SteamError( ESteamErrorCode.NotLoggedIn );
                return false;
            }

            userId = SteamGlobal.ClientTGT.UserID;
            return true;
        }
    }
}