using System;
using System.Threading.Tasks;

namespace SteamKit2
{
    /// <summary>
    /// 
    /// </summary>
    public class UserConsoleAuthenticator : IAuthenticator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<string> ProvideDeviceCode()
        {
            string? code;

            do
            {
                Console.Write( "STEAM GUARD! Please enter your 2 factor auth code from your authenticator app: " );
                code = Console.ReadLine()?.Trim();
            }
            while ( string.IsNullOrEmpty( code ) );

            return Task.FromResult( code! );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<string> ProvideEmailCode( string email )
        {
            string? code;

            do
            {
                Console.Write( $"STEAM GUARD! Please enter the auth code sent to the email at {email}: " );
                code = Console.ReadLine()?.Trim();
            }
            while ( string.IsNullOrEmpty( code ) );

            return Task.FromResult( code! );
        }
    }
}
