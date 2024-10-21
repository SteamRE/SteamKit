/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// This handler is used for authenticating on Steam.
    /// </summary>
    public sealed class SteamAuthentication
    {
        SteamClient Client;
        internal Internal.Authentication AuthenticationService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamAuthentication"/> class.
        /// </summary>
        /// <param name="steamClient">The <see cref="SteamClient"/> this instance will be associated with.</param>
        internal SteamAuthentication( SteamClient steamClient )
        {
            ArgumentNullException.ThrowIfNull( steamClient );

            Client = steamClient;

            var unifiedMessages = steamClient.GetHandler<SteamUnifiedMessages>()!;
            AuthenticationService = unifiedMessages.CreateService<Internal.Authentication>();
        }

        /// <summary>
        /// Gets public key for the provided account name which can be used to encrypt the account password.
        /// </summary>
        /// <param name="accountName">The account name to get RSA public key for.</param>
        async Task<CAuthentication_GetPasswordRSAPublicKey_Response> GetPasswordRSAPublicKeyAsync( string accountName )
        {
            var request = new CAuthentication_GetPasswordRSAPublicKey_Request
            {
                account_name = accountName
            };

            var response = await AuthenticationService.GetPasswordRSAPublicKey( request );

            if ( response.Result != EResult.OK )
            {
                throw new AuthenticationException( "Failed to get password public key", response.Result );
            }

            return response.Body;
        }

        /// <summary>
        /// Given a refresh token for a client app audience (e.g. desktop client / mobile client), generate an access token.
        /// </summary>
        /// <param name="steamID">The SteamID this token belongs to.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="allowRenewal">If true, allow renewing the token.</param>
        public async Task<AccessTokenGenerateResult> GenerateAccessTokenForAppAsync( SteamID steamID, string refreshToken, bool allowRenewal = false )
        {
            var request = new CAuthentication_AccessToken_GenerateForApp_Request
            {
                refresh_token = refreshToken,
                steamid = steamID.ConvertToUInt64(),
            };

            if ( allowRenewal )
            {
                request.renewal_type = ETokenRenewalType.k_ETokenRenewalType_Allow;
            }

            var response = await AuthenticationService.GenerateAccessTokenForApp( request );

            if ( response.Result != EResult.OK )
            {
                throw new AuthenticationException( "Failed to generate token", response.Result );
            }

            return new AccessTokenGenerateResult( response.Body );
        }

        /// <summary>
        /// Start the authentication process using QR codes.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        public async Task<QrAuthSession> BeginAuthSessionViaQRAsync( AuthSessionDetails details )
        {
            if ( !Client.IsConnected )
            {
                throw new InvalidOperationException( "The SteamClient instance must be connected." );
            }

            var request = new CAuthentication_BeginAuthSessionViaQR_Request
            {
                website_id = details.WebsiteID,
                device_details = new CAuthentication_DeviceDetails
                {
                    device_friendly_name = details.DeviceFriendlyName,
                    platform_type = details.PlatformType,
                    os_type = ( int )details.ClientOSType,
                }
            };

            var response = await AuthenticationService.BeginAuthSessionViaQR( request );

            if ( response.Result != EResult.OK )
            {
                throw new AuthenticationException( "Failed to begin QR auth session", response.Result );
            }

            var authResponse = new QrAuthSession( this, details.Authenticator, response.Body );

            return authResponse;
        }

        /// <summary>
        /// Start the authentication process by providing username and password.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        /// <exception cref="ArgumentNullException">No auth details were provided.</exception>
        /// <exception cref="ArgumentException">Username or password are not set within <paramref name="details"/>.</exception>
        public async Task<CredentialsAuthSession> BeginAuthSessionViaCredentialsAsync( AuthSessionDetails details )
        {
            ArgumentNullException.ThrowIfNull( details );

            if ( string.IsNullOrEmpty( details.Username ) || string.IsNullOrEmpty( details.Password ) )
            {
                throw new ArgumentException( "BeginAuthSessionViaCredentials requires a username and password to be set in 'details'." );
            }

            if ( !Client.IsConnected )
            {
                throw new InvalidOperationException( "The SteamClient instance must be connected." );
            }

            // Encrypt the password
            var publicKey = await GetPasswordRSAPublicKeyAsync( details.Username! ).ConfigureAwait( false );
            var rsaParameters = new RSAParameters
            {
                Modulus = Utils.DecodeHexString( publicKey.publickey_mod ),
                Exponent = Utils.DecodeHexString( publicKey.publickey_exp ),
            };

            using var rsa = RSA.Create();
            rsa.ImportParameters( rsaParameters );
            var encryptedPassword = rsa.Encrypt( Encoding.UTF8.GetBytes( details.Password ), RSAEncryptionPadding.Pkcs1 );

            // Create request
            var request = new CAuthentication_BeginAuthSessionViaCredentials_Request
            {
                account_name = details.Username,
                persistence = details.IsPersistentSession ? ESessionPersistence.k_ESessionPersistence_Persistent : ESessionPersistence.k_ESessionPersistence_Ephemeral,
                website_id = details.WebsiteID,
                guard_data = details.GuardData,
                encrypted_password = Convert.ToBase64String( encryptedPassword ),
                encryption_timestamp = publicKey.timestamp,
                device_details = new CAuthentication_DeviceDetails
                {
                    device_friendly_name = details.DeviceFriendlyName,
                    platform_type = details.PlatformType,
                    os_type = ( int )details.ClientOSType,
                }
            };

            var response = await AuthenticationService.BeginAuthSessionViaCredentials( request );

            // eresult can be InvalidPassword, ServiceUnavailable, InvalidParam, RateLimitExceeded
            if ( response.Result != EResult.OK )
            {
                throw new AuthenticationException( "Authentication failed", response.Result );
            }

            var authResponse = new CredentialsAuthSession( this, details.Authenticator, response.Body );

            return authResponse;
        }
    }
}
