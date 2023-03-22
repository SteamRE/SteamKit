/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
            public string? DeviceFriendlyName { get; set; } = $"{Environment.MachineName} (SteamKit2)";

            /// <summary>
            /// Gets or sets the platform type that the login will be performed for.
            /// </summary>
            public EAuthTokenPlatformType PlatformType { get; set; } = EAuthTokenPlatformType.k_EAuthTokenPlatformType_SteamClient;

            /// <summary>
            /// Gets or sets the client operating system type.
            /// </summary>
            /// <value>The client operating system type.</value>
            public EOSType ClientOSType { get; set; } = Utils.GetOSType();

            /// <summary>
            /// Gets or sets the session persistence.
            /// </summary>
            /// <value>The persistence.</value>
            public bool IsPersistentSession { get; set; } = false;

            /// <summary>
            /// Gets or sets the website id that the login will be performed for.
            /// Known values are "Unknown", "Client", "Mobile", "Website", "Store", "Community", "Partner", "SteamStats".
            /// </summary>
            /// <value>The website id.</value>
            public string? WebsiteID { get; set; } = "Client";

            /// <summary>
            /// Steam guard data for client login. Provide <see cref="AuthPollResult.NewGuardData"/> if available.
            /// </summary>
            /// <value>The guard data.</value>
            public string? GuardData { get; set; }

            /// <summary>
            /// Authenticator object which will be used to handle 2-factor authentication if necessary.
            /// Use <see cref="UserConsoleAuthenticator"/> for a default implementation.
            /// </summary>
            /// <value>The authenticator object.</value>
            public IAuthenticator? Authenticator { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class AuthPollResult
        {
            /// <summary>
            /// Account name of authenticating account.
            /// </summary>
            public string AccountName { get; }
            /// <summary>
            /// New refresh token.
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

        /// <summary>
        /// Represents an authentication sesssion which can be used to finish the authentication and get access tokens.
        /// </summary>
        public class AuthSession
        {
            internal SteamAuthentication Authentication { get; }

            /// <summary>
            /// Confirmation types that will be able to confirm the request.
            /// </summary>
            internal List<CAuthentication_AllowedConfirmation> AllowedConfirmations { get; }

            /// <summary>
            /// Authenticator object which will be used to handle 2-factor authentication if necessary.
            /// </summary>
            public IAuthenticator? Authenticator { get; }
            /// <summary>
            /// Unique identifier of requestor, also used for routing, portion of QR code.
            /// </summary>
            public ulong ClientID { get; internal set; }
            /// <summary>
            /// Unique request ID to be presented by requestor at poll time.
            /// </summary>
            public byte[] RequestID { get; }
            /// <summary>
            /// Refresh interval with which requestor should call PollAuthSessionStatus.
            /// </summary>
            public TimeSpan PollingInterval { get; }

            internal AuthSession( SteamAuthentication authentication, IAuthenticator? authenticator, ulong clientId, byte[] requestId, List<CAuthentication_AllowedConfirmation> allowedConfirmations, float pollingInterval )
            {
                Authentication = authentication;
                Authenticator = authenticator;
                ClientID = clientId;
                RequestID = requestId;
                AllowedConfirmations = SortConfirmations( allowedConfirmations );
                PollingInterval = TimeSpan.FromSeconds( ( double )pollingInterval );
            }

            /// <summary>
            /// Handle any 2-factor authentication, and if necessary poll for updates until authentication succeeds.
            /// </summary>
            /// <returns>An object containing tokens which can be used to login to Steam.</returns>
            /// <exception cref="InvalidOperationException">Thrown when an invalid state occurs, such as no supported confirmation methods are available.</exception>
            /// <exception cref="AuthenticationException">Thrown when polling fails.</exception>
            public async Task<AuthPollResult> PollingWaitForResultAsync( CancellationToken? cancellationToken = null )
            {
                var pollLoop = false;
                var preferredConfirmation = AllowedConfirmations.FirstOrDefault();

                if ( preferredConfirmation == null || preferredConfirmation.confirmation_type == EAuthSessionGuardType.k_EAuthSessionGuardType_Unknown )
                {
                    throw new InvalidOperationException( "There are no allowed confirmations" );
                }

                // If an authenticator is provided and we device confirmation is available, allow consumers to choose whether they want to
                // simply poll until confirmation is accepted, or whether they want to fallback to the next preferred confirmation type.
                if ( Authenticator != null && preferredConfirmation.confirmation_type == EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceConfirmation )
                {
                    var prefersToPollForConfirmation = await Authenticator.AcceptDeviceConfirmationAsync();

                    if ( !prefersToPollForConfirmation )
                    {
                        if ( AllowedConfirmations.Count <= 1 )
                        {
                            throw new InvalidOperationException( "AcceptDeviceConfirmation returned false which indicates a fallback to another confirmation type, but there are no other confirmation types available." );
                        }

                        preferredConfirmation = AllowedConfirmations[ 1 ];
                    }
                }

                switch ( preferredConfirmation.confirmation_type )
                {
                    // No steam guard
                    case EAuthSessionGuardType.k_EAuthSessionGuardType_None:
                        break;

                    // 2-factor code from the authenticator app or sent to an email
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

                        var expectedInvalidCodeResult = preferredConfirmation.confirmation_type switch
                        {
                            EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode => EResult.InvalidLoginAuthCode,
                            EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode => EResult.TwoFactorCodeMismatch,
                            _ => throw new NotImplementedException(),
                        };
                        var previousCodeWasIncorrect = false;
                        var waitingForValidCode = true;

                        do
                        {
                            cancellationToken?.ThrowIfCancellationRequested();

                            try
                            {
                                var task = preferredConfirmation.confirmation_type switch
                                {
                                    EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode => Authenticator.GetEmailCodeAsync( preferredConfirmation.associated_message, previousCodeWasIncorrect ),
                                    EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode => Authenticator.GetDeviceCodeAsync( previousCodeWasIncorrect ),
                                    _ => throw new NotImplementedException(),
                                };

                                var code = await task;

                                cancellationToken?.ThrowIfCancellationRequested();

                                if ( string.IsNullOrEmpty( code ) )
                                {
                                    throw new InvalidOperationException( "No code was provided by the authenticator." );
                                }

                                await credentialsAuthSession.SendSteamGuardCodeAsync( code, preferredConfirmation.confirmation_type );

                                waitingForValidCode = false;
                            }
                            catch ( AuthenticationException e ) when ( e.Result == expectedInvalidCodeResult )
                            {
                                previousCodeWasIncorrect = true;
                            }
                        }
                        while ( waitingForValidCode );

                        break;

                    // This is a prompt that appears in the Steam mobile app
                    case EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceConfirmation:
                        pollLoop = true;
                        break;

                    /*
                    case EAuthSessionGuardType.k_EAuthSessionGuardType_EmailConfirmation:
                        // TODO: what is this?
                        pollLoop = true;
                        break;

                    case EAuthSessionGuardType.k_EAuthSessionGuardType_MachineToken:
                        // ${u.De.LOGIN_BASE_URL}jwt/checkdevice - with steam machine guard cookie set
                        throw new NotImplementedException( $"Machine token confirmation is not supported by SteamKit at the moment." );
                    */

                    default:
                        throw new NotImplementedException( $"Unsupported confirmation type {preferredConfirmation.confirmation_type}." );
                }

                if ( !pollLoop )
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var pollResponse = await PollAuthSessionStatusAsync();

                    if ( pollResponse == null )
                    {
                        throw new AuthenticationException( "Authentication failed", EResult.Fail );
                    }

                    return pollResponse;
                }

                while ( true )
                {
                    if( cancellationToken is CancellationToken nonNullCancellationToken )
                    {
                        await Task.Delay( PollingInterval, nonNullCancellationToken );
                    }
                    else
                    {
                        await Task.Delay( PollingInterval );
                    }

                    var pollResponse = await PollAuthSessionStatusAsync();

                    if ( pollResponse != null )
                    {
                        return pollResponse;
                    }
                }
            }

            /// <summary>
            /// Polls for authentication status once. Prefer using <see cref="PollingWaitForResultAsync"/> instead.
            /// </summary>
            /// <returns>An object containing tokens which can be used to login to Steam, or null if not yet authenticated.</returns>
            /// <exception cref="AuthenticationException">Thrown when polling fails.</exception>
            public async Task<AuthPollResult?> PollAuthSessionStatusAsync()
            {
                var request = new CAuthentication_PollAuthSessionStatus_Request
                {
                    client_id = ClientID,
                    request_id = RequestID,
                };

                var message = await Authentication.AuthenticationService!.SendMessage( api => api.PollAuthSessionStatus( request ) );

                // eresult can be Expired, FileNotFound, Fail
                if ( message.Result != EResult.OK )
                {
                    throw new AuthenticationException( "Failed to poll status", message.Result );
                }

                var response = message.GetDeserializedResponse<CAuthentication_PollAuthSessionStatus_Response>();

                HandlePollAuthSessionStatusResponse( response );

                if ( response.refresh_token.Length > 0 )
                {
                    return new AuthPollResult( response );
                }

                return null;
            }

            internal virtual void HandlePollAuthSessionStatusResponse( CAuthentication_PollAuthSessionStatus_Response response)
            {
                if ( response.new_client_id != default )
                {
                    ClientID = response.new_client_id;
                }
            }
        }

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

            internal override void HandlePollAuthSessionStatusResponse( CAuthentication_PollAuthSessionStatus_Response response )
            {
                base.HandlePollAuthSessionStatusResponse( response );

                if ( response.new_challenge_url.Length > 0 )
                {
                    ChallengeURL = response.new_challenge_url;
                    ChallengeURLChanged?.Invoke();
                }
            }
        }

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

                var message = await Authentication.AuthenticationService!.SendMessage( api => api.UpdateAuthSessionWithSteamGuardCode( request ) );
                var response = message.GetDeserializedResponse<CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response>();

                // can be InvalidLoginAuthCode, TwoFactorCodeMismatch, Expired
                if ( message.Result != EResult.OK )
                {
                    throw new AuthenticationException( "Failed to send steam guard code", message.Result );
                }

                // response may contain agreement_session_url
            }
        }

        internal SteamUnifiedMessages.UnifiedService<IAuthentication>? AuthenticationService { get; private set; }

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

            var message = await AuthenticationService!.SendMessage( api => api.GetPasswordRSAPublicKey( request ) );

            if ( message.Result != EResult.OK )
            {
                throw new AuthenticationException( "Failed to get password public key", message.Result );
            }

            var response = message.GetDeserializedResponse<CAuthentication_GetPasswordRSAPublicKey_Response>();

            return response;
        }

        /// <summary>
        /// Start the authentication process using QR codes.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        public async Task<QrAuthSession> BeginAuthSessionViaQR( AuthSessionDetails details )
        {
            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            AuthenticationService = unifiedMessages.CreateService<IAuthentication>();

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

            var message = await AuthenticationService.SendMessage( api => api.BeginAuthSessionViaQR( request ) );

            if ( message.Result != EResult.OK )
            {
                throw new AuthenticationException( "Failed to begin QR auth session", message.Result );
            }

            var response = message.GetDeserializedResponse<CAuthentication_BeginAuthSessionViaQR_Response>();

            var authResponse = new QrAuthSession( this, details.Authenticator, response );

            return authResponse;
        }

        /// <summary>
        /// Start the authentication process by providing username and password.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        /// <exception cref="ArgumentNullException">No auth details were provided.</exception>
        /// <exception cref="ArgumentException">Username or password are not set within <paramref name="details"/>.</exception>
        public async Task<CredentialsAuthSession> BeginAuthSessionViaCredentials( AuthSessionDetails details )
        {
            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            AuthenticationService = unifiedMessages.CreateService<IAuthentication>();

            if ( details == null )
            {
                throw new ArgumentNullException( nameof( details ) );
            }

            if ( string.IsNullOrEmpty( details.Username ) || string.IsNullOrEmpty( details.Password ) )
            {
                throw new ArgumentException( "BeginAuthSessionViaCredentials requires a username and password to be set in 'details'." );
            }

            // Encrypt the password
            var publicKey = await GetPasswordRSAPublicKeyAsync( details.Username! );
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

            var message = await AuthenticationService.SendMessage( api => api.BeginAuthSessionViaCredentials( request ) );

            // eresult can be InvalidPassword, ServiceUnavailable, InvalidParam, RateLimitExceeded
            if ( message.Result != EResult.OK )
            {
                throw new AuthenticationException( "Authentication failed", message.Result );
            }

            var response = message.GetDeserializedResponse<CAuthentication_BeginAuthSessionViaCredentials_Response>();

            var authResponse = new CredentialsAuthSession( this, details.Authenticator, response );

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

        /// <summary>
        /// Sort available guard confirmation methods by an order that we prefer to handle them in
        /// </summary>
        static List<CAuthentication_AllowedConfirmation> SortConfirmations( List<CAuthentication_AllowedConfirmation> confirmations )
        {
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
                if ( sortOrder.TryGetValue( x.confirmation_type, out var sortIndex ) )
                {
                    return sortIndex;
                }

                return int.MaxValue;
            } ).ToList();
        }
    }
}
