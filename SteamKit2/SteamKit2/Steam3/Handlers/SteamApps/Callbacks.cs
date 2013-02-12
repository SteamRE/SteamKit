/*
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

                    this.TimeCreated = Utils.DateTimeFromUnixTime( license.time_created );
                    this.TimeNextProcess = Utils.DateTimeFromUnixTime( license.time_next_process );

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


#if STATIC_CALLBACKS
            internal LicenseListCallback( SteamClient client, CMsgClientLicenseList msg )
                : base( client )
#else
            internal LicenseListCallback( CMsgClientLicenseList msg )
#endif
            {
                this.Result = ( EResult )msg.eresult;

                var list = msg.licenses
                    .Select( l => new License( l ) )
                    .ToList();

                this.LicenseList = new ReadOnlyCollection<License>( list );
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


#if STATIC_CALLBACKS
            internal AppOwnershipTicketCallback( SteamClient client, CMsgClientGetAppOwnershipTicketResponse msg )
                : base( client )
#else
            internal AppOwnershipTicketCallback( CMsgClientGetAppOwnershipTicketResponse msg )
#endif
            {
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
                            kv.ReadAsBinary( ms );

                        if ( kv.Children != null )
                        {
                            Sections.Add( ( EAppInfoSection )section.section_id, kv.Children.FirstOrDefault() ?? KeyValue.Invalid );
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


#if STATIC_CALLBACKS
            internal AppInfoCallback( SteamClient client, CMsgClientAppInfoResponse msg )
                : base( client )
#else
            internal AppInfoCallback( CMsgClientAppInfoResponse msg )
#endif
            {
                var list = new List<App>();

                list.AddRange( msg.apps.Select( a => new App( a, App.AppInfoStatus.OK ) ) );
                list.AddRange( msg.apps_unknown.Select( a => new App( a, App.AppInfoStatus.Unknown ) ) );

                AppsPending = msg.apps_pending;

                Apps = new ReadOnlyCollection<App>( list );
            }
        }

        /// <summary>
        /// This callback is received in response to calling <see cref="SteamApps.GetPackageInfo"/>.
        /// </summary>
        public sealed class PackageInfoCallback : CallbackMsg
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
                        br.ReadUInt32(); // unknown uint at the beginning of the buffer
                        Data.ReadAsBinary( ms );
                    }

                    if ( Data.Children != null )
                    {
                        Data = Data.Children.FirstOrDefault() ?? KeyValue.Invalid;
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

#if STATIC_CALLBACKS
            internal PackageInfoCallback( SteamClient client, CMsgClientPackageInfoResponse msg )
                : base( client )
#else
            internal PackageInfoCallback( CMsgClientPackageInfoResponse msg )
#endif
            {
                var packages = new List<Package>();

                packages.AddRange( msg.packages.Select( p => new Package( p, Package.PackageStatus.OK ) ) );
                packages.AddRange( msg.packages_unknown.Select( p => new Package( p, Package.PackageStatus.Unknown ) ) );

                PackagesPending = msg.packages_pending;

                Packages = new ReadOnlyCollection<Package>( packages );
            }
        }

#pragma warning restore 0419

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


#if STATIC_CALLBACKS
            internal AppChangesCallback( SteamClient client, CMsgClientAppInfoChanges msg )
                : base( client )
#else
            internal AppChangesCallback( CMsgClientAppInfoChanges msg )
#endif
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

#if STATIC_CALLBACKS
            internal DepotKeyCallback( SteamClient client, CMsgClientGetDepotDecryptionKeyResponse msg )
                : base( client )
#else
            internal DepotKeyCallback( CMsgClientGetDepotDecryptionKeyResponse msg )
#endif
            {
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


#if STATIC_CALLBACKS
            internal GameConnectTokensCallback( SteamClient client, CMsgClientGameConnectTokens msg )
                : base( client )
#else
            internal GameConnectTokensCallback( CMsgClientGameConnectTokens msg )
#endif
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


#if STATIC_CALLBACKS
            internal VACStatusCallback( SteamClient client, MsgClientVACBanStatus msg, byte[] payload )
                : base( client )
#else
            internal VACStatusCallback( MsgClientVACBanStatus msg, byte[] payload )
#endif
            {
                var tempList = new List<uint>();

                using ( DataStream ds = new DataStream( payload ) )
                {
                    for ( int x = 0 ; x < msg.NumBans ; x++ )
                    {
                        tempList.Add( ds.ReadUInt32() );
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

#if STATIC_CALLBACKS
            internal PICSTokensCallback( SteamClient client, CMsgPICSAccessTokenResponse msg )
                : base( client )
#else
            internal PICSTokensCallback( CMsgPICSAccessTokenResponse msg )
#endif
            {
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

                internal PICSChangeData( CMsgPICSChangesSinceResponse.AppChange change )
                {
                    this.ID = change.appid;
                    this.ChangeNumber = change.change_number;
                    this.NeedsToken = change.needs_token;
                }

                internal PICSChangeData( CMsgPICSChangesSinceResponse.PackageChange change )
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

#if STATIC_CALLBACKS
            internal PICSChangesCallback( SteamClient client, CMsgPICSChangesSinceResponse msg )
                : base( client )
#else
            internal PICSChangesCallback( CMsgPICSChangesSinceResponse msg )
#endif
            {
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

                internal PICSProductInfo( CMsgPICSProductInfoResponse.AppInfo app_info )
                {
                    this.ID = app_info.appid;
                    this.ChangeNumber = app_info.change_number;
                    this.MissingToken = app_info.missing_token;
                    this.SHAHash = app_info.sha;

                    this.KeyValues = new KeyValue();
                    using (MemoryStream ms = new MemoryStream(app_info.buffer, 0, app_info.buffer.Length - 1))
                        this.KeyValues.ReadAsText(ms);

                    this.OnlyPublic = app_info.only_public;
                }

                internal PICSProductInfo( CMsgPICSProductInfoResponse.PackageInfo package_info )
                {
                    this.ID = package_info.packageid;
                    this.ChangeNumber = package_info.change_number;
                    this.MissingToken = package_info.missing_token;
                    this.SHAHash = package_info.sha;

                    this.KeyValues = new KeyValue();
                    using ( MemoryStream ms = new MemoryStream( package_info.buffer ) )
                    using ( var br = new BinaryReader( ms ) )
                    {
                        br.ReadUInt32();
                        this.KeyValues.ReadAsBinary( ms );
                    }
                }
            }

            /// <summary>
            /// Gets if this response contains only product metadata
            /// </summary>
            public bool MetaDataOnly { get; private set; }
            /// <summary>
            /// Gets if the are more product information responses pending
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

#if STATIC_CALLBACKS
            internal PICSProductInfoCallback( SteamClient client, CMsgPICSProductInfoResponse msg )
                : base( client )
#else
            internal PICSProductInfoCallback( CMsgPICSProductInfoResponse msg )
#endif
            {
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
                    Apps.Add( app_info.appid, new PICSProductInfo( app_info ) );
                }
            }
        }

    }
}