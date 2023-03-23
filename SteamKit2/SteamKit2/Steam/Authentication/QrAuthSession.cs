/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// QR code based authentication session.
    /// </summary>
    public sealed class QrAuthSession : AuthSession
    {
        /// <summary>
        /// URL based on client ID, which can be rendered as QR code.
        /// </summary>
        public string ChallengeURL { get; internal set; }

        /// <summary>
        /// Called whenever the challenge url is refreshed by Steam.
        /// </summary>
        public Action? ChallengeURLChanged { get; set; }

        internal QrAuthSession( SteamAuthentication authentication, IAuthenticator? authenticator, CAuthentication_BeginAuthSessionViaQR_Response response )
            : base( authentication, authenticator, response.client_id, response.request_id, response.allowed_confirmations, response.interval )
        {
            ChallengeURL = response.challenge_url;
        }

        /// <inheritdoc/>
        protected override void HandlePollAuthSessionStatusResponse( CAuthentication_PollAuthSessionStatus_Response response )
        {
            base.HandlePollAuthSessionStatusResponse( response );

            if ( response.new_challenge_url.Length > 0 )
            {
                ChallengeURL = response.new_challenge_url;
                ChallengeURLChanged?.Invoke();
            }
        }
    }
}
