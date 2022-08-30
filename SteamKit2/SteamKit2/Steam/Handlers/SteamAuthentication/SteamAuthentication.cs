/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
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
        /* EPasswordLoginSessionStatus
            Unstarted: 0,
            Starting: 1,
            InvalidCredentials: 2,
            WaitingForEmailCode: 3,
            WaitingForEmailConfirmation: 4,
            WaitingForDeviceCode: 5,
            WaitingForDeviceConfirmation: 6,
            StartMoveAuthenticator: 7,
            WaitingForMoveCode: 8,
            AuthenticatorMoved: 9,
            InvalidEmailCode: 10,
            InvalidDeviceCode: 11,
            InvalidMoveCode: 12,
            WaitingForToken: 13,
            Success: 14,
            Failure: 15,
            Stopped: 16,
        */

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

        public class AuthComplete
        {
            public string AccountName { get; set; }
            public string RefreshToken { get; set; }
            public string AccessToken { get; set; }
        }

        public class AuthSession
        {
            public SteamClient Client { get; internal set; }
            public ulong ClientID { get; set; }
            public byte[] RequestID { get; set; }
            public List<CAuthentication_AllowedConfirmation> AllowedConfirmations { get; set; }
            public TimeSpan PollingInterval { get; set; }

            public async Task<AuthComplete> StartPolling()
            {
                // TODO: Sort by preferred methods?
                foreach ( var allowedConfirmation in AllowedConfirmations )
                {
                    switch ( allowedConfirmation.confirmation_type )
                    {
                        case EAuthSessionGuardType.k_EAuthSessionGuardType_None:
                            // no steam guard
                            // if we poll now we will get access token in response and send login to the cm
                            break;

                        case EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode:
                            // sent steam guard email at allowedConfirmation.associated_message
                            // use SendSteamGuardCode
                            break;

                        case EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode:
                            // totp code from mobile app
                            // use SendSteamGuardCode
                            break;

                        case EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceConfirmation:
                            // TODO: is this accept prompt that automatically appears in the mobile app?
                            break;

                        case EAuthSessionGuardType.k_EAuthSessionGuardType_EmailConfirmation:
                            // TODO: what is this?
                            break;

                        case EAuthSessionGuardType.k_EAuthSessionGuardType_MachineToken:
                            // ${u.De.LOGIN_BASE_URL}jwt/checkdevice - with steam machine guard cookie set
                            break;

                    }
                }

                while ( true )
                {
                    // TODO: For guard type none we don't need delay
                    await Task.Delay( PollingInterval );

                    var pollResponse = await PollAuthSessionStatus();

                    if ( pollResponse.refresh_token.Length > 0 )
                    {
                        return new AuthComplete
                        {
                            AccessToken = pollResponse.access_token,
                            RefreshToken = pollResponse.refresh_token,
                            AccountName = pollResponse.account_name,
                        };
                    }
                }
            }

            public async Task<CAuthentication_PollAuthSessionStatus_Response> PollAuthSessionStatus()
            {
                var request = new CAuthentication_PollAuthSessionStatus_Request
                {
                    client_id = ClientID,
                    request_id = RequestID,
                };

                var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
                var contentService = unifiedMessages.CreateService<IAuthentication>();
                var message = await contentService.SendMessage( api => api.PollAuthSessionStatus( request ) );
                var response = message.GetDeserializedResponse<CAuthentication_PollAuthSessionStatus_Response>();

                // eresult can be Expired, FileNotFound, Fail

                if ( response.new_client_id > 0 )
                {
                    ClientID = response.new_client_id;
                }

                if ( this is QrAuthSession qrResponse && response.new_challenge_url.Length > 0 )
                {
                    qrResponse.ChallengeURL = response.new_challenge_url;
                }

                return response;
            }
        }

        public sealed class QrAuthSession : AuthSession
        {
            public string ChallengeURL { get; set; }
        }

        public sealed class CredentialsAuthSession : AuthSession
        {
            public SteamID SteamID { get; set; }

            public async Task SendSteamGuardCode( string code, EAuthSessionGuardType codeType )
            {
                var request = new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request
                {
                    client_id = ClientID,
                    steamid = SteamID,
                    code = code,
                    code_type = codeType,
                };

                var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
                var contentService = unifiedMessages.CreateService<IAuthentication>();
                var message = await contentService.SendMessage( api => api.UpdateAuthSessionWithSteamGuardCode( request ) );
                var response = message.GetDeserializedResponse<CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response>();

                // can be InvalidLoginAuthCode, TwoFactorCodeMismatch, Expired
                if ( message.Result != EResult.OK )
                {
                    throw new Exception( $"Failed to send steam guard code with result {message.Result}" );
                }
            }
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

            if ( message.Result != EResult.OK )
            {
                throw new Exception( $"Failed to get password public key with result {message.Result}" );
            }

            var response = message.GetDeserializedResponse<CAuthentication_GetPasswordRSAPublicKey_Response>();

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        public async Task<QrAuthSession> BeginAuthSessionViaQR( AuthSessionDetails details )
        {
            var request = new CAuthentication_BeginAuthSessionViaQR_Request
            {
                platform_type = details.PlatformType,
                device_friendly_name = details.DeviceFriendlyName,
            };

            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var contentService = unifiedMessages.CreateService<IAuthentication>();
            var message = await contentService.SendMessage( api => api.BeginAuthSessionViaQR( request ) );

            if ( message.Result != EResult.OK )
            {
                throw new Exception( $"Failed to begin QR auth session with result {message.Result}" );
            }

            var response = message.GetDeserializedResponse<CAuthentication_BeginAuthSessionViaQR_Response>();

            var authResponse = new QrAuthSession
            {
                Client = Client,
                ClientID = response.client_id,
                RequestID = response.request_id,
                AllowedConfirmations = response.allowed_confirmations,
                PollingInterval = TimeSpan.FromSeconds( ( double )response.interval ),
                ChallengeURL = response.challenge_url,
            };

            return authResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        /// <exception cref="ArgumentNullException">No auth details were provided.</exception>
        /// <exception cref="ArgumentException">Username or password are not set within <paramref name="details"/>.</exception>
        public async Task<CredentialsAuthSession> BeginAuthSessionViaCredentials( AuthSessionDetails details )
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

            // eresult can be InvalidPassword, ServiceUnavailable
            if ( message.Result != EResult.OK )
            {
                throw new Exception( $"Authentication failed with result {message.Result}" );
            }

            var response = message.GetDeserializedResponse<CAuthentication_BeginAuthSessionViaCredentials_Response>();

            var authResponse = new CredentialsAuthSession
            {
                Client = Client,
                ClientID = response.client_id,
                RequestID = response.request_id,
                AllowedConfirmations = response.allowed_confirmations,
                PollingInterval = TimeSpan.FromSeconds( ( double )response.interval ),
                SteamID = new SteamID( response.steamid ),
            };

            return authResponse;
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
