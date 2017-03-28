/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using SteamKit2.Internal;

namespace SteamKit2
{
    public sealed partial class SteamTrading
    {
        /// <summary>
        /// This callback is fired when this client receives a trade proposal.
        /// </summary>
        public sealed class TradeProposedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the Trade ID of his proposal, used for replying.
            /// </summary>
            public uint TradeID { get; private set; }

            /// <summary>
            /// Gets the SteamID of the client that sent the proposal.
            /// </summary>
            public SteamID OtherClient { get; private set; }


            internal TradeProposedCallback( CMsgTrading_InitiateTradeRequest msg )
            {
                this.TradeID = msg.trade_request_id;

                this.OtherClient = msg.other_steamid;
            }
        }


        /// <summary>
        /// This callback is fired when this client receives the response from a trade proposal.
        /// </summary>
        public sealed class TradeResultCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the Trade ID that this result is for.
            /// </summary>
            public uint TradeID { get; private set; }

            /// <summary>
            /// Gets the response of the trade proposal.
            /// </summary>
            public EEconTradeResponse Response { get; private set; }

            /// <summary>
            /// Gets the SteamID of the client that responded to the proposal.
            /// </summary>
            public SteamID OtherClient { get; private set; }

            /// <summary>
            /// Gets the number of days Steam Guard is required to have been active on this account.
            /// </summary>
            public uint NumDaysSteamGuardRequired { get; private set; }

            /// <summary>
            /// Gets the number of days a new device cannot trade for.
            /// </summary>
            public uint NumDaysNewDeviceCooldown { get; private set; }

            /// <summary>
            /// Gets the default number of days one cannot trade for after a password reset.
            /// </summary>
            public uint DefaultNumDaysPasswordResetProbation { get; private set; }

            /// <summary>
            /// Gets the number of days one cannot trade for after a password reset.
            /// </summary>
            public uint NumDaysPasswordResetProbation { get; private set; }


            internal TradeResultCallback( CMsgTrading_InitiateTradeResponse msg )
            {
                this.TradeID = msg.trade_request_id;

                this.Response = ( EEconTradeResponse )msg.response;

                this.OtherClient = msg.other_steamid;

                this.NumDaysSteamGuardRequired = msg.steamguard_required_days;

                this.NumDaysNewDeviceCooldown = msg.new_device_cooldown_days;

                this.DefaultNumDaysPasswordResetProbation = msg.default_password_reset_probation_days;

                this.NumDaysPasswordResetProbation = msg.password_reset_probation_days;
            }
        }


        /// <summary>
        /// This callback is fired when a trading session has started.
        /// </summary>
        public sealed class SessionStartCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the SteamID of the client that this the trading session has started with.
            /// </summary>
            public SteamID OtherClient { get; private set; }


            internal SessionStartCallback( CMsgTrading_StartSession msg )
            {
                this.OtherClient = msg.other_steamid;
            }
        }
    }
}
