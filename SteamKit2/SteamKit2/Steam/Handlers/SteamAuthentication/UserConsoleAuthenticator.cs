using System;
using System.Threading.Tasks;

namespace SteamKit2
{
    public class UserConsoleAuthenticator : IAuthenticator
    {
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
