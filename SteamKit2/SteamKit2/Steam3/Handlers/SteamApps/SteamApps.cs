/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with apps and packages on the Steam network.
    /// </summary>
    public sealed partial class SteamApps : ClientMsgHandler
    {

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

            public AppDetails()
            {
                // request all sections by default
                SectionFlags = 0xFFFF;
                SectionCRC = new List<uint>();
            }
        }


        internal SteamApps()
        {
        }


        /// <summary>
        /// Requests an app ownership ticket for the specified AppID.
        /// Results are returned in a <see cref="AppOwnershipTicketCallback"/> callback.
        /// </summary>
        /// <param name="appid">The appid.</param>
        public void GetAppOwnershipTicket( uint appid )
        {
            var request = new ClientMsgProtobuf<MsgClientGetAppOwnershipTicket>();

            request.Msg.Proto.app_id = appid;

            this.Client.Send( request );
        }

        /// <summary>
        /// Requests app information for a single app. Use the overload for requesting information on a batch of apps.
        /// Results are returned in a <see cref="AppInfoCallback"/> callback.
        /// </summary>
        /// <param name="app">The app to request information for.</param>
        /// <param name="supportsBatches">if set to <c>true</c>, the request supports batches.</param>
        public void GetAppInfo( AppDetails app, bool supportsBatches = false )
        {
            GetAppInfo( new AppDetails[] { app }, supportsBatches );
        }
        /// <summary>
        /// Requests app information for a list of apps.
        /// Results are returned in a <see cref="AppInfoCallback"/> callback.
        /// </summary>
        /// <param name="apps">The apps to request information for.</param>
        /// <param name="supportsBatches">if set to <c>true</c>, the request supports batches.</param>
        public void GetAppInfo( IEnumerable<uint> apps, bool supportsBatches = false )
        {
            GetAppInfo( apps.Select( a => new AppDetails { AppID = a } ), supportsBatches );
        }
        /// <summary>
        /// Requests app information for a list of apps.
        /// Results are returned in a <see cref="AppInfoCallback"/> callback.
        /// </summary>
        /// <param name="apps">The apps to request information for.</param>
        /// <param name="supportsBatches">if set to <c>true</c>, the request supports batches.</param>
        public void GetAppInfo( IEnumerable<AppDetails> apps, bool supportsBatches = false )
        {
            var request = new ClientMsgProtobuf<MsgClientAppInfoRequest>();

            request.Msg.Proto.apps.AddRange( apps.Select( a =>
            {
                var app = new CMsgClientAppInfoRequest.App
                {
                    app_id = a.AppID,
                    section_flags = a.SectionFlags,
                };

                app.section_CRC.AddRange( a.SectionCRC );

                return app;
            } ) );

            request.Msg.Proto.supports_batches = supportsBatches;

            this.Client.Send( request );
        }

        /// <summary>
        /// Requests package information for a single package. Use the overload for requesting information on a batch of packages.
        /// Results are returned in a <see cref="PackageInfoCallback"/> callback.
        /// </summary>
        /// <param name="packageId">The package id to request information for.</param>
        /// <param name="metaDataOnly">if set to <c>true</c>, request metadata only.</param>
        public void GetPackageInfo( uint packageId, bool metaDataOnly = false )
        {
            GetPackageInfo( new uint[] { packageId }, metaDataOnly );
        }
        /// <summary>
        /// Requests package information for a list of packages.
        /// Results are returned in a <see cref="PackageInfoCallback"/> callback.
        /// </summary>
        /// <param name="packageId">The packages to request information for.</param>
        /// <param name="metaDataOnly">if set to <c>true</c> to request metadata only.</param>
        public void GetPackageInfo( IEnumerable<uint> packageId, bool metaDataOnly = false )
        {
            var request = new ClientMsgProtobuf<MsgClientPackageInfoRequest>();

            request.Msg.Proto.package_ids.AddRange( packageId );
            request.Msg.Proto.meta_data_only = metaDataOnly;

            this.Client.Send( request );
        }

        /// <summary>
        /// Requests a list of app changes since the last provided change number value.
        /// Results are returned in a <see cref="AppChangesCallback"/> callback.
        /// </summary>
        /// <param name="lastChangeNumber">The last change number value.</param>
        /// <param name="sendChangelist">if set to <c>true</c>, request a change list.</param>
        public void GetAppChanges( uint lastChangeNumber = 0, bool sendChangelist = false  )
        {
            var request = new ClientMsgProtobuf<MsgClientAppInfoUpdate>();

            request.Msg.Proto.last_changenumber = lastChangeNumber;
            request.Msg.Proto.send_changelist = sendChangelist;

            this.Client.Send( request );
        }

        /// <summary>
        /// Request the depot decryption key for a specified DepotID.
        /// Results are returned in a <see cref="DepotKeyCallback"/> callback.
        /// </summary>
        /// <param name="depotid">The DepotID to request a decryption key for.</param>
        public void GetDepotDecryptionKey( uint depotid )
        {
            var request = new ClientMsg<MsgClientGetDepotDecryptionKey, ExtendedClientMsgHdr>();

            request.Msg.DepotID = depotid;

            this.Client.Send( request );
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="e">The <see cref="SteamKit2.ClientMsgEventArgs"/> instance containing the event data.</param>
        public override void HandleMsg( ClientMsgEventArgs e )
        {
            switch ( e.EMsg )
            {
                case EMsg.ClientLicenseList:
                    HandleLicenseList( e );
                    break;

                case EMsg.ClientGameConnectTokens:
                    HandleGameConnectTokens( e );
                    break;

                case EMsg.ClientVACBanStatus:
                    HandleVACBanStatus( e );
                    break;

                case EMsg.ClientGetAppOwnershipTicketResponse:
                    HandleAppOwnershipTicketResponse( e );
                    break;

                case EMsg.ClientAppInfoResponse:
                    HandleAppInfoResponse( e );
                    break;

                case EMsg.ClientPackageInfoResponse:
                    HandlePackageInfoResponse( e );
                    break;

                case EMsg.ClientAppInfoChanges:
                    HandleAppInfoChanges( e );
                    break;

                case EMsg.ClientGetDepotDecryptionKeyResponse:
                    HandleDepotKeyResponse(e);
                    break;
            }
        }


        #region ClientMsg Handlers
        void HandleAppOwnershipTicketResponse( ClientMsgEventArgs e )
        {
            var ticketResponse = new ClientMsgProtobuf<MsgClientGetAppOwnershipTicketResponse>();

            try
            {
                ticketResponse.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamApps", "HandleAppOwnershipTicketResponse encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            var callback = new AppOwnershipTicketCallback( Client, ticketResponse.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new AppOwnershipTicketCallback( ticketResponse.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleAppInfoResponse( ClientMsgEventArgs e )
        {
            var infoResponse = new ClientMsgProtobuf<MsgClientAppInfoResponse>();

            try
            {
                infoResponse.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamApps", "HandleAppInfoResponse encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            var callback = new AppInfoCallback( Client, infoResponse.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new AppInfoCallback( infoResponse.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandlePackageInfoResponse( ClientMsgEventArgs e )
        {
            var response = new ClientMsgProtobuf<MsgClientPackageInfoResponse>( e.Data );

#if STATIC_CALLBACKS
            var callback = new PackageInfoCallback( Client, response.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new PackageInfoCallback( response.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleAppInfoChanges( ClientMsgEventArgs e )
        {
            var changes = new ClientMsgProtobuf<MsgClientAppInfoChanges>( e.Data );

#if STATIC_CALLBACKS
            var callback = new AppChangesCallback( Client, changes.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new AppChangesCallback( changes.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleDepotKeyResponse(ClientMsgEventArgs e)
        {
            var keyResponse = new ClientMsg<MsgClientGetDepotDecryptionKeyResponse, ExtendedClientMsgHdr>(e.Data);

#if STATIC_CALLBACKS
            var callback = new DepotKeyCallback( Client, keyResponse.Msg );
            SteamClient.PostCallback( callback );
#else
            var callback = new DepotKeyCallback(keyResponse.Msg);
            this.Client.PostCallback(callback);
#endif
        }
        void HandleGameConnectTokens( ClientMsgEventArgs e )
        {
            var gcTokens = new ClientMsgProtobuf<MsgClientGameConnectTokens>( e.Data );

#if STATIC_CALLBACKS
            var callback = new GameConnectTokensCallback( Client, gcTokens.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new GameConnectTokensCallback( gcTokens.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleLicenseList( ClientMsgEventArgs e )
        {
            var licenseList = new ClientMsgProtobuf<MsgClientLicenseList>();

            try
            {
                licenseList.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamApps", "HandleLicenseList encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            var callback = new LicenseListCallback( Client, licenseList.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new LicenseListCallback( licenseList.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleVACBanStatus( ClientMsgEventArgs e )
        {
            var vacStatus = new ClientMsg<MsgClientVACBanStatus, ExtendedClientMsgHdr>( e.Data );

#if STATIC_CALLBACKS
            var callback = new VACStatusCallback( Client, vacStatus.Msg, vacStatus.Payload.ToArray() );
            SteamClient.PostCallback( callback );
#else
            var callback = new VACStatusCallback( vacStatus.Msg, vacStatus.Payload.ToArray() );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion

    }
}
