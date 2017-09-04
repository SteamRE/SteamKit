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
    /// This handler handles Steam user statistic related actions.
    /// </summary>
    public sealed partial class SteamUserStats : ClientMsgHandler
    {
        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamUserStats()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientGetNumberOfCurrentPlayersDPResponse, HandleNumberOfPlayersResponse },
                { EMsg.ClientLBSFindOrCreateLBResponse, HandleFindOrCreateLBResponse },
                { EMsg.ClientLBSGetLBEntriesResponse, HandleGetLBEntriesRespons },
            };
        }

        /// <summary>
        /// Retrieves the number of current players for a given app id.
        /// Results are returned in a <see cref="NumberOfPlayersCallback"/>.
        /// </summary>
        /// <param name="appId">The app id to request the number of players for.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="NumberOfPlayersCallback"/>.</returns>
        public AsyncJob<NumberOfPlayersCallback> GetNumberOfCurrentPlayers( uint appId )
        {
            var msg = new ClientMsgProtobuf<CMsgDPGetNumberOfCurrentPlayers>( EMsg.ClientGetNumberOfCurrentPlayersDP );
            msg.SourceJobID = Client.GetNextJobID();

            msg.Body.appid = appId;

            Client.Send( msg );

            return new AsyncJob<NumberOfPlayersCallback>( this.Client, msg.SourceJobID );
        }

        /// <summary>
        /// Asks the Steam back-end for a leaderboard by name for a given appid.
        /// Results are returned in a <see cref="FindOrCreateLeaderboardCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appId">The AppID to request a leaderboard for.</param>
        /// <param name="name">Name of the leaderboard to request.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="FindOrCreateLeaderboardCallback"/>.</returns>
        public AsyncJob<FindOrCreateLeaderboardCallback> FindLeaderboard( uint appId, string name )
        {
            var msg = new ClientMsgProtobuf<CMsgClientLBSFindOrCreateLB>( EMsg.ClientLBSFindOrCreateLB );
            msg.SourceJobID = Client.GetNextJobID();

            // routing_appid has to be set correctly to receive a response
            msg.ProtoHeader.routing_appid = appId;

            msg.Body.app_id = appId;
            msg.Body.leaderboard_name = name;
            msg.Body.create_if_not_found = false;

            Client.Send( msg );

            return new AsyncJob<FindOrCreateLeaderboardCallback>( this.Client, msg.SourceJobID );
        }
        /// <summary>
        /// Asks the Steam back-end for a leaderboard by name, and will create it if it's not yet.
        /// Results are returned in a <see cref="FindOrCreateLeaderboardCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appId">The AppID to request a leaderboard for.</param>
        /// <param name="name">Name of the leaderboard to create.</param>
        /// <param name="sortMethod">Sort method to use for this leaderboard</param>
        /// <param name="displayType">Display type for this leaderboard.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="FindOrCreateLeaderboardCallback"/>.</returns>
        public AsyncJob<FindOrCreateLeaderboardCallback> CreateLeaderboard( uint appId, string name, ELeaderboardSortMethod sortMethod, ELeaderboardDisplayType displayType )
        {
            var msg = new ClientMsgProtobuf<CMsgClientLBSFindOrCreateLB>( EMsg.ClientLBSFindOrCreateLB );
            msg.SourceJobID = Client.GetNextJobID();

            // routing_appid has to be set correctly to receive a response
            msg.ProtoHeader.routing_appid = appId;

            msg.Body.app_id = appId;
            msg.Body.leaderboard_name = name;
            msg.Body.leaderboard_display_type = ( int )displayType;
            msg.Body.leaderboard_sort_method = ( int )sortMethod;
            msg.Body.create_if_not_found = true;

            Client.Send( msg );

            return new AsyncJob<FindOrCreateLeaderboardCallback>( this.Client, msg.SourceJobID );
        }

        /// <summary>
        /// Asks the Steam back-end for a set of rows in the leaderboard.
        /// Results are returned in a <see cref="LeaderboardEntriesCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appId">The AppID to request leaderboard rows for.</param>
        /// <param name="id">ID of the leaderboard to view.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="LeaderboardEntriesCallback"/>.</returns>
        /// <param name="rangeStart">Range start or 0.</param>
        /// <param name="rangeEnd">Range end or max leaderboard entries.</param>
        /// <param name="dataRequest">Type of request.</param>
        public AsyncJob<LeaderboardEntriesCallback> GetLeaderboardEntries( uint appId, int id, int rangeStart, int rangeEnd, ELeaderboardDataRequest dataRequest )
        {
            var msg = new ClientMsgProtobuf<CMsgClientLBSGetLBEntries>( EMsg.ClientLBSGetLBEntries );
            msg.SourceJobID = Client.GetNextJobID();

            // routing_appid has to be set correctly to receive a response
            msg.ProtoHeader.routing_appid = appId;

            msg.Body.app_id = ( int )appId;
            msg.Body.leaderboard_id = id;
            msg.Body.leaderboard_data_request = ( int )dataRequest;
            msg.Body.range_start = rangeStart;
            msg.Body.range_end = rangeEnd;

            Client.Send( msg );

            return new AsyncJob<LeaderboardEntriesCallback>( this.Client, msg.SourceJobID );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The <see cref="SteamKit2.IPacketMsg"/> instance containing the event data.</param>
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
        void HandleNumberOfPlayersResponse( IPacketMsg packetMsg )
        {
            var msg = new ClientMsgProtobuf<CMsgDPGetNumberOfCurrentPlayersResponse>( packetMsg );

            var callback = new NumberOfPlayersCallback( msg.TargetJobID, msg.Body );
            Client.PostCallback( callback );
        }
        void HandleFindOrCreateLBResponse( IPacketMsg packetMsg )
        {
            var msg = new ClientMsgProtobuf<CMsgClientLBSFindOrCreateLBResponse>( packetMsg );

            var callback = new FindOrCreateLeaderboardCallback( msg.TargetJobID, msg.Body );
            Client.PostCallback( callback );
        }
        void HandleGetLBEntriesRespons( IPacketMsg packetMsg )
        {
            var msg = new ClientMsgProtobuf<CMsgClientLBSGetLBEntriesResponse>( packetMsg );

            var callback = new LeaderboardEntriesCallback( msg.TargetJobID, msg.Body );
            Client.PostCallback( callback );
        }
        #endregion
    }
}
