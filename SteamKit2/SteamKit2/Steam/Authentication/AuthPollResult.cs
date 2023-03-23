/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// Represents authentication poll result.
    /// </summary>
    public sealed class AuthPollResult
    {
        /// <summary>
        /// Account name of authenticating account.
        /// </summary>
        public string AccountName { get; }
        /// <summary>
        /// New refresh token.
        /// This can be provided to <see cref="SteamUser.LogOnDetails.AccessToken"/>.
        /// </summary>
        public string RefreshToken { get; }
        /// <summary>
        /// New token subordinate to <see cref="RefreshToken"/>.
        /// </summary>
        public string AccessToken { get; }
        /// <summary>
        /// May contain remembered machine ID for future login, usually when account uses email based Steam Guard.
        /// Supply it in <see cref="AuthSessionDetails.GuardData"/> for future logins to avoid resending an email. This value should be stored per account.
        /// </summary>
        public string? NewGuardData { get; }

        internal AuthPollResult( CAuthentication_PollAuthSessionStatus_Response response )
        {
            AccessToken = response.access_token;
            RefreshToken = response.refresh_token;
            AccountName = response.account_name;
            NewGuardData = response.new_guard_data;
        }
    }
}
