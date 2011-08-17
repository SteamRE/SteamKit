/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

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

                var list = msg.licenses.ConvertAll<License>(
                    ( input ) =>
                    {
                        return new License( input );
                    }
                );

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

    }
}
