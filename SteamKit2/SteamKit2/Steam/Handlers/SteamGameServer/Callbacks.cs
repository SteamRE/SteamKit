/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using SteamKit2.Internal;

namespace SteamKit2
{
    partial class SteamGameServer
    {
        /// <summary>
        /// This callback is fired when the game server receives a status reply.
        /// </summary>
        public sealed class StatusReplyCallback : CallbackMsg
        {
            /// <summary>
            /// Gets a value indicating whether this game server is VAC secure.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this server is VAC secure; otherwise, <c>false</c>.
            /// </value>
            public bool IsSecure { get; private set; }


            internal StatusReplyCallback( CMsgGSStatusReply reply )
            {
                IsSecure = reply.is_secure;
            }
        }

        /// <summary>
        /// This callback is fired when ticket authentication has completed.
        /// </summary>
        public sealed class TicketAuthCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the SteamID the ticket auth completed for.
            /// </summary>
            public SteamID SteamID { get; private set; }
            /// <summary>
            /// Gets the GameID the ticket was for.
            /// </summary>
            public GameID GameID { get; private set; }

            /// <summary>
            /// Gets the authentication state.
            /// </summary>
            public uint State { get; private set; }

            /// <summary>
            /// Gets the auth session response.
            /// </summary>
            public EAuthSessionResponse AuthSessionResponse { get; private set; }

            /// <summary>
            /// Gets the ticket CRC.
            /// </summary>
            public uint TicketCRC { get; private set; }
            /// <summary>
            /// Gets the ticket sequence.
            /// </summary>
            public uint TicketSequence { get; private set; }


            internal TicketAuthCallback( CMsgClientTicketAuthComplete tickAuth )
            {
                SteamID = tickAuth.steam_id;
                GameID = tickAuth.game_id;

                State = tickAuth.estate;

                AuthSessionResponse = ( EAuthSessionResponse )tickAuth.eauth_session_response;

                TicketCRC = tickAuth.ticket_crc;
                TicketSequence = tickAuth.ticket_sequence;
            }
        }
    }
}
