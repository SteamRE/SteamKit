/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Net;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for requesting server list details from Steam.
    /// </summary>
    public sealed partial class SteamMasterServer : ClientMsgHandler
    {
        /// <summary>
        /// Details used when performing a server list query.
        /// </summary>
        public sealed class QueryDetails
        {
            /// <summary>
            /// Gets or sets the AppID used when querying servers.
            /// </summary>
            public uint AppID { get; set; }

            /// <summary>
            /// Gets or sets the filter used for querying the master server.
            /// Check https://developer.valvesoftware.com/wiki/Master_Server_Query_Protocol for details on how the filter is structured.
            /// </summary>
            public string? Filter { get; set; }
            /// <summary>
            /// Gets or sets the region that servers will be returned from.
            /// </summary>
            public ERegionCode Region { get; set; }

            /// <summary>
            /// Gets or sets the IP address that will be GeoIP located.
            /// This is done to return servers closer to this location.
            /// </summary>
            public IPAddress? GeoLocatedIP { get; set; }

            /// <summary>
            /// Gets or sets the maximum number of servers to return.
            /// </summary>
            public uint MaxServers { get; set; }
        }


        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamMasterServer()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.GMSClientServerQueryResponse, HandleServerQueryResponse },
            };
        }


        /// <summary>
        /// Requests a list of servers from the Steam game master server.
        /// Results are returned in a <see cref="QueryCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="details">The details for the request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="QueryCallback"/>.</returns>
        public AsyncJob<QueryCallback> ServerQuery( QueryDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( nameof(details) );
            }

            var query = new ClientMsgProtobuf<CMsgClientGMSServerQuery>( EMsg.ClientGMSServerQuery );
            query.SourceJobID = Client.GetNextJobID();

            query.Body.app_id = details.AppID;

            if ( details.GeoLocatedIP != null )
            {
                query.Body.geo_location_ip = NetHelpers.GetIPAddressAsUInt( details.GeoLocatedIP );
            }

            query.Body.filter_text = details.Filter;
            query.Body.region_code = ( uint )details.Region;

            query.Body.max_servers = details.MaxServers;

            this.Client.Send( query );

            return new AsyncJob<QueryCallback>( this.Client, query.SourceJobID );
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                throw new ArgumentNullException( nameof(packetMsg) );
            }

            bool haveFunc = dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc );

            if ( !haveFunc )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleServerQueryResponse( IPacketMsg packetMsg )
        {
            var queryResponse = new ClientMsgProtobuf<CMsgGMSClientServerQueryResponse>( packetMsg );

            var callback = new QueryCallback(queryResponse.TargetJobID, queryResponse.Body);
            Client.PostCallback( callback );
        }
        #endregion

    }
}
