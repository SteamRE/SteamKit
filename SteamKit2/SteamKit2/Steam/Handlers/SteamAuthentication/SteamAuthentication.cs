﻿/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
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

            /// <summary>
            /// 
            /// </summary>
            public IAuthenticator? Authenticator { get; set; }
        }

        public class AuthPollResult
        {
            public string AccountName { get; set; }
            public string RefreshToken { get; set; }
            public string AccessToken { get; set; }
        }

        public class AuthSession
        {
            public SteamClient Client { get; internal set; }
            public IAuthenticator? Authenticator { get; set; }
            public ulong ClientID { get; set; }
            public byte[] RequestID { get; set; }
            public List<CAuthentication_AllowedConfirmation> AllowedConfirmations { get; set; }
            public TimeSpan PollingInterval { get; set; }

            public async Task<AuthPollResult> StartPolling()
            {
                var pollLoop = false;
                var preferredConfirmation = AllowedConfirmations.FirstOrDefault();

                if ( preferredConfirmation == null || preferredConfirmation.confirmation_type == EAuthSessionGuardType.k_EAuthSessionGuardType_Unknown )
                {
                    throw new InvalidOperationException( "There are no allowed confirmations" );
                }

                switch ( preferredConfirmation.confirmation_type )
                {
                    case EAuthSessionGuardType.k_EAuthSessionGuardType_None:
                        // no steam guard
                        break;

                    case EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode:
                    case EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode:
                        if ( !( this is CredentialsAuthSession credentialsAuthSession ) )
                        {
                            throw new InvalidOperationException( $"Got {preferredConfirmation.confirmation_type} confirmation type in a session that is not {nameof( CredentialsAuthSession )}." );
                        }

                        if ( Authenticator == null )
                        {
                            throw new InvalidOperationException( $"This account requires an authenticator for login, but none was provided in {nameof( AuthSessionDetails )}." );
                        }

                        var task = preferredConfirmation.confirmation_type switch
                        {
                            EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode => Authenticator.ProvideEmailCode( preferredConfirmation.associated_message ),
                            EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode => Authenticator.ProvideDeviceCode(),
                            _ => throw new NotImplementedException(),
                        };

                        var code = await task;

                        if ( string.IsNullOrEmpty( code ) )
                        {
                            throw new InvalidOperationException( "No code was provided by the authenticator." );
                        }

                        await credentialsAuthSession.SendSteamGuardCode( code, preferredConfirmation.confirmation_type );

                        break;

                    case EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceConfirmation:
                        // TODO: is this accept prompt that automatically appears in the mobile app?
                        pollLoop = true;
                        break;

                    case EAuthSessionGuardType.k_EAuthSessionGuardType_EmailConfirmation:
                        // TODO: what is this?
                        pollLoop = true;
                        break;

                    case EAuthSessionGuardType.k_EAuthSessionGuardType_MachineToken:
                        // ${u.De.LOGIN_BASE_URL}jwt/checkdevice - with steam machine guard cookie set
                        throw new NotImplementedException( $"Machine token confirmation is not supported by SteamKit at the moment." );

                    default:
                        throw new NotImplementedException( $"Unsupported confirmation type {preferredConfirmation.confirmation_type}." );
                }

                if ( !pollLoop )
                {
                    var pollResponse = await PollAuthSessionStatus();

                    if ( pollResponse == null )
                    {
                        throw new Exception( "Auth failed" );
                    }

                    return pollResponse;
                }

                while ( true )
                {
                    // TODO: Realistically we only need to poll for confirmation-based (like qr, or device confirm) types
                    // TODO: For guard type none we don't need delay
                    await Task.Delay( PollingInterval );

                    var pollResponse = await PollAuthSessionStatus();

                    if( pollResponse != null )
                    {
                        return pollResponse;
                    }
                }
            }

            public async Task<AuthPollResult?> PollAuthSessionStatus()
            {
                var request = new CAuthentication_PollAuthSessionStatus_Request
                {
                    client_id = ClientID,
                    request_id = RequestID,
                };

                var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
                var contentService = unifiedMessages.CreateService<IAuthentication>();
                var message = await contentService.SendMessage( api => api.PollAuthSessionStatus( request ) );

                // eresult can be Expired, FileNotFound, Fail
                if ( message.Result != EResult.OK )
                {
                    throw new Exception( $"Failed to poll with result {message.Result}" );
                }

                var response = message.GetDeserializedResponse<CAuthentication_PollAuthSessionStatus_Response>();

                if ( response.new_client_id > 0 )
                {
                    ClientID = response.new_client_id;
                }

                if ( this is QrAuthSession qrResponse && response.new_challenge_url.Length > 0 )
                {
                    qrResponse.ChallengeURL = response.new_challenge_url;
                }

                if ( response.refresh_token.Length > 0 )
                {
                    return new AuthPollResult
                    {
                        AccessToken = response.access_token,
                        RefreshToken = response.refresh_token,
                        AccountName = response.account_name,
                    };
                }

                return null;
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
                AllowedConfirmations = SortConfirmations( response.allowed_confirmations ),
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
                Authenticator = details.Authenticator,
                ClientID = response.client_id,
                RequestID = response.request_id,
                AllowedConfirmations = SortConfirmations( response.allowed_confirmations ),
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

        private static List<CAuthentication_AllowedConfirmation> SortConfirmations( List<CAuthentication_AllowedConfirmation> confirmations )
        {
            /*
            valve's preferred order:
            0. k_EAuthSessionGuardType_DeviceConfirmation = 4, poll
            1. k_EAuthSessionGuardType_DeviceCode = 3, no poll
            2. k_EAuthSessionGuardType_EmailCode = 2, no poll
            3. k_EAuthSessionGuardType_None = 1, instant poll
            4. k_EAuthSessionGuardType_Unknown = 0,
            5. k_EAuthSessionGuardType_EmailConfirmation = 5, poll
            k_EAuthSessionGuardType_MachineToken = 6, checkdevice then instant poll
            */
            var preferredConfirmationTypes = new EAuthSessionGuardType[]
            {
                EAuthSessionGuardType.k_EAuthSessionGuardType_None,
                EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceConfirmation,
                EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode,
                EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode,
                EAuthSessionGuardType.k_EAuthSessionGuardType_EmailConfirmation,
                EAuthSessionGuardType.k_EAuthSessionGuardType_MachineToken,
                EAuthSessionGuardType.k_EAuthSessionGuardType_Unknown,
            };
            var sortOrder = Enumerable.Range( 0, preferredConfirmationTypes.Length ).ToDictionary( x => preferredConfirmationTypes[ x ], x => x );

            return confirmations.OrderBy( x =>
            {
                if( sortOrder.TryGetValue( x.confirmation_type, out var sortIndex ) )
                {
                    return sortIndex;
                }

                return int.MaxValue;
            } ).ToList();
        }
    }
}