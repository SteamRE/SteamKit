/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with apps and packages on the Steam network.
    /// </summary>
    public sealed partial class SteamApps : ClientMsgHandler
    {

// Ambiguous reference in cref attribute: 'SteamApps.GetPackageInfo'. Assuming 'SteamKit2.SteamApps.GetPackageInfo(uint, bool)',
// but could have also matched other overloads including 'SteamKit2.SteamApps.GetPackageInfo(System.Collections.Generic.IEnumerable<uint>, bool)'.
#pragma warning disable 0419

        /// <summary>
        /// Represents app request details when calling <see cref="SteamApps.GetAppInfo"/>.
        /// </summary>
        public sealed class AppDetails
        {
            /// <summary>
            /// Gets or sets the AppID for this request.
            /// </summary>
            /// <value>The AppID.</value>
            public uint AppID { get; set; }

            /// <summary>
            /// Gets or sets the section flags for this request.
            /// </summary>
            /// <value>The section flags.</value>
            public uint SectionFlags { get; set; }
            /// <summary>
            /// Gets the Section CRC list for this request.
            /// </summary>
            public List<uint> SectionCRC { get; private set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="AppDetails"/> class.
            /// </summary>
            public AppDetails()
            {
                // request all sections by default
                SectionFlags = 0xFFFF;
                SectionCRC = new List<uint>();
            }
        }

        /// <summary>
        /// Represents a PICS request used for <see cref="SteamApps.PICSGetProductInfo"/>
        /// </summary>
        public sealed class PICSRequest
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
            /// Requests only public app info
            /// </summary>
            /// <value>The flag specifying if only public data is requested</value>
            public bool Public { get; set; }

            /// <summary>
            /// Instantiate an empty PICS product info request
            /// </summary>
            public PICSRequest() : this( 0, 0, true )
            {
            }

            /// <summary>
            ///  Instantiate a PICS product info request for a given app or package id
            /// </summary>
            /// <param name="id">App or package ID</param>
            public PICSRequest( uint id ) : this( id, 0, true )
            {
            }

            /// <summary>
            /// Instantiate a PICS product info request for a given app or package id and an access token
            /// </summary>
            /// <param name="id">App or package ID</param>
            /// <param name="access_token">PICS access token</param>
            /// <param name="only_public">Get only public info</param>
            public PICSRequest( uint id, ulong access_token, bool only_public )
            {
                ID = id;
                AccessToken = access_token;
                Public = only_public;
            }
        }
#pragma warning restore 0419


        internal SteamApps()
        {
        }


        /// <summary>
        /// Requests an app ownership ticket for the specified AppID.
        /// Results are returned in a <see cref="AppOwnershipTicketCallback"/> callback.
        /// </summary>
        /// <param name="appid">The appid to request the ownership ticket of.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetAppOwnershipTicket( uint appid )
        {
            var request = new ClientMsgProtobuf<CMsgClientGetAppOwnershipTicket>( EMsg.ClientGetAppOwnershipTicket );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = appid;

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Requests app information for a single app. Use the overload for requesting information on a batch of apps.
        /// Results are returned in a <see cref="AppInfoCallback"/> callback.
        /// </summary>
        /// <param name="app">The app to request information for.</param>
        /// <param name="supportsBatches">if set to <c>true</c>, the request supports batches.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetAppInfo( AppDetails app, bool supportsBatches = false )
        {
            return GetAppInfo( new AppDetails[] { app }, supportsBatches );
        }
        /// <summary>
        /// Requests app information for a single app. Use the overload for requesting information on a batch of apps.
        /// Results are returned in a <see cref="AppInfoCallback"/> callback.
        /// </summary>
        /// <param name="app">The app to request information for.</param>
        /// <param name="supportsBatches">if set to <c>true</c>, the request supports batches.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetAppInfo( uint app, bool supportsBatches = false )
        {
            return GetAppInfo( new uint[] { app }, supportsBatches );
        }
        /// <summary>
        /// Requests app information for a list of apps.
        /// Results are returned in a <see cref="AppInfoCallback"/> callback.
        /// </summary>
        /// <param name="apps">The apps to request information for.</param>
        /// <param name="supportsBatches">if set to <c>true</c>, the request supports batches.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetAppInfo( IEnumerable<uint> apps, bool supportsBatches = false )
        {
            return GetAppInfo( apps.Select( a => new AppDetails { AppID = a } ), supportsBatches );
        }
        /// <summary>
        /// Requests app information for a list of apps.
        /// Results are returned in a <see cref="AppInfoCallback"/> callback.
        /// </summary>
        /// <param name="apps">The apps to request information for.</param>
        /// <param name="supportsBatches">if set to <c>true</c>, the request supports batches.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetAppInfo( IEnumerable<AppDetails> apps, bool supportsBatches = false )
        {
            var request = new ClientMsgProtobuf<CMsgClientAppInfoRequest>( EMsg.ClientAppInfoRequest );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.apps.AddRange( apps.Select( a =>
            {
                var app = new CMsgClientAppInfoRequest.App
                {
                    app_id = a.AppID,
                    section_flags = a.SectionFlags,
                };

                app.section_CRC.AddRange( a.SectionCRC );

                return app;
            } ) );

            request.Body.supports_batches = supportsBatches;

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Requests package information for a single package. Use the overload for requesting information on a batch of packages.
        /// Results are returned in a <see cref="PackageInfoCallback"/> callback.
        /// </summary>
        /// <param name="packageId">The package id to request information for.</param>
        /// <param name="metaDataOnly">if set to <c>true</c>, request metadata only.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetPackageInfo( uint packageId, bool metaDataOnly = false )
        {
            return GetPackageInfo( new uint[] { packageId }, metaDataOnly );
        }
        /// <summary>
        /// Requests package information for a list of packages.
        /// Results are returned in a <see cref="PackageInfoCallback"/> callback.
        /// </summary>
        /// <param name="packageId">The packages to request information for.</param>
        /// <param name="metaDataOnly">if set to <c>true</c> to request metadata only.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetPackageInfo( IEnumerable<uint> packageId, bool metaDataOnly = false )
        {
            var request = new ClientMsgProtobuf<CMsgClientPackageInfoRequest>( EMsg.ClientPackageInfoRequest );

            request.SourceJobID = Client.GetNextJobID();

            request.Body.package_ids.AddRange( packageId );
            request.Body.meta_data_only = metaDataOnly;

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Requests a list of app changes since the last provided change number value.
        /// Results are returned in a <see cref="AppChangesCallback"/> callback.
        /// </summary>
        /// <param name="lastChangeNumber">The last change number value.</param>
        /// <param name="sendChangelist">if set to <c>true</c>, request a change list.</param>
        public void GetAppChanges( uint lastChangeNumber = 0, bool sendChangelist = true  )
        {
            var request = new ClientMsgProtobuf<CMsgClientAppInfoUpdate>( EMsg.ClientAppInfoUpdate );

            request.Body.last_changenumber = lastChangeNumber;
            request.Body.send_changelist = sendChangelist;

            this.Client.Send( request );
        }

        /// <summary>
        /// Request the depot decryption key for a specified DepotID.
        /// Results are returned in a <see cref="DepotKeyCallback"/> callback.
        /// </summary>
        /// <param name="depotid">The DepotID to request a decryption key for.</param>
        /// <param name="appid">The AppID to request the decryption key for.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID GetDepotDecryptionKey( uint depotid, uint appid = 0 )
        {
            var request = new ClientMsgProtobuf<CMsgClientGetDepotDecryptionKey>( EMsg.ClientGetDepotDecryptionKey );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.depot_id = depotid;
            request.Body.app_id = appid;

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Request PICS access tokens for a list of app ids and package ids
        /// Results are returned in a <see cref="PICSTokensCallback"/> callback.
        /// </summary>
        /// <param name="appIds">List of app ids to request access tokens for.</param>
        /// <param name="packageIds">List of package ids to request access tokens for.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID PICSGetAccessTokens( IEnumerable<uint> appIds, IEnumerable<uint> packageIds )
        {
            var request = new ClientMsgProtobuf<CMsgPICSAccessTokenRequest>( EMsg.PICSAccessTokenRequest );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.packageids.AddRange( packageIds );
            request.Body.appids.AddRange( appIds );

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Request changes for apps and packages since a given change number
        /// Results are returned in a <see cref="PICSChangesCallback"/> callback.
        /// </summary>
        /// <param name="lastChangeNumber">Last change number seen.</param>
        /// <param name="sendAppChangelist">Whether to send app changes.</param>
        /// <param name="sendPackageChangelist">Whether to send package changes.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID PICSGetChangesSince( uint lastChangeNumber = 0, bool sendAppChangelist = true, bool sendPackageChangelist = false )
        {
            var request = new ClientMsgProtobuf<CMsgPICSChangesSinceRequest>( EMsg.PICSChangesSinceRequest );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.since_change_number = lastChangeNumber;
            request.Body.send_app_info_changes = sendAppChangelist;
            request.Body.send_package_info_changes = sendPackageChangelist;

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Request product information for an app or package
        /// Results are returned in a <see cref="PICSProductInfoCallback"/> callback.
        /// </summary>
        /// <param name="app">App id requested.</param>
        /// <param name="package">Package id requested.</param>
        /// <param name="onlyPublic">Whether to send only public information.</param>
        /// <param name="metaDataOnly">Whether to send only meta data.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID PICSGetProductInfo(uint? app, uint? package, bool onlyPublic = true, bool metaDataOnly = false)
        {
            List<uint> apps = new List<uint>();
            List<uint> packages = new List<uint>();

            if ( app.HasValue ) apps.Add( app.Value );
            if ( package.HasValue ) packages.Add( package.Value );

            return PICSGetProductInfo( apps, packages, onlyPublic, metaDataOnly );
        }

        /// <summary>
        /// Request product information for a list of apps or packages
        /// Results are returned in a <see cref="PICSProductInfoCallback"/> callback.
        /// </summary>
        /// <param name="apps">List of app ids requested.</param>
        /// <param name="packages">List of package ids requested.</param>
        /// <param name="onlyPublic">Whether to send only public information.</param>
        /// <param name="metaDataOnly">Whether to send only meta data.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID PICSGetProductInfo( IEnumerable<uint> apps, IEnumerable<uint> packages, bool onlyPublic = true, bool metaDataOnly = false )
        {
            return PICSGetProductInfo( apps.Select( app => new PICSRequest( app, 0, onlyPublic ) ), packages.Select( package => new PICSRequest( package ) ), metaDataOnly );
        }

        /// <summary>
        /// Request product information for a list of apps or packages
        /// Results are returned in a <see cref="PICSProductInfoCallback"/> callback.
        /// </summary>
        /// <param name="apps">List of <see cref="PICSRequest"/> requests for apps.</param>
        /// <param name="packages">List of <see cref="PICSRequest"/> requests for packages.</param>
        /// <param name="metaDataOnly">Whether to send only meta data.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID PICSGetProductInfo( IEnumerable<PICSRequest> apps, IEnumerable<PICSRequest> packages, bool metaDataOnly = false )
        {
            var request = new ClientMsgProtobuf<CMsgPICSProductInfoRequest>( EMsg.PICSProductInfoRequest );
            request.SourceJobID = Client.GetNextJobID();

            foreach ( var app_request in apps )
            {
                var appinfo = new CMsgPICSProductInfoRequest.AppInfo();
                appinfo.access_token = app_request.AccessToken;
                appinfo.appid = app_request.ID;
                appinfo.only_public = app_request.Public;

                request.Body.apps.Add( appinfo );
            }

            foreach ( var package_request in packages )
            {
                var packageinfo = new CMsgPICSProductInfoRequest.PackageInfo();
                packageinfo.access_token = package_request.AccessToken;
                packageinfo.packageid = package_request.ID;

                request.Body.packages.Add( packageinfo );
            }

            request.Body.meta_data_only = metaDataOnly;

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            switch ( packetMsg.MsgType )
            {
                case EMsg.ClientLicenseList:
                    HandleLicenseList( packetMsg );
                    break;

                case EMsg.ClientGameConnectTokens:
                    HandleGameConnectTokens( packetMsg );
                    break;

                case EMsg.ClientVACBanStatus:
                    HandleVACBanStatus( packetMsg );
                    break;

                case EMsg.ClientGetAppOwnershipTicketResponse:
                    HandleAppOwnershipTicketResponse( packetMsg );
                    break;

                case EMsg.ClientAppInfoResponse:
                    HandleAppInfoResponse( packetMsg );
                    break;

                case EMsg.ClientPackageInfoResponse:
                    HandlePackageInfoResponse( packetMsg );
                    break;

                case EMsg.ClientAppInfoChanges:
                    HandleAppInfoChanges( packetMsg );
                    break;

                case EMsg.ClientGetDepotDecryptionKeyResponse:
                    HandleDepotKeyResponse( packetMsg );
                    break;

                case EMsg.PICSAccessTokenResponse:
                    HandlePICSAccessTokenResponse( packetMsg );
                    break;

                case EMsg.PICSChangesSinceResponse:
                    HandlePICSChangesSinceResponse( packetMsg );
                    break;

                case EMsg.PICSProductInfoResponse:
                    HandlePICSProductInfoResponse( packetMsg );
                    break;
            }
        }


        #region ClientMsg Handlers
        void HandleAppOwnershipTicketResponse( IPacketMsg packetMsg )
        {
            var ticketResponse = new ClientMsgProtobuf<CMsgClientGetAppOwnershipTicketResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new AppOwnershipTicketCallback( Client, ticketResponse.Body );
            var callback = new SteamClient.JobCallback<AppOwnershipTicketCallback>( Client, ticketResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new AppOwnershipTicketCallback( ticketResponse.Body );
            var callback = new SteamClient.JobCallback<AppOwnershipTicketCallback>( ticketResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleAppInfoResponse( IPacketMsg packetMsg )
        {
            var infoResponse = new ClientMsgProtobuf<CMsgClientAppInfoResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new AppInfoCallback( Client, infoResponse.Body );
            var callback = new SteamClient.JobCallback<AppInfoCallback>( Client, infoResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new AppInfoCallback( infoResponse.Body );
            var callback = new SteamClient.JobCallback<AppInfoCallback>( infoResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        void HandlePackageInfoResponse( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientPackageInfoResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new PackageInfoCallback( Client, response.Body );
            var callback = new SteamClient.JobCallback<PackageInfoCallback>( Client, response.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new PackageInfoCallback( response.Body );
            var callback = new SteamClient.JobCallback<PackageInfoCallback>( response.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleAppInfoChanges( IPacketMsg packetMsg )
        {
            var changes = new ClientMsgProtobuf<CMsgClientAppInfoChanges>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new AppChangesCallback( Client, changes.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new AppChangesCallback( changes.Body );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleDepotKeyResponse( IPacketMsg packetMsg )
        {
            var keyResponse = new ClientMsgProtobuf<CMsgClientGetDepotDecryptionKeyResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new DepotKeyCallback( Client, keyResponse.Body );
            var callback = new SteamClient.JobCallback<DepotKeyCallback>( Client, keyResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new DepotKeyCallback( keyResponse.Body );
            var callback = new SteamClient.JobCallback<DepotKeyCallback>( keyResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleGameConnectTokens( IPacketMsg packetMsg )
        {
            var gcTokens = new ClientMsgProtobuf<CMsgClientGameConnectTokens>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new GameConnectTokensCallback( Client, gcTokens.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new GameConnectTokensCallback( gcTokens.Body );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleLicenseList( IPacketMsg packetMsg )
        {
            var licenseList = new ClientMsgProtobuf<CMsgClientLicenseList>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new LicenseListCallback( Client, licenseList.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new LicenseListCallback( licenseList.Body );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleVACBanStatus( IPacketMsg packetMsg )
        {
            Debug.Assert( !packetMsg.IsProto );

            var vacStatus = new ClientMsg<MsgClientVACBanStatus>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new VACStatusCallback( Client, vacStatus.Body, vacStatus.Payload.ToArray() );
            SteamClient.PostCallback( callback );
#else
            var callback = new VACStatusCallback( vacStatus.Body, vacStatus.Payload.ToArray() );
            this.Client.PostCallback( callback );
#endif
        }
        void HandlePICSAccessTokenResponse( IPacketMsg packetMsg )
        {
            Debug.Assert( packetMsg.IsProto );

            var tokensResponse = new ClientMsgProtobuf<CMsgPICSAccessTokenResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new PICSTokensCallback( Client, tokensResponse.Body );
            var callback = new SteamClient.JobCallback<PICSTokensCallback>( Client, tokensResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new PICSTokensCallback( tokensResponse.Body );
            var callback = new SteamClient.JobCallback<PICSTokensCallback>( tokensResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        void HandlePICSChangesSinceResponse( IPacketMsg packetMsg )
        {
            Debug.Assert( packetMsg.IsProto );

            var changesResponse = new ClientMsgProtobuf<CMsgPICSChangesSinceResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new PICSChangesCallback( Client, changesResponse.Body );
            var callback = new SteamClient.JobCallback<PICSChangesCallback>( Client, changesResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new PICSChangesCallback( changesResponse.Body );
            var callback = new SteamClient.JobCallback<PICSChangesCallback>( changesResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        void HandlePICSProductInfoResponse( IPacketMsg packetMsg )
        {
            Debug.Assert( packetMsg.IsProto );

            var productResponse = new ClientMsgProtobuf<CMsgPICSProductInfoResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new PICSProductInfoCallback( Client, productResponse.Body );
            var callback = new SteamClient.JobCallback<PICSProductInfoCallback>( Client, productResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new PICSProductInfoCallback( productResponse.Body );
            var callback = new SteamClient.JobCallback<PICSProductInfoCallback>( productResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion

    }
}
