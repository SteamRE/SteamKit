/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// Represents access token generation result.
    /// </summary>
    public sealed class AccessTokenGenerateResult
    {
        /// <summary>
        /// New refresh token.
        /// This can be provided to <see cref="SteamUser.LogOnDetails.AccessToken"/> and <see cref="SteamAuthentication.GenerateAccessTokenForAppAsync"/>.
        /// May be an empty string.
        /// </summary>
        public string RefreshToken { get; }
        /// <summary>
        /// New token subordinate to <see cref="RefreshToken"/>.
        /// </summary>
        public string AccessToken { get; }

        internal AccessTokenGenerateResult( CAuthentication_AccessToken_GenerateForApp_Response response )
        {
            AccessToken = response.access_token;
            RefreshToken = response.refresh_token;
        }
    }
}
