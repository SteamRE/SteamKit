/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

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

                var list = msg.licenses.ConvertAll<License>( input => new License( input ) );

                this.LicenseList = new ReadOnlyCollection<License>( list );
            }
        }

        public sealed class AppOwnershipTicketCallback : CallbackMsg
        {
            public EResult Result { get; private set; }

            public uint AppID { get; private set; }
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

        public sealed class AppInfoCallback : CallbackMsg
        {
            public sealed class AppInfo
            {
                public enum AppInfoStatus
                {
                    OK,
                    Unknown
                }

                public AppInfoStatus Status { get; private set; }
                public uint AppID { get; private set; }
                public uint ChangeNumber { get; private set; }
                public Dictionary<int, KeyValue> Sections { get; private set; }

                internal AppInfo(CMsgClientAppInfoResponse.App app, AppInfoStatus status)
                {
                    Status = status;
                    AppID = app.app_id;
                    ChangeNumber = app.change_number;
                    Sections = new Dictionary<int, KeyValue>();

                    foreach(var section in app.sections)
                    {
                        KeyValue kv = new KeyValue();
                        using(MemoryStream ms = new MemoryStream(section.section_kv))
                            kv.ReadAsBinary(ms);

                        Sections.Add((int)section.section_id, kv);
                    }
                }

                internal AppInfo(uint appid, AppInfoStatus status)
                {
                    Status = status;
                    AppID = appid;
                }
            }

            public ReadOnlyCollection<AppInfo> Apps { get; private set; }
            public uint AppsPending { get; private set; }

#if STATIC_CALLBACKS
            internal AppInfoCallback( SteamClient client, CMsgClientAppInfoResponse msg )
                : base( client )
#else
            internal AppInfoCallback(CMsgClientAppInfoResponse msg)
#endif
            {
                List<AppInfo> list = new List<AppInfo>();

                BuildList(msg.apps, AppInfo.AppInfoStatus.OK, ref list);
                BuildList(msg.apps_unknown, AppInfo.AppInfoStatus.Unknown, ref list);

                AppsPending = msg.apps_pending;

                Apps = new ReadOnlyCollection<AppInfo>(list);
            }

            internal void BuildList(List<CMsgClientAppInfoResponse.App> apps, AppInfo.AppInfoStatus status, ref List<AppInfo> list)
            {
                foreach (var app in apps)
                {
                    list.Add(new AppInfo(app, status));
                }
            }

            internal void BuildList(List<uint> apps, AppInfo.AppInfoStatus status, ref List<AppInfo> list)
            {
                foreach (var app in apps)
                {
                    list.Add(new AppInfo(app, status));
                }
            }
        }

        public sealed class PackageInfoCallback : CallbackMsg
        {
            public sealed class Package
            {
                public enum PackageStatus
                {
                    OK,
                    Unknown,
                }

                public PackageStatus Status { get; private set; }
                public uint PackageID { get; private set; }
                public uint ChangeNumber { get; private set; }
                public byte[] Hash { get; private set; }
                public KeyValue Data { get; private set; }

                public Package( CMsgClientPackageInfoResponse.Package pack, Package.PackageStatus status )
                {
                    Status = status;

                    PackageID = pack.package_id;
                    ChangeNumber = pack.change_number;
                    Hash = pack.sha;

                    Data = new KeyValue();

                    using ( var ms = new MemoryStream( pack.buffer ) )
                        Data.ReadAsBinary( ms );
                }

                public Package( uint packageId, Package.PackageStatus status )
                {
                    Status = status;
                    PackageID = packageId;
                }
            }


            public ReadOnlyCollection<Package> Packages { get; private set; }
            public uint PackagesPending { get; private set; }

#if STATIC_CALLBACKS
            internal PackageInfoCallback( SteamClient client, CMsgClientPackageInfoResponse msg )
                : base( client )
#else
            internal PackageInfoCallback( CMsgClientPackageInfoResponse msg )
#endif
            {
                var packages = new List<Package>();

                BuildList( msg.packages, Package.PackageStatus.OK, packages );
                BuildList( msg.packages_unknown, Package.PackageStatus.Unknown, packages );

                PackagesPending = msg.packages_pending;

                Packages = new ReadOnlyCollection<Package>( packages );
            }


            void BuildList( List<CMsgClientPackageInfoResponse.Package> packages, Package.PackageStatus status, List<Package> list )
            {
                packages.ForEach( pack => list.Add( new Package( pack, status ) ) );
            }
            void BuildList( List<uint> packages, Package.PackageStatus status, List<Package> list )
            {
                packages.ForEach( id  => list.Add( new Package( id, status ) ) );
            }
        }

        public sealed class AppChangesCallback : CallbackMsg
        {
            public ReadOnlyCollection<uint> AppIDs { get; private set; }
            public uint CurrentChangeNumber { get; private set; }

            public bool ForceFullUpdate { get; set; }

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

        public sealed class DepotKeyCallback : CallbackMsg
        {
            public EResult Result { get; set; }
            public uint DepotID { get; set; }
            public byte[] DepotKey { get; set; }

#if STATIC_CALLBACKS
            internal DepotKeyCallback( SteamClient client, MsgClientGetDepotDecryptionKeyResponse msg )
                : base( client )
#else
            internal DepotKeyCallback( MsgClientGetDepotDecryptionKeyResponse msg )
#endif
            {
                Result = (EResult)msg.Result;
                DepotID = msg.DepotID;
                DepotKey = msg.DepotEncryptionKey;
            }
        }

        public sealed class GameConnectTokensCallback : CallbackMsg
        {
            public uint TokensToKeep { get; private set; }
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

        public sealed class VACStatusCallback : CallbackMsg
        {
            public ReadOnlyCollection<uint> BannedApps { get; set; }


#if STATIC_CALLBACKS
            internal VACStatusCallback( SteamClient client, MsgClientVACBanStatus msg, byte[] payload )
                : base( client )
#else
            internal VACStatusCallback( MsgClientVACBanStatus msg, byte[] payload )
#endif
            {
                List<uint> tempList = new List<uint>();

                DataStream ds = new DataStream( payload );

                for ( int x = 0 ; x < msg.NumBans ; x++ )
                {
                    tempList.Add( ds.ReadUInt32() );
                }

                BannedApps = new ReadOnlyCollection<uint>( tempList );
            }
        }

    }
}
