/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System.Threading.Tasks;
using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// Credentials based authentication session.
    /// </summary>
    public sealed class CredentialsAuthSession : AuthSession
    {
        /// <summary>
        /// SteamID of the account logging in, will only be included if the credentials were correct.
        /// </summary>
        public SteamID SteamID { get; }

        internal CredentialsAuthSession( SteamAuthentication authentication, IAuthenticator? authenticator, CAuthentication_BeginAuthSessionViaCredentials_Response response )
            : base( authentication, authenticator, response.client_id, response.request_id, response.allowed_confirmations, response.interval )
        {
            SteamID = new SteamID( response.steamid );
        }

        /// <summary>
        /// Send Steam Guard code for this authentication session.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="codeType">Type of code.</param>
        /// <returns></returns>
        /// <exception cref="AuthenticationException"></exception>
        public async Task SendSteamGuardCodeAsync( string code, EAuthSessionGuardType codeType )
        {
            var request = new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request
            {
                client_id = ClientID,
                steamid = SteamID,
                code = code,
                code_type = codeType,
            };

            var message = await Authentication.AuthenticationService.SendMessage( api => api.UpdateAuthSessionWithSteamGuardCode( request ) );
            var response = message.GetDeserializedResponse<CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response>();

            // Observed results can be InvalidLoginAuthCode, TwoFactorCodeMismatch, Expired, DuplicateRequest.
            // DuplicateRequest happens when accepting the prompt in the mobile app, and then trying to send guard code here,
            // we do not throw on it here because authentication will succeed on the next poll.
            if ( message.Result != EResult.OK && message.Result != EResult.DuplicateRequest )
            {
                throw new AuthenticationException( "Failed to send steam guard code", message.Result );
            }

            // response may contain agreement_session_url
        }
    }
}
