/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// Represents an authentication sesssion which can be used to finish the authentication and get access tokens.
    /// </summary>
    public class AuthSession
    {
        /// <summary>
        /// Instance of <see cref="SteamAuthentication"/> that created this authentication session.
        /// </summary>
        private protected SteamAuthentication Authentication { get; }

        /// <summary>
        /// Confirmation types that will be able to confirm the request.
        /// </summary>
        List<CAuthentication_AllowedConfirmation> AllowedConfirmations;

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
        public async Task<AuthPollResult> PollingWaitForResultAsync( CancellationToken cancellationToken = default )
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
                var prefersToPollForConfirmation = await Authenticator.AcceptDeviceConfirmationAsync().ConfigureAwait( false );

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
                    if ( this is not CredentialsAuthSession credentialsAuthSession )
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
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var task = preferredConfirmation.confirmation_type switch
                            {
                                EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode => Authenticator.GetEmailCodeAsync( preferredConfirmation.associated_message, previousCodeWasIncorrect ),
                                EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode => Authenticator.GetDeviceCodeAsync( previousCodeWasIncorrect ),
                                _ => throw new NotImplementedException(),
                            };

                            var code = await task.ConfigureAwait( false );

                            cancellationToken.ThrowIfCancellationRequested();

                            if ( string.IsNullOrEmpty( code ) )
                            {
                                throw new InvalidOperationException( "No code was provided by the authenticator." );
                            }

                            await credentialsAuthSession.SendSteamGuardCodeAsync( code, preferredConfirmation.confirmation_type ).ConfigureAwait( false );

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
                cancellationToken.ThrowIfCancellationRequested();

                var pollResponse = await PollAuthSessionStatusAsync().ConfigureAwait( false ) ?? throw new AuthenticationException( "Authentication failed", EResult.Fail );
                return pollResponse;
            }

            while ( true )
            {
                await Task.Delay( PollingInterval, cancellationToken ).ConfigureAwait( false );

                var pollResponse = await PollAuthSessionStatusAsync().ConfigureAwait( false );

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

            var message = await Authentication.AuthenticationService.SendMessage( api => api.PollAuthSessionStatus( request ) );

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

        /// <summary>
        /// Handles poll authentication session status response.
        /// </summary>
        /// <param name="response">The response.</param>
        protected virtual void HandlePollAuthSessionStatusResponse( CAuthentication_PollAuthSessionStatus_Response response )
        {
            if ( response.new_client_id != default )
            {
                ClientID = response.new_client_id;
            }
        }

        /// <summary>
        /// Sort available guard confirmation methods by an order that we prefer to handle them in.
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
