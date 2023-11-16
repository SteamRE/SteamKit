using System.Threading.Tasks;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// Represents an authenticator to be used with <see cref="SteamAuthentication"/>.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// This method is called when the account being logged into requires 2-factor authentication using the authenticator app.
        /// </summary>
        /// <param name="previousCodeWasIncorrect">True when previously provided code was incorrect.</param>
        /// <returns>The 2-factor auth code used to login. This is the code that can be received from the authenticator app.</returns>
        public Task<string> GetDeviceCodeAsync( bool previousCodeWasIncorrect );

        /// <summary>
        /// This method is called when the account being logged into uses Steam Guard email authentication. This code is sent to the user's email.
        /// </summary>
        /// <param name="email">The email address that the Steam Guard email was sent to.</param>
        /// <param name="previousCodeWasIncorrect">True when previously provided code was incorrect.</param>
        /// <returns>The Steam Guard auth code used to login.</returns>
        public Task<string> GetEmailCodeAsync( string email, bool previousCodeWasIncorrect );

        /// <summary>
        /// This method is called when the account being logged has the Steam Mobile App and accepts authentication notification prompts.
        ///
        /// Return false if you want to fallback to entering a code instead.
        /// </summary>
        /// <returns>Return true to poll until the authentication is accepted, return false to fallback to entering a code.</returns>
        public Task<bool> AcceptDeviceConfirmationAsync();
    }
}
