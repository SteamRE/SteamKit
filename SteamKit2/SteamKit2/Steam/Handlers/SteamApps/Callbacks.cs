/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    public sealed partial class SteamApps
    {

        /// <summary>
        /// This callback is fired during logon, informing the client of it's available licenses.
        /// </summary>
        public sealed class LicenseListCallback : CallbackMsg
        {
            /// <summary>
            /// Represents a granted license (steam3 subscription) for one or more games.
            /// </summary>
            public sealed class License
            {
                /// <summary>
                /// Gets the package ID used to identify the license.
                /// </summary>
                /// <value>The package ID.</value>
                public uint PackageID { get; private set; }

                /// <summary>
                /// Gets the last change number for this license.
                /// </summary>
                public int LastChangeNumber { get; private set; }

                /// <summary>
                /// Gets the time the license was created.
                /// </summary>
                /// <value>The time created.</value>
                public DateTime TimeCreated { get; private set; }
                /// <summary>
                /// Gets the next process time for the license.
                /// </summary>
                /// <value>The next process time.</value>
                public DateTime TimeNextProcess { get; private set; }

                /// <summary>
                /// Gets the minute limit of the license.
                /// </summary>
                /// <value>The minute limit.</value>
                public int MinuteLimit { get; private set; }
                /// <summary>
                /// Gets the minutes used of the license.
                /// </summary>
                /// <value>The minutes used.</value>
                public int MinutesUsed { get; private set; }

                /// <summary>
                /// Gets the payment method used when the license was created.
                /// </summary>
                /// <value>The payment method.</value>
                public EPaymentMethod PaymentMethod { get; private set; }
                /// <summary>
                /// Gets the license flags.
                /// </summary>
                /// <value>The license flags.</value>
                public ELicenseFlags LicenseFlags { get; private set; }

                /// <summary>
                /// Gets the two letter country code where the license was purchased.
                /// </summary>
                /// <value>The purchase country code.</value>
                public string PurchaseCountryCode { get; private set; }

                /// <summary>
                /// Gets the type of the license.
                /// </summary>
                /// <value>The type of the license.</value>
                public ELicenseType LicenseType { get; private set; }

                /// <summary>
                /// Gets the territory code of the license.
                /// </summary>
                /// <value>The territory code.</value>
                public int TerritoryCode { get; private set; }

                /// <summary>
                /// Gets the owner account id of the license.
                /// </summary>
                /// <value>The owned account id.</value>
                public uint OwnerAccountID { get; private set; }

                /// <summary>
                /// Gets the PICS access token for this package.
                /// </summary>
                /// <value>The access token.</value>
                public ulong AccessToken { get; private set; }

                /// <summary>
                /// Gets the master package id.
                /// </summary>
                /// <value>The master package id.</value>
                public uint MasterPackageID { get; private set; }

                internal License( CMsgClientLicenseList.License license )
                {
                    this.PackageID = license.package_id;

                    this.LastChangeNumber = license.change_number;

                    this.TimeCreated = DateUtils.DateTimeFromUnixTime( license.time_created );
                    this.TimeNextProcess = DateUtils.DateTimeFromUnixTime( license.time_next_process );

                    this.MinuteLimit = license.minute_limit;
                    this.MinutesUsed = license.minutes_used;

                    this.PaymentMethod = ( EPaymentMethod )license.payment_method;
                    this.LicenseFlags = ( ELicenseFlags )license.flags;

                    this.PurchaseCountryCode = license.purchase_country_code;

                    this.LicenseType = ( ELicenseType )license.license_type;

                    this.TerritoryCode = license.territory_code;

                    this.AccessToken = license.access_token;
                    this.OwnerAccountID = license.owner_id;
                    this.MasterPackageID = license.master_package_id;
                }
            }

            /// <summary>
            /// Gets the result of the message.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the license list.
            /// </summary>
            /// <value>The license list.</value>
            public ReadOnlyCollection<License> LicenseList { get; private set; }


            internal LicenseListCallback( IPacketMsg packetMsg )
            {
                var licenseList = new ClientMsgProtobuf<CMsgClientLicenseList>( packetMsg );
                var msg = licenseList.Body;

                this.Result = ( EResult )msg.eresult;

                var list = new License[ msg.licenses.Count ];

                for ( var i = 0; i < list.Length; i++ )
                {
                    list[ i ] = new License( msg.licenses[ i ] );
                }

                this.LicenseList = new ReadOnlyCollection<License>( list );
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="o:SteamApps.RequestFreeLicence"/>, informing the client of newly granted packages, if any.
        /// </summary>
        public sealed class FreeLicenseCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the message.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the list of granted apps.
            /// </summary>
            /// <value>List of granted apps.</value>
            public ReadOnlyCollection<uint> GrantedApps { get; private set; }

            /// <summary>
            /// Gets the list of granted packages.
            /// </summary>
            /// <value>List of granted packages.</value>
            public ReadOnlyCollection<uint> GrantedPackages { get; private set; }

            internal FreeLicenseCallback( IPacketMsg packetMsg )
            {
                var grantedLicenses = new ClientMsgProtobuf<CMsgClientRequestFreeLicenseResponse>( packetMsg );
                var msg = grantedLicenses.Body;

                this.JobID = grantedLicenses.TargetJobID;

                this.Result = ( EResult )msg.eresult;

                this.GrantedApps = new ReadOnlyCollection<uint>( msg.granted_appids );
                this.GrantedPackages = new ReadOnlyCollection<uint>( msg.granted_packageids );
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="SteamApps.GetAppOwnershipTicket"/>.
        /// </summary>
        public sealed class AppOwnershipTicketCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of requesting the ticket.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the AppID this ticket is for.
            /// </summary>
            public uint AppID { get; private set; }
            /// <summary>
            /// Gets the ticket data.
            /// </summary>
            public byte[] Ticket { get; private set; }

            internal AppOwnershipTicketCallback( IPacketMsg packetMsg )
            {
                var ticketResponse = new ClientMsgProtobuf<CMsgClientGetAppOwnershipTicketResponse>( packetMsg );
                var msg = ticketResponse.Body;

                this.JobID = ticketResponse.TargetJobID;

                this.Result = ( EResult )msg.eresult;
                this.AppID = msg.app_id;
                this.Ticket = msg.ticket;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="SteamApps.GetDepotDecryptionKey"/>.
        /// </summary>
        public sealed class DepotKeyCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of requesting this encryption key.
            /// </summary>
            public EResult Result { get; private set; }
            /// <summary>
            /// Gets the DepotID this encryption key is for.
            /// </summary>
            public uint DepotID { get; private set; }

            /// <summary>
            /// Gets the encryption key for this depot.
            /// </summary>
            public byte[] DepotKey { get; private set; }


            internal DepotKeyCallback( IPacketMsg packetMsg )
            {
                var keyResponse = new ClientMsgProtobuf<CMsgClientGetDepotDecryptionKeyResponse>( packetMsg );
                var msg = keyResponse.Body;

                JobID = keyResponse.TargetJobID;

                Result = ( EResult )msg.eresult;
                DepotID = msg.depot_id;
                DepotKey = msg.depot_encryption_key;
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="SteamApps.GetLegacyGameKey"/>.
        /// </summary>
        public sealed class LegacyGameKeyCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of requesting this game key.
            /// </summary>
            public EResult Result { get; private set; }
            /// <summary>
            /// Gets the appid that this game key is for.
            /// </summary>
            public uint AppID { get; private set; }
            /// <summary>
            /// Gets the game key.
            /// </summary>
            public string? Key { get; private set; }

            internal LegacyGameKeyCallback( IPacketMsg packetMsg )
            {
                var keyResponse = new ClientMsg<MsgClientGetLegacyGameKeyResponse>( packetMsg );
                var msg = keyResponse.Body;

                JobID = keyResponse.TargetJobID;
                AppID = msg.AppId;
                Result = msg.Result;

                if ( msg.Length > 0 )
                {
                    var length = ( int )msg.Length - 1;
                    var payload = keyResponse.Payload.ToArray();
                    Key = System.Text.Encoding.ASCII.GetString( payload, 0, length );
                }
            }
        }

        /// <summary>
        /// This callback is fired when the client receives a list of game connect tokens.
        /// </summary>
        public sealed class GameConnectTokensCallback : CallbackMsg
        {
            /// <summary>
            /// Gets a count of tokens to keep.
            /// </summary>
            public uint TokensToKeep { get; private set; }
            /// <summary>
            /// Gets the list of tokens.
            /// </summary>
            public ReadOnlyCollection<byte[]> Tokens { get; private set; }


            internal GameConnectTokensCallback( IPacketMsg packetMsg )
            {
                var gcTokens = new ClientMsgProtobuf<CMsgClientGameConnectTokens>( packetMsg );
                var msg = gcTokens.Body;

                TokensToKeep = msg.max_tokens_to_keep;
                Tokens = new ReadOnlyCollection<byte[]>( msg.tokens );
            }
        }

        /// <summary>
        /// This callback is fired when the client receives it's VAC banned status.
        /// </summary>
        public sealed class VACStatusCallback : CallbackMsg
        {
            /// <summary>
            /// Gets a list of VAC banned apps the client is banned from.
            /// </summary>
            public ReadOnlyCollection<uint> BannedApps { get; private set; }


            internal VACStatusCallback( IPacketMsg packetMsg )
            {
                var vacStatus = new ClientMsg<MsgClientVACBanStatus>( packetMsg );
                var msg = vacStatus.Body;

                var tempList = new List<uint>();

                using var br = new BinaryReader( vacStatus.Payload, Encoding.UTF8, leaveOpen: true );
                for ( int x = 0; x < msg.NumBans; x++ )
                {
                    tempList.Add( br.ReadUInt32() );
                }

                BannedApps = new ReadOnlyCollection<uint>( tempList );
            }
        }

        /// <summary>
        /// This callback is fired when the PICS returns access tokens for a list of appids and packageids
        /// </summary>
        public sealed class PICSTokensCallback : CallbackMsg
        {
            /// <summary>
            /// Gets a list of denied package tokens
            /// </summary>
            public ReadOnlyCollection<uint> PackageTokensDenied { get; private set; }
            /// <summary>
            /// Gets a list of denied app tokens
            /// </summary>
            public ReadOnlyCollection<uint> AppTokensDenied { get; private set; }
            /// <summary>
            /// Dictionary containing requested package tokens
            /// </summary>
            public Dictionary<uint, ulong> PackageTokens { get; private set; }
            /// <summary>
            /// Dictionary containing requested package tokens
            /// </summary>
            public Dictionary<uint, ulong> AppTokens { get; private set; }


            internal PICSTokensCallback( IPacketMsg packetMsg )
            {
                var tokensResponse = new ClientMsgProtobuf<CMsgClientPICSAccessTokenResponse>( packetMsg );
                var msg = tokensResponse.Body;

                JobID = tokensResponse.TargetJobID;

                PackageTokensDenied = new ReadOnlyCollection<uint>( msg.package_denied_tokens );
                AppTokensDenied = new ReadOnlyCollection<uint>( msg.app_denied_tokens );
                PackageTokens = [];
                AppTokens = [];

                foreach ( var package_token in msg.package_access_tokens )
                {
                    PackageTokens[ package_token.packageid ] = package_token.access_token;
                }

                foreach ( var app_token in msg.app_access_tokens )
                {
                    AppTokens[ app_token.appid ] = app_token.access_token;
                }
            }
        }

        /// <summary>
        /// This callback is fired when the PICS returns the changes since the last change number
        /// </summary>
        public sealed class PICSChangesCallback : CallbackMsg
        {
            /// <summary>
            /// Holds the change data for a single app or package
            /// </summary>
            public sealed class PICSChangeData
            {
                /// <summary>
                /// App or package ID this change data represents
                /// </summary>
                public uint ID { get; private set; }
                /// <summary>
                /// Current change number of this app
                /// </summary>
                public uint ChangeNumber { get; private set; }
                /// <summary>
                /// Signals if an access token is needed for this request
                /// </summary>
                public bool NeedsToken { get; private set; }

                internal PICSChangeData( CMsgClientPICSChangesSinceResponse.AppChange change )
                {
                    this.ID = change.appid;
                    this.ChangeNumber = change.change_number;
                    this.NeedsToken = change.needs_token;
                }

                internal PICSChangeData( CMsgClientPICSChangesSinceResponse.PackageChange change )
                {
                    this.ID = change.packageid;
                    this.ChangeNumber = change.change_number;
                    this.NeedsToken = change.needs_token;
                }
            }

            /// <summary>
            /// Supplied change number for the request
            /// </summary>
            public uint LastChangeNumber { get; private set; }
            /// <summary>
            /// Gets the current change number
            /// </summary>
            public uint CurrentChangeNumber { get; private set; }
            /// <summary>
            /// If this update requires a full update of the information
            /// </summary>
            public bool RequiresFullUpdate { get; private set; }
            /// <summary>
            /// If this update requires a full update of the app information
            /// </summary>
            public bool RequiresFullAppUpdate { get; private set; }
            /// <summary>
            /// If this update requires a full update of the package information
            /// </summary>
            public bool RequiresFullPackageUpdate { get; private set; }
            /// <summary>
            /// Dictionary containing requested package tokens
            /// </summary>
            public Dictionary<uint, PICSChangeData> PackageChanges { get; private set; }
            /// <summary>
            /// Dictionary containing requested package tokens
            /// </summary>
            public Dictionary<uint, PICSChangeData> AppChanges { get; private set; }


            internal PICSChangesCallback( IPacketMsg packetMsg )
            {
                var changesResponse = new ClientMsgProtobuf<CMsgClientPICSChangesSinceResponse>( packetMsg );
                var msg = changesResponse.Body;

                JobID = changesResponse.TargetJobID;

                LastChangeNumber = msg.since_change_number;
                CurrentChangeNumber = msg.current_change_number;
                RequiresFullUpdate = msg.force_full_update;
                RequiresFullAppUpdate = msg.force_full_app_update;
                RequiresFullPackageUpdate = msg.force_full_package_update;
                PackageChanges = [];
                AppChanges = [];

                foreach ( var package_change in msg.package_changes )
                {
                    PackageChanges[ package_change.packageid ] = new PICSChangeData( package_change );
                }

                foreach ( var app_change in msg.app_changes )
                {
                    AppChanges[ app_change.appid ] = new PICSChangeData( app_change );
                }
            }
        }

        /// <summary>
        /// This callback is fired when the PICS returns the product information requested
        /// </summary>
        public sealed class PICSProductInfoCallback : CallbackMsg
        {
            /// <summary>
            /// Represents the information for a single app or package
            /// </summary>
            public sealed class PICSProductInfo
            {
                /// <summary>
                /// Gets the ID of the app or package
                /// </summary>
                public uint ID { get; private set; }
                /// <summary>
                /// Gets the current change number for the app or package
                /// </summary>
                public uint ChangeNumber { get; private set; }
                /// <summary>
                /// Gets if an access token was required for the request
                /// </summary>
                public bool MissingToken { get; private set; }
                /// <summary>
                /// Gets the hash of the content
                /// </summary>
                public byte[]? SHAHash { get; private set; }
                /// <summary>
                /// Gets the KeyValue info
                /// </summary>
                public KeyValue KeyValues { get; private set; }
                /// <summary>
                /// For an app request, returns if only the public information was requested
                /// </summary>
                public bool OnlyPublic { get; private set; }
                /// <summary>
                /// Whether or not to use HTTP to load the KeyValues data.
                /// </summary>
                public bool UseHttp { get; private set; }
                /// <summary>
                /// For an app metadata-only request, returns the Uri for HTTP appinfo requests.
                /// </summary>
                public Uri? HttpUri
                {
                    get
                    {
                        // We should have all these fields set for the response to a metadata-only request, but guard here just in case.
                        if ( !this.HasValidHttpUri )
                        {
                            return null;
                        }

                        var shaString = Convert.ToHexString( this.SHAHash! ).ToLowerInvariant();
                        var uriString = string.Format( "http://{0}/appinfo/{1}/sha/{2}.txt.gz", this.HttpHost, this.ID, shaString );
                        return new Uri( uriString );
                    }
                }
                private string? HttpHost;
                private bool HasValidHttpUri => this.SHAHash != null && this.SHAHash.Length > 0 && !string.IsNullOrEmpty( HttpHost );

                internal PICSProductInfo( CMsgClientPICSProductInfoResponse parentResponse, CMsgClientPICSProductInfoResponse.AppInfo app_info )
                {
                    this.ID = app_info.appid;
                    this.ChangeNumber = app_info.change_number;
                    this.MissingToken = app_info.missing_token;
                    this.SHAHash = app_info.sha;

                    this.KeyValues = new KeyValue();

                    if ( app_info.buffer != null && app_info.buffer.Length > 0 )
                    {
                        // we don't want to read the trailing null byte
                        using var ms = new MemoryStream( app_info.buffer, 0, app_info.buffer.Length - 1 );
                        this.KeyValues.ReadAsText( ms );
                    }

                    this.OnlyPublic = app_info.only_public;
                    this.HttpHost = parentResponse.http_host;
                    this.UseHttp = this.HasValidHttpUri && app_info.size >= parentResponse.http_min_size;
                }

                internal PICSProductInfo( CMsgClientPICSProductInfoResponse.PackageInfo package_info )
                {
                    this.ID = package_info.packageid;
                    this.ChangeNumber = package_info.change_number;
                    this.MissingToken = package_info.missing_token;
                    this.SHAHash = package_info.sha;

                    this.KeyValues = new KeyValue();

                    if ( package_info.buffer != null )
                    {
                        using MemoryStream ms = new MemoryStream( package_info.buffer );
                        using var br = new BinaryReader( ms );
                        // steamclient checks this value == 1 before it attempts to read the KV from the buffer
                        // see: CPackageInfo::UpdateFromBuffer(CSHA const&,uint,CUtlBuffer &)
                        // todo: we've apparently ignored this with zero ill effects, but perhaps we want to respect it?
                        br.ReadUInt32();

                        this.KeyValues.TryReadAsBinary( ms );
                    }
                }
            }

            /// <summary>
            /// Gets if this response contains only product metadata
            /// </summary>
            public bool MetaDataOnly { get; private set; }
            /// <summary>
            /// Gets if there are more product information responses pending
            /// </summary>
            public bool ResponsePending { get; private set; }
            /// <summary>
            /// Gets a list of unknown package ids
            /// </summary>
            public ReadOnlyCollection<uint> UnknownPackages { get; private set; }
            /// <summary>
            /// Gets a list of unknown app ids
            /// </summary>
            public ReadOnlyCollection<uint> UnknownApps { get; private set; }
            /// <summary>
            /// Dictionary containing requested app info
            /// </summary>
            public Dictionary<uint, PICSProductInfo> Apps { get; private set; }
            /// <summary>
            /// Dictionary containing requested package info
            /// </summary>
            public Dictionary<uint, PICSProductInfo> Packages { get; private set; }


            internal PICSProductInfoCallback( IPacketMsg packetMsg )
            {
                var productResponse = new ClientMsgProtobuf<CMsgClientPICSProductInfoResponse>( packetMsg );
                var msg = productResponse.Body;

                JobID = productResponse.TargetJobID;

                MetaDataOnly = msg.meta_data_only;
                ResponsePending = msg.response_pending;
                UnknownPackages = new ReadOnlyCollection<uint>( msg.unknown_packageids );
                UnknownApps = new ReadOnlyCollection<uint>( msg.unknown_appids );
                Packages = new( msg.packages.Count );
                Apps = new( msg.apps.Count );

                foreach ( var package_info in msg.packages )
                {
                    Packages[ package_info.packageid ] = new PICSProductInfo( package_info );
                }

                foreach ( var app_info in msg.apps )
                {
                    Apps[ app_info.appid ] = new PICSProductInfo( msg, app_info );
                }
            }
        }

        /// <summary>
        /// This callback is received when the list of guest passes is updated.
        /// </summary>
        public sealed class GuestPassListCallback : CallbackMsg
        {
            /// <summary>
            /// Result of the operation
            /// </summary>
            public EResult Result { get; set; }
            /// <summary>
            /// Number of guest passes to be given out
            /// </summary>
            public int CountGuestPassesToGive { get; set; }
            /// <summary>
            /// Number of guest passes to be redeemed
            /// </summary>
            public int CountGuestPassesToRedeem { get; set; }
            /// <summary>
            /// Guest pass list
            /// </summary>
            public List<KeyValue> GuestPasses { get; set; }


            internal GuestPassListCallback( IPacketMsg packetMsg )
            {
                var guestPasses = new ClientMsg<MsgClientUpdateGuestPassesList>( packetMsg );
                var msg = guestPasses.Body;

                Result = msg.Result;
                CountGuestPassesToGive = msg.CountGuestPassesToGive;
                CountGuestPassesToRedeem = msg.CountGuestPassesToRedeem;

                GuestPasses = [];
                for ( int i = 0; i < CountGuestPassesToGive + CountGuestPassesToRedeem; i++ )
                {
                    var kv = new KeyValue();
                    kv.TryReadAsBinary( guestPasses.Payload );
                    GuestPasses.Add( kv );
                }
            }
        }

        /// <summary>
        /// This callback is received in response to activating a guest pass or a gift.
        /// </summary>
        public sealed class RedeemGuestPassResponseCallback : CallbackMsg
        {
            /// <summary>
            /// Result of the operation
            /// </summary>
            public EResult Result { get; set; }
            /// <summary>
            /// Package ID which was activated.
            /// </summary>
            public uint PackageID { get; set; }
            /// <summary>
            /// App ID which must be owned to activate this guest pass.
            /// </summary>
            public uint MustOwnAppID { get; set; }


            internal RedeemGuestPassResponseCallback( IPacketMsg packetMsg )
            {
                var redeemedGuestPass = new ClientMsgProtobuf<CMsgClientRedeemGuestPassResponse>( packetMsg );
                var msg = redeemedGuestPass.Body;

                JobID = redeemedGuestPass.TargetJobID;
                Result = ( EResult )msg.eresult;
                PackageID = msg.package_id;
                MustOwnAppID = msg.must_own_appid;
            }
        }

        /// <summary>
        /// This callback is received in a response to activating a Steam key.
        /// </summary>
        public sealed class PurchaseResponseCallback : CallbackMsg
        {
            /// <summary>
            /// Result of the operation
            /// </summary>
            public EResult Result { get; set; }
            /// <summary>
            /// Purchase result of the operation
            /// </summary>
            public EPurchaseResultDetail PurchaseResultDetail { get; set; }
            /// <summary>
            /// Purchase receipt of the operation
            /// </summary>
            public KeyValue PurchaseReceiptInfo { get; set; }


            internal PurchaseResponseCallback( IPacketMsg packetMsg )
            {
                var purchaseResponse = new ClientMsgProtobuf<CMsgClientPurchaseResponse>( packetMsg );
                var msg = purchaseResponse.Body;

                JobID = purchaseResponse.TargetJobID;
                Result = ( EResult )msg.eresult;
                PurchaseResultDetail = ( EPurchaseResultDetail )msg.purchase_result_details;
                PurchaseReceiptInfo = new KeyValue();

                if ( msg.purchase_receipt_info == null )
                {
                    return;
                }

                using var ms = new MemoryStream( msg.purchase_receipt_info );
                PurchaseReceiptInfo.TryReadAsBinary( ms );
            }
        }

        /// <summary>
        /// This callback is received when a CDN auth token is received
        /// </summary>
        public sealed class CDNAuthTokenCallback : CallbackMsg
        {
            /// <summary>
            /// Result of the operation
            /// </summary>
            public EResult Result { get; set; }
            /// <summary>
            /// CDN auth token
            /// </summary>
            public string Token { get; set; }
            /// <summary>
            /// Token expiration date
            /// </summary>
            public DateTime Expiration { get; set; }

            internal CDNAuthTokenCallback( IPacketMsg packetMsg )
            {
                var response = new ClientMsgProtobuf<CMsgClientGetCDNAuthTokenResponse>( packetMsg );
                var msg = response.Body;

                JobID = response.TargetJobID;

                Result = ( EResult )msg.eresult;
                Token = msg.token;
                Expiration = DateUtils.DateTimeFromUnixTime( msg.expiration_time );
            }
        }

        /// <summary>
        /// This callback is received when a beta password check has been completed
        /// </summary>
        public sealed class CheckAppBetaPasswordCallback : CallbackMsg
        {
            /// <summary>
            /// Result of the operation
            /// </summary>
            public EResult Result { get; set; }
            /// <summary>
            /// Map of beta names to their encryption keys
            /// </summary>
            public Dictionary<string, byte[]> BetaPasswords { get; private set; }

            internal CheckAppBetaPasswordCallback( IPacketMsg packetMsg )
            {
                var response = new ClientMsgProtobuf<CMsgClientCheckAppBetaPasswordResponse>( packetMsg );
                var msg = response.Body;

                JobID = response.TargetJobID;

                Result = ( EResult )msg.eresult;
                BetaPasswords = [];

                foreach ( var password in msg.betapasswords )
                {
                    BetaPasswords[ password.betaname ] = Utils.DecodeHexString( password.betapassword );
                }
            }
        }
    }
}
