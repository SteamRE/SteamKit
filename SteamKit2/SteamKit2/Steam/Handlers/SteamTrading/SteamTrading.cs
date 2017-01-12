﻿/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for initializing Steam trades with other clients.
    /// </summary>
    public sealed partial class SteamTrading : ClientMsgMappingHandler
    {
        /// <inheritdoc />
        protected override Dictionary<EMsg, Action<IPacketMsg>> DispatchMap { get; }

        internal SteamTrading()
        {
            DispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.EconTrading_InitiateTradeProposed, HandleTradeProposed },
                { EMsg.EconTrading_InitiateTradeResult, HandleTradeResult },
                { EMsg.EconTrading_StartSession, HandleStartSession },
            };
        }


        /// <summary>
        /// Proposes a trade to another client.
        /// </summary>
        /// <param name="user">The client to trade.</param>
        public void Trade( SteamID user )
        {
            var tradeReq = new ClientMsgProtobuf<CMsgTrading_InitiateTradeRequest>( EMsg.EconTrading_InitiateTradeRequest );

            tradeReq.Body.other_steamid = user;

            Client.Send( tradeReq );
        }

        /// <summary>
        /// Responds to a trade proposal.
        /// </summary>
        /// <param name="tradeId">The trade id of the received proposal.</param>
        /// <param name="acceptTrade">if set to <c>true</c>, the trade will be accepted.</param>
        public void RespondToTrade( uint tradeId, bool acceptTrade )
        {
            var tradeResp = new ClientMsgProtobuf<CMsgTrading_InitiateTradeResponse>( EMsg.EconTrading_InitiateTradeResponse );

            tradeResp.Body.trade_request_id = tradeId;
            tradeResp.Body.response = acceptTrade ? 0u : 1u;

            Client.Send( tradeResp );
        }

        /// <summary>
        /// Cancels an already sent trade proposal.
        /// </summary>
        /// <param name="user">The user.</param>
        public void CancelTrade( SteamID user )
        {
            var cancelTrade = new ClientMsgProtobuf<CMsgTrading_CancelTradeRequest>( EMsg.EconTrading_CancelTradeRequest );

            cancelTrade.Body.other_steamid = user;

            Client.Send( cancelTrade );
        }

        #region ClientMsg Handlers
        void HandleTradeProposed( IPacketMsg packetMsg )
        {
            var tradeProp = new ClientMsgProtobuf<CMsgTrading_InitiateTradeRequest>( packetMsg );

            var callback = new TradeProposedCallback( tradeProp.Body );
            Client.PostCallback( callback );
        }
        void HandleTradeResult( IPacketMsg packetMsg )
        {
            var tradeResult = new ClientMsgProtobuf<CMsgTrading_InitiateTradeResponse>( packetMsg );

            var callback = new TradeResultCallback( tradeResult.Body );
            Client.PostCallback( callback );
        }
        void HandleStartSession( IPacketMsg packetMsg )
        {
            var startSess = new ClientMsgProtobuf<CMsgTrading_StartSession>( packetMsg );

            var callback = new SessionStartCallback( startSess.Body );
            Client.PostCallback( callback );
        }
        #endregion

    }
}
