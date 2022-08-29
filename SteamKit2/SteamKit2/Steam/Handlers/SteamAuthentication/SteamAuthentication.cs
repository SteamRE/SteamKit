/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for authenticating on Steam.
    /// </summary>
    public sealed class SteamAuthentication : ClientMsgHandler
    {
        /// <summary>
        /// Represents the details required to authenticate on Steam.
        /// </summary>
        public sealed class AuthSessionDetails
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            /// <value>The username.</value>
            public string? Username { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>The password.</value>
            public string? Password { get; set; }

            /// <summary>
            /// Gets or sets the device name (or user agent).
            /// </summary>
            /// <value>The device name.</value>
            public string? DeviceFriendlyName { get; set; }

            /// <summary>
            /// Gets or sets the platform type that the login will be performed for.
            /// </summary>
            public EAuthTokenPlatformType PlatformType { get; set; } = EAuthTokenPlatformType.k_EAuthTokenPlatformType_SteamClient;

            /// <summary>
            /// Gets or sets the session persistence.
            /// </summary>
            /// <value>The persistence.</value>
            public ESessionPersistence Persistence { get; set; } = ESessionPersistence.k_ESessionPersistence_Persistent;

            /// <summary>
            /// Gets or sets the website id that the login will be performed for. (EMachineAuthWebDomain)
            /// </summary>
            /// <value>The website id.</value>
            public string? WebsiteID { get; set; }
        }

        /// <summary>
        /// Gets public key for the provided account name which can be used to encrypt the account password.
        /// </summary>
        /// <param name="accountName">The account name to get RSA public key for.</param>
        public async Task<CAuthentication_GetPasswordRSAPublicKey_Response> GetPasswordRSAPublicKey( string accountName )
        {
            var request = new CAuthentication_GetPasswordRSAPublicKey_Request
            {
                account_name = accountName
            };

            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var contentService = unifiedMessages.CreateService<IAuthentication>();
            var message = await contentService.SendMessage( api => api.GetPasswordRSAPublicKey( request ) );
            var response = message.GetDeserializedResponse<CAuthentication_GetPasswordRSAPublicKey_Response>();

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        public async Task BeginAuthSessionViaQR( AuthSessionDetails details )
        {
            var request = new CAuthentication_BeginAuthSessionViaQR_Request
            {
                platform_type = details.PlatformType,
                device_friendly_name = details.DeviceFriendlyName,
            };

            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var contentService = unifiedMessages.CreateService<IAuthentication>();
            var message = await contentService.SendMessage( api => api.BeginAuthSessionViaQR( request ) );
            var response = message.GetDeserializedResponse<CAuthentication_BeginAuthSessionViaQR_Response>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        /// <exception cref="ArgumentNullException">No auth details were provided.</exception>
        /// <exception cref="ArgumentException">Username or password are not set within <paramref name="details"/>.</exception>
        public async Task BeginAuthSessionViaCredentials( AuthSessionDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( nameof( details ) );
            }

            if ( string.IsNullOrEmpty( details.Username ) || string.IsNullOrEmpty( details.Password ) )
            {
                throw new ArgumentException( "BeginAuthSessionViaCredentials requires a username and password to be set in 'details'." );
            }

            // Encrypt the password
            var publicKey = await GetPasswordRSAPublicKey( details.Username! );
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
                platform_type = details.PlatformType,
                device_friendly_name = details.DeviceFriendlyName,
                account_name = details.Username,
                persistence = details.Persistence,
                website_id = details.WebsiteID,
                encrypted_password = Convert.ToBase64String( encryptedPassword ),
                encryption_timestamp = publicKey.timestamp,
            };

            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var contentService = unifiedMessages.CreateService<IAuthentication>();
            var message = await contentService.SendMessage( api => api.BeginAuthSessionViaCredentials( request ) );
            var response = message.GetDeserializedResponse<CAuthentication_BeginAuthSessionViaCredentials_Response>();

            var t = 1;
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            // not used
        }
    }
}
