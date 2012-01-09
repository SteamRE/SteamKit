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
    public sealed partial class SteamMasterServer : ClientMsgHandler
    {
        public sealed class QueryDetails
        {
            public uint AppID { get; set; }

            public string Filter { get; set; }
            public ERegionCode Region { get; set; }

            public IPAddress GeoLocatedIP { get; set; }

            public uint MaxServers { get; set; }
        }


        internal SteamMasterServer()
        {
        }


        public ulong ServerQuery( QueryDetails details )
        {
            var query = new ClientMsgProtobuf<CMsgClientGMSServerQuery>( EMsg.ClientGMSServerQuery );
            query.SourceJobID = Client.GetNextJobID();

            query.Body.app_id = details.AppID;

            if ( details.GeoLocatedIP != null )
                query.Body.geo_location_ip = NetHelpers.GetIPAddress( details.GeoLocatedIP );

            query.Body.filter_text = details.Filter;
            query.Body.region_code = ( uint )details.Region;

            query.Body.max_servers = details.MaxServers;

            this.Client.Send( query );

            return query.SourceJobID;
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            switch ( packetMsg.MsgType )
            {
                case EMsg.GMSClientServerQueryResponse:
                    HandleServerQueryResponse( packetMsg );
                    break;
            }
        }


        #region ClientMsg Handlers
        void HandleServerQueryResponse( IPacketMsg packetMsg )
        {
            var queryResponse = new ClientMsgProtobuf<CMsgGMSClientServerQueryResponse>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new QueryCallback( Client, queryResponse.Body );
            var callback = new SteamClient.JobCallback<QueryCallback>( Client, queryResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new QueryCallback( queryResponse.Body );
            var callback = new SteamClient.JobCallback<QueryCallback>( queryResponse.TargetJobID, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        #endregion

    }
}
