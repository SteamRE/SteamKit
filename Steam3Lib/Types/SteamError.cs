using System;
using System.Collections.Generic;
using System.Text;

namespace SteamLib
{
    public enum ESteamErrorCode
    {
        None,
        NotImplemented,         // method not implemented

        CallTimedOut,           // async call was not processed in time

        NoAuthServersAvailable, // could not find an authentication server for user
        NoConnectivity,         // cannot connect to steam servers

        LoginFailed,            // unable to login
        AlreadyLoggedIn,        // already logged in, or login in progress
        NotLoggedIn,            // method required a current login
    }

    public class SteamError
    {
        public ESteamErrorCode ErrorCode { get; private set; }


        public SteamError()
            : this( ESteamErrorCode.None )
        {
        }
        public SteamError( ESteamErrorCode errorCode )
        {
            this.ErrorCode = errorCode;
        }


        public bool IsError()
        {
            return this.ErrorCode != ESteamErrorCode.None;
        }

        public override string ToString()
        {
            return this.ErrorCode.ToString();
        }
    }
}
