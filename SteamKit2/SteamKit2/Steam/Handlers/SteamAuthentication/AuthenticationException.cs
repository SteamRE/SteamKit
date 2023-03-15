using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit2
{
    public class AuthenticationException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public EResult Result { get; private set; }

        public AuthenticationException( string message, EResult result )
            : base( $"{message} with result {result}." )
        {
            Result = result;
        }
    }
}
