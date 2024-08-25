/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with apps and packages on the Steam network.
    /// </summary>
    public sealed partial class SteamApps : ClientMsgHandler
    {
        /// <summary>
        /// Represents a PICS request used for <see cref="o:SteamApps.PICSGetProductInfo"/>
        /// </summary>
        public struct PICSRequest
        {
            /// <summary>
            /// Gets or sets the ID of the app or package being requested
            /// </summary>
            /// <value>The ID</value>
            public uint ID { get; set; }
            /// <summary>
            /// Gets or sets the access token associated with the request
            /// </summary>
            /// <value>The access token</value>
            public ulong AccessToken { get; set; }

            /// <summary>
            /// Instantiate a PICS product info request for a given app or package id and an access token
            /// </summary>
            /// <param name="id">App or package ID</param>
            /// <param name="access_token">PICS access token</param>
            public PICSRequest( uint id = 0, ulong access_token = 0 )
            {
                ID = id;
                AccessToken = access_token;
            }
        }

        private static CallbackMsg? GetCallback( IPacketMsg packetMsg ) => packetMsg.MsgType switch
        {
            EMsg.ClientLicenseList => new LicenseListCallback( packetMsg ),
            EMsg.ClientRequestFreeLicenseResponse => new FreeLicenseCallback( packetMsg ),
            EMsg.ClientPurchaseResponse => new PurchaseResponseCallback( packetMsg ),
            EMsg.ClientRedeemGuestPassResponse => new RedeemGuestPassResponseCallback( packetMsg ),
            EMsg.ClientGameConnectTokens => new GameConnectTokensCallback( packetMsg ),
            EMsg.ClientVACBanStatus => new VACStatusCallback( packetMsg ),
            EMsg.ClientGetAppOwnershipTicketResponse => new AppOwnershipTicketCallback( packetMsg ),
            EMsg.ClientGetDepotDecryptionKeyResponse => new DepotKeyCallback( packetMsg ),
            EMsg.ClientGetLegacyGameKeyResponse => new LegacyGameKeyCallback( packetMsg ),
            EMsg.ClientPICSAccessTokenResponse => new PICSTokensCallback( packetMsg ),
            EMsg.ClientPICSChangesSinceResponse => new PICSChangesCallback( packetMsg ),
            EMsg.ClientPICSProductInfoResponse => new PICSProductInfoCallback( packetMsg ),
            EMsg.ClientUpdateGuestPassesList => new GuestPassListCallback( packetMsg ),
            EMsg.ClientGetCDNAuthTokenResponse => new CDNAuthTokenCallback( packetMsg ),
            EMsg.ClientCheckAppBetaPasswordResponse => new CheckAppBetaPasswordCallback( packetMsg ),
            _ => null,
        };

        /// <summary>
        /// Requests an app ownership ticket for the specified AppID.
        /// Results are returned in a <see cref="AppOwnershipTicketCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appid">The appid to request the ownership ticket of.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="AppOwnershipTicketCallback"/>.</returns>
        public AsyncJob<AppOwnershipTicketCallback> GetAppOwnershipTicket( uint appid )
        {
            var request = new ClientMsgProtobuf<CMsgClientGetAppOwnershipTicket>( EMsg.ClientGetAppOwnershipTicket );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = appid;

            this.Client.Send( request );

            return new AsyncJob<AppOwnershipTicketCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Request the depot decryption key for a specified DepotID.
        /// Results are returned in a <see cref="DepotKeyCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="depotid">The DepotID to request a decryption key for.</param>
        /// <param name="appid">The AppID parent of the DepotID.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="DepotKeyCallback"/>.</returns>
        public AsyncJob<DepotKeyCallback> GetDepotDecryptionKey( uint depotid, uint appid )
        {
            var request = new ClientMsgProtobuf<CMsgClientGetDepotDecryptionKey>( EMsg.ClientGetDepotDecryptionKey );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.depot_id = depotid;
            request.Body.app_id = appid;

            this.Client.Send( request );

            return new AsyncJob<DepotKeyCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Request PICS access tokens for an app or package.
        /// Results are returned in a <see cref="PICSTokensCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="app">App id to request access token for.</param>
        /// <param name="package">Package id to request access token for.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="PICSTokensCallback"/>.</returns>
        public AsyncJob<PICSTokensCallback> PICSGetAccessTokens( uint? app, uint? package )
        {
            List<uint> apps = [];
            List<uint> packages = [];

            if ( app.HasValue ) apps.Add( app.Value );
            if ( package.HasValue ) packages.Add( package.Value );

            return PICSGetAccessTokens( apps, packages );
        }

        /// <summary>
        /// Request PICS access tokens for a list of app ids and package ids
        /// Results are returned in a <see cref="PICSTokensCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appIds">List of app ids to request access tokens for.</param>
        /// <param name="packageIds">List of package ids to request access tokens for.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="PICSTokensCallback"/>.</returns>
        public AsyncJob<PICSTokensCallback> PICSGetAccessTokens( IEnumerable<uint> appIds, IEnumerable<uint> packageIds )
        {
            var request = new ClientMsgProtobuf<CMsgClientPICSAccessTokenRequest>( EMsg.ClientPICSAccessTokenRequest );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.packageids.AddRange( packageIds );
            request.Body.appids.AddRange( appIds );

            this.Client.Send( request );

            return new AsyncJob<PICSTokensCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Request changes for apps and packages since a given change number
        /// Results are returned in a <see cref="PICSChangesCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="lastChangeNumber">Last change number seen.</param>
        /// <param name="sendAppChangelist">Whether to send app changes.</param>
        /// <param name="sendPackageChangelist">Whether to send package changes.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="PICSChangesCallback"/>.</returns>
        public AsyncJob<PICSChangesCallback> PICSGetChangesSince( uint lastChangeNumber = 0, bool sendAppChangelist = true, bool sendPackageChangelist = false )
        {
            var request = new ClientMsgProtobuf<CMsgClientPICSChangesSinceRequest>( EMsg.ClientPICSChangesSinceRequest );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.since_change_number = lastChangeNumber;
            request.Body.send_app_info_changes = sendAppChangelist;
            request.Body.send_package_info_changes = sendPackageChangelist;

            this.Client.Send( request );

            return new AsyncJob<PICSChangesCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Request product information for an app or package
        /// Results are returned in a <see cref="PICSProductInfoCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="app"><see cref="PICSRequest"/> request for an app.</param>
        /// <param name="package"><see cref="PICSRequest"/> request for a package.</param>
        /// <param name="metaDataOnly">Whether to send only meta data.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="PICSProductInfoCallback"/>.</returns>
        public AsyncJobMultiple<PICSProductInfoCallback> PICSGetProductInfo( PICSRequest? app, PICSRequest? package, bool metaDataOnly = false )
        {
            var apps = new List<PICSRequest>();
            var packages = new List<PICSRequest>();

            if ( app.HasValue ) apps.Add( app.Value );
            if ( package.HasValue ) packages.Add( package.Value );

            return PICSGetProductInfo( apps, packages, metaDataOnly );
        }

        /// <summary>
        /// Request product information for a list of apps or packages
        /// Results are returned in a <see cref="PICSProductInfoCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="apps">List of <see cref="PICSRequest"/> requests for apps.</param>
        /// <param name="packages">List of <see cref="PICSRequest"/> requests for packages.</param>
        /// <param name="metaDataOnly">Whether to send only meta data.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="PICSProductInfoCallback"/>.</returns>
        public AsyncJobMultiple<PICSProductInfoCallback> PICSGetProductInfo( IEnumerable<PICSRequest> apps, IEnumerable<PICSRequest> packages, bool metaDataOnly = false )
        {
            ArgumentNullException.ThrowIfNull( apps );

            ArgumentNullException.ThrowIfNull( packages );

            var request = new ClientMsgProtobuf<CMsgClientPICSProductInfoRequest>( EMsg.ClientPICSProductInfoRequest );
            request.SourceJobID = Client.GetNextJobID();

            foreach ( var app_request in apps )
            {
                var appinfo = new CMsgClientPICSProductInfoRequest.AppInfo();
                appinfo.access_token = app_request.AccessToken;
                appinfo.appid = app_request.ID;
                appinfo.only_public_obsolete = false;

                request.Body.apps.Add( appinfo );
            }

            foreach ( var package_request in packages )
            {
                var packageinfo = new CMsgClientPICSProductInfoRequest.PackageInfo();
                packageinfo.access_token = package_request.AccessToken;
                packageinfo.packageid = package_request.ID;

                request.Body.packages.Add( packageinfo );
            }

            request.Body.meta_data_only = metaDataOnly;

            this.Client.Send( request );

            return new AsyncJobMultiple<PICSProductInfoCallback>( this.Client, request.SourceJobID, static callback => !callback.ResponsePending );
        }


        /// <summary>
        /// Request product information for an app or package
        /// Results are returned in a <see cref="CDNAuthTokenCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="app">App id requested.</param>
        /// <param name="depot">Depot id requested.</param>
        /// <param name="host_name">CDN host name being requested.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="CDNAuthTokenCallback"/>.</returns>
        public AsyncJob<CDNAuthTokenCallback> GetCDNAuthToken( uint app, uint depot, string host_name )
        {
            var request = new ClientMsgProtobuf<CMsgClientGetCDNAuthToken>( EMsg.ClientGetCDNAuthToken );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = app;
            request.Body.depot_id = depot;
            request.Body.host_name = host_name;

            this.Client.Send( request );

            return new AsyncJob<CDNAuthTokenCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Request a free license for given appid, can be used for free on demand apps
        /// Results are returned in a <see cref="FreeLicenseCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="app">The app to request a free license for.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="FreeLicenseCallback"/>.</returns>
        public AsyncJob<FreeLicenseCallback> RequestFreeLicense( uint app )
        {
            return RequestFreeLicense( [ app ] );
        }
        /// <summary>
        /// Request a free license for given appids, can be used for free on demand apps
        /// Results are returned in a <see cref="FreeLicenseCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="apps">The apps to request a free license for.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="FreeLicenseCallback"/>.</returns>
        public AsyncJob<FreeLicenseCallback> RequestFreeLicense( IEnumerable<uint> apps )
        {
            ArgumentNullException.ThrowIfNull( apps );

            var request = new ClientMsgProtobuf<CMsgClientRequestFreeLicense>( EMsg.ClientRequestFreeLicense );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.appids.AddRange( apps );

            this.Client.Send( request );

            return new AsyncJob<FreeLicenseCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Submit a beta password for a given app to retrieve any betas and their encryption keys.
        /// Results are returned in a <see cref="CheckAppBetaPasswordCallback"/> callback.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="app">App id requested.</param>
        /// <param name="password">Password to check.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="CheckAppBetaPasswordCallback"/>.</returns>
        public AsyncJob<CheckAppBetaPasswordCallback> CheckAppBetaPassword( uint app, string password )
        {
            var request = new ClientMsgProtobuf<CMsgClientCheckAppBetaPassword>( EMsg.ClientCheckAppBetaPassword );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = app;
            request.Body.betapassword = password;

            this.Client.Send( request );

            return new AsyncJob<CheckAppBetaPasswordCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Request the legacy CD game keys for the requested appid.
        /// </summary>
        /// <param name="appid">The AppID to request game keys for.</param>
        public AsyncJob<LegacyGameKeyCallback> GetLegacyGameKey( uint appid )
        {
            var request = new ClientMsg<MsgClientGetLegacyGameKey>();
            request.SourceJobID = Client.GetNextJobID();
            request.Body.AppId = appid;

            this.Client.Send( request );

            return new AsyncJob<LegacyGameKeyCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            var callback = GetCallback( packetMsg );

            if ( callback == null )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            this.Client.PostCallback( callback );
        }
    }
}
