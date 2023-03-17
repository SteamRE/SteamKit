using System;
using System.Threading.Tasks;

namespace SteamKit2
{
    /// <summary>
    /// This is a default implementation of <see cref="IAuthenticator"/> to ease of use.
    ///
    /// This implementation will prompt user to enter 2-factor authentication codes in the console.
    /// </summary>
    public class UserConsoleAuthenticator : IAuthenticator
    {
        /// <inheritdoc />
        public Task<string> ProvideDeviceCode( bool previousCodeWasIncorrect )
        {
            if ( previousCodeWasIncorrect )
            {
                Console.Error.WriteLine( "The previous 2-factor auth code you have provided is incorrect." );
            }

            string? code;

            do
            {
                Console.Error.Write( "STEAM GUARD! Please enter your 2-factor auth code from your authenticator app: " );
                code = Console.ReadLine()?.Trim();
            }
            while ( string.IsNullOrEmpty( code ) );

            return Task.FromResult( code! );
        }

        /// <inheritdoc />
        public Task<string> ProvideEmailCode( string email, bool previousCodeWasIncorrect )
        {
            if ( previousCodeWasIncorrect )
            {
                Console.Error.WriteLine( "The previous 2-factor auth code you have provided is incorrect." );
            }

            string? code;

            do
            {
                Console.Error.Write( $"STEAM GUARD! Please enter the auth code sent to the email at {email}: " );
                code = Console.ReadLine()?.Trim();
            }
            while ( string.IsNullOrEmpty( code ) );

            return Task.FromResult( code! );
        }

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmation()
        {
            Console.Error.WriteLine( "STEAM GUARD! Use the Steam Mobile App to confirm your sign in..." );

            return Task.FromResult( true );
        }
    }
}
