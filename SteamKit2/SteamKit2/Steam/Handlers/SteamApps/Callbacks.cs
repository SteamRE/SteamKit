﻿/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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


            internal LicenseListCallback( CMsgClientLicenseList msg )
            {
                this.Result = ( EResult )msg.eresult;

                var list = msg.licenses
                    .Select( l => new License( l ) )
                    .ToList();

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

            internal FreeLicenseCallback( JobID jobID, CMsgClientRequestFreeLicenseResponse msg )
            {
                this.JobID = jobID;

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


            internal AppOwnershipTicketCallback( JobID jobID, CMsgClientGetAppOwnershipTicketResponse msg )
            {
                this.JobID = jobID;

                this.Result = ( EResult )msg.eresult;
                this.AppID = msg.app_id;
                this.Ticket = msg.ticket;
            }
        }

// Ambiguous reference in cref attribute: 'SteamApps.GetPackageInfo'. Assuming 'SteamKit2.SteamApps.GetPackageInfo(uint, bool)',
// but could have also matched other overloads including 'SteamKit2.SteamApps.GetPackageInfo(System.Collections.Generic.IEnumerable<uint>, bool)'.
#pragma warning disable 0419

        /// <summary>
        /// This callback is received in response to calling <see cref="SteamApps.GetAppInfo"/>.
        /// </summary>
        public sealed class AppInfoCallback : CallbackMsg
#pragma warning restore 0419
        {
            /// <summary>
            /// Represents a single app in the info response.
            /// </summary>
            public sealed class App
            {
                /// <summary>
                /// The status of a requested app.
                /// </summary>
                public enum AppInfoStatus
                {
                    /// <summary>
                    /// The information for this app was requested successfully.
                    /// </summary>
                    OK,
                    /// <summary>
                    /// This app is unknown.
                    /// </summary>
                    Unknown
                }


                /// <summary>
                /// Gets the status of the app.
                /// </summary>
                public AppInfoStatus Status { get; private set; }
                /// <summary>
                /// Gets the AppID for this app.
                /// </summary>
                public uint AppID { get; private set; }
                /// <summary>
                /// Gets the last change number for this app.
                /// </summary>
                public uint ChangeNumber { get; private set; }
                /// <summary>
                /// Gets a section data for this app.
                /// </summary>
                public Dictionary<EAppInfoSection, KeyValue> Sections { get; private set; }


                internal App( CMsgClientAppInfoResponse.App app, AppInfoStatus status )
                {
                    Status = status;
                    AppID = app.app_id;
                    ChangeNumber = app.change_number;
                    Sections = new Dictionary<EAppInfoSection, KeyValue>();

                    foreach ( var section in app.sections )
                    {
                        KeyValue kv = new KeyValue();

                        using ( MemoryStream ms = new MemoryStream( section.section_kv ) )
                        {
                            if ( kv.TryReadAsBinary( ms ) )
                            {
                                Sections.Add( ( EAppInfoSection )section.section_id, kv );
                            }
                        }
                    }
                }

                internal App( uint appid, AppInfoStatus status )
                {
                    Status = status;
                    AppID = appid;
                }
            }

            /// <summary>
            /// Gets the list of apps this response contains.
            /// </summary>
            public ReadOnlyCollection<App> Apps { get; private set; }
            /// <summary>
            /// Gets the number of apps pending in this response.
            /// </summary>
            public uint AppsPending { get; private set; }


            internal AppInfoCallback( JobID jobID, CMsgClientAppInfoResponse msg )
            {
                JobID = jobID;

                var list = new List<App>();

                list.AddRange( msg.apps.Select( a => new App( a, App.AppInfoStatus.OK ) ) );
                list.AddRange( msg.apps_unknown.Select( a => new App( a, App.AppInfoStatus.Unknown ) ) );

                AppsPending = msg.apps_pending;

                Apps = new ReadOnlyCollection<App>( list );
            }
        }

        // Ambiguous reference in cref attribute: 'SteamApps.GetPackageInfo'. Assuming 'SteamKit2.SteamApps.GetPackageInfo(uint, bool)',
        // but could have also matched other overloads including 'SteamKit2.SteamApps.GetPackageInfo(System.Collections.Generic.IEnumerable<uint>, bool)'.
#pragma warning disable 0419

        /// <summary>
        /// This callback is received in response to calling <see cref="SteamApps.GetPackageInfo"/>.
        /// </summary>
        public sealed class PackageInfoCallback : CallbackMsg
#pragma warning restore 0419
        {
            /// <summary>
            /// Represents a single package in this response.
            /// </summary>
            public sealed class Package
            {
                /// <summary>
                /// The status of a package.
                /// </summary>
                public enum PackageStatus
                {
                    /// <summary>
                    /// The information for this package was requested successfully.
                    /// </summary>
                    OK,
                    /// <summary>
                    /// This package is unknown.
                    /// </summary>
                    Unknown,
                }

                /// <summary>
                /// Gets the status of this package.
                /// </summary>
                public PackageStatus Status { get; private set; }
                /// <summary>
                /// Gets the PackageID for this package.
                /// </summary>
                public uint PackageID { get; private set; }
                /// <summary>
                /// Gets the last change number for this package.
                /// </summary>
                public uint ChangeNumber { get; private set; }
                /// <summary>
                /// Gets a hash of the package data for caching purposes.
                /// </summary>
                public byte[] Hash { get; private set; }
                /// <summary>
                /// Gets the data for this package.
                /// </summary>
                public KeyValue Data { get; private set; }


                internal Package( CMsgClientPackageInfoResponse.Package pack, Package.PackageStatus status )
                {
                    Status = status;

                    PackageID = pack.package_id;
                    ChangeNumber = pack.change_number;
                    Hash = pack.sha;

                    Data = new KeyValue();

                    using ( var ms = new MemoryStream( pack.buffer ) )
                    using ( var br = new BinaryReader( ms ) )
                    {
                        // steamclient checks this value == 1 before it attempts to read the KV from the buffer
                        // see: CPackageInfo::UpdateFromBuffer(CSHA const&,uint,CUtlBuffer &)
                        // todo: we've apparently ignored this with zero ill effects, but perhaps we want to respect it?
                        br.ReadUInt32();
                        
                        Data.TryReadAsBinary( ms );
                    }
                }

                internal Package( uint packageId, Package.PackageStatus status )
                {
                    Status = status;
                    PackageID = packageId;
                }
            }


            /// <summary>
            /// Gets the list of packages this response contains.
            /// </summary>
            public ReadOnlyCollection<Package> Packages { get; private set; }
            /// <summary>
            /// Gets a count of packages pending in this response.
            /// </summary>
            public uint PackagesPending { get; private set; }


            internal PackageInfoCallback( JobID jobID, CMsgClientPackageInfoResponse msg )
            {
                JobID = jobID;

                var packages = new List<Package>();

                packages.AddRange( msg.packages.Select( p => new Package( p, Package.PackageStatus.OK ) ) );
                packages.AddRange( msg.packages_unknown.Select( p => new Package( p, Package.PackageStatus.Unknown ) ) );

                PackagesPending = msg.packages_pending;

                Packages = new ReadOnlyCollection<Package>( packages );
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="SteamApps.GetAppChanges"/>.
        /// </summary>
        public sealed class AppChangesCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the list of AppIDs that have changed since the last change number request.
            /// </summary>
            public ReadOnlyCollection<uint> AppIDs { get; private set; }
            /// <summary>
            /// Gets the current change number.
            /// </summary>
            public uint CurrentChangeNumber { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the backend wishes for the client to perform a full update.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the client should perform a full update; otherwise, <c>false</c>.
            /// </value>
            public bool ForceFullUpdate { get; private set; }


            internal AppChangesCallback( CMsgClientAppInfoChanges msg )
            {
                AppIDs = new ReadOnlyCollection<uint>( msg.appIDs );
                CurrentChangeNumber = msg.current_change_number;

                ForceFullUpdate = msg.force_full_update;
            }
        }

        /// <summary>
        /// This callback is recieved in response to calling <see cref="SteamApps.GetDepotDecryptionKey"/>.
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


            internal DepotKeyCallback( JobID jobID, CMsgClientGetDepotDecryptionKeyResponse msg )
            {
                JobID = jobID;

                Result = ( EResult )msg.eresult;
                DepotID = msg.depot_id;
                DepotKey = msg.depot_encryption_key;
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


            internal GameConnectTokensCallback( CMsgClientGameConnectTokens msg )
            {
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


            internal VACStatusCallback( MsgClientVACBanStatus msg, byte[] payload )
            {
                var tempList = new List<uint>();

                using ( var ms = new MemoryStream( payload ) )
                using ( var br = new BinaryReader( ms ) )
                {
                    for ( int x = 0 ; x < msg.NumBans ; x++ )
                    {
                        tempList.Add( br.ReadUInt32() );
                    }

                    BannedApps = new ReadOnlyCollection<uint>( tempList );
                }
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


            internal PICSTokensCallback( JobID jobID, CMsgClientPICSAccessTokenResponse msg )
            {
                JobID = jobID;

                PackageTokensDenied = new ReadOnlyCollection<uint>( msg.package_denied_tokens );
                AppTokensDenied = new ReadOnlyCollection<uint>( msg.app_denied_tokens );
                PackageTokens = new Dictionary<uint, ulong>();
                AppTokens = new Dictionary<uint, ulong>();

                foreach ( var package_token in msg.package_access_tokens )
                {
                    PackageTokens.Add( package_token.packageid, package_token.access_token );
                }

                foreach ( var app_token in msg.app_access_tokens )
                {
                    AppTokens.Add( app_token.appid, app_token.access_token );
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
            /// Dictionary containing requested package tokens
            /// </summary>
            public Dictionary<uint, PICSChangeData> PackageChanges { get; private set; }
            /// <summary>
            /// Dictionary containing requested package tokens
            /// </summary>
            public Dictionary<uint, PICSChangeData> AppChanges { get; private set; }


            internal PICSChangesCallback( JobID jobID, CMsgClientPICSChangesSinceResponse msg )
            {
                JobID = jobID;

                LastChangeNumber = msg.since_change_number;
                CurrentChangeNumber = msg.current_change_number;
                RequiresFullUpdate = msg.force_full_update;
                PackageChanges = new Dictionary<uint, PICSChangeData>();
                AppChanges = new Dictionary<uint, PICSChangeData>();

                foreach ( var package_change in msg.package_changes )
                {
                    PackageChanges.Add( package_change.packageid, new PICSChangeData( package_change ) );
                }

                foreach ( var app_change in msg.app_changes )
                {
                    AppChanges.Add( app_change.appid, new PICSChangeData( app_change ) );
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
                public byte[] SHAHash { get; private set; }
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
                public Uri HttpUri { get; private set; }

                internal PICSProductInfo( CMsgClientPICSProductInfoResponse parentResponse, CMsgClientPICSProductInfoResponse.AppInfo app_info)
                {
                    this.ID = app_info.appid;
                    this.ChangeNumber = app_info.change_number;
                    this.MissingToken = app_info.missing_token;
                    this.SHAHash = app_info.sha;

                    this.KeyValues = new KeyValue();

                    if ( app_info.buffer != null )
                    {
                        // we don't want to read the trailing null byte
                        using ( var ms = new MemoryStream( app_info.buffer, 0, app_info.buffer.Length - 1 ) )
                        {
                            this.KeyValues.ReadAsText( ms );
                        }
                    }

                    this.OnlyPublic = app_info.only_public;

                    // We should have all these fields set for the response to a metadata-only request, but guard here just in case.
                    if (this.SHAHash != null && this.SHAHash.Length > 0 && !string.IsNullOrEmpty(parentResponse.http_host))
                    {
                        var shaString = BitConverter.ToString(this.SHAHash)
                            .Replace("-", string.Empty)
                            .ToLower();
                        var uriString = string.Format("http://{0}/appinfo/{1}/sha/{2}.txt.gz", parentResponse.http_host, this.ID, shaString);
                        this.HttpUri = new Uri(uriString);
                    }

                    this.UseHttp = this.HttpUri != null && app_info.size >= parentResponse.http_min_size;
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
                        using ( MemoryStream ms = new MemoryStream( package_info.buffer ) )
                        using ( var br = new BinaryReader( ms ) )
                        {
                            // steamclient checks this value == 1 before it attempts to read the KV from the buffer
                            // see: CPackageInfo::UpdateFromBuffer(CSHA const&,uint,CUtlBuffer &)
                            // todo: we've apparently ignored this with zero ill effects, but perhaps we want to respect it?
                            br.ReadUInt32();
                            
                            this.KeyValues.TryReadAsBinary( ms );
                        }
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


            internal PICSProductInfoCallback( JobID jobID, CMsgClientPICSProductInfoResponse msg )
            {
                JobID = jobID;

                MetaDataOnly = msg.meta_data_only;
                ResponsePending = msg.response_pending;
                UnknownPackages = new ReadOnlyCollection<uint>( msg.unknown_packageids );
                UnknownApps = new ReadOnlyCollection<uint>( msg.unknown_appids );
                Packages = new Dictionary<uint, PICSProductInfo>();
                Apps = new Dictionary<uint, PICSProductInfo>();

                foreach ( var package_info in msg.packages )
                {
                    Packages.Add( package_info.packageid, new PICSProductInfo( package_info ) );
                }

                foreach ( var app_info in msg.apps )
                {
                    Apps.Add( app_info.appid, new PICSProductInfo( msg, app_info ) );
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


            internal GuestPassListCallback( MsgClientUpdateGuestPassesList msg, Stream payload )
            {
                Result = msg.Result;
                CountGuestPassesToGive = msg.CountGuestPassesToGive;
                CountGuestPassesToRedeem = msg.CountGuestPassesToRedeem;

                GuestPasses = new List<KeyValue>();
                for ( int i = 0; i < CountGuestPassesToGive + CountGuestPassesToRedeem; i++ )
                {
                    var kv = new KeyValue();
                    kv.TryReadAsBinary( payload );
                    GuestPasses.Add( kv );
                }
            }
        }

        /// <summary>
        /// This callback is received when a guest pass has been sent
        /// </summary>
        public sealed class SendGuestPassCallback : CallbackMsg
        {
            /// <summary>
            /// Result of the operation
            /// </summary>
            public EResult Result { get; set; }


            internal SendGuestPassCallback( MsgClientSendGuestPassResponse msg )
            {
                Result = msg.Result;
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

            internal CDNAuthTokenCallback( JobID jobID, CMsgClientGetCDNAuthTokenResponse msg )
            {
                JobID = jobID;

                Result = (EResult)msg.eresult;
                Token = msg.token;
                Expiration = DateUtils.DateTimeFromUnixTime( msg.expiration_time );
            }
        }
    }
}