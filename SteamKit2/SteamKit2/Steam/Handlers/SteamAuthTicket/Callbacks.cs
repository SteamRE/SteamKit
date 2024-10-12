using System.Collections.Generic;
using SteamKit2.Internal;

namespace SteamKit2
{
    public sealed partial class SteamAuthTicket
    {
        /// <summary>
        /// This callback is fired when Steam accepts our auth ticket as valid.
        /// </summary>
        public sealed class TicketAcceptedCallback : CallbackMsg
        {
            /// <summary>
            /// <see cref="List{T}"/> of AppIDs of the games that have generated tickets.
            /// </summary>
            public List<uint> AppIDs { get; private set; }
            /// <summary>
            /// <see cref="List{T}"/> of CRC32 hashes of activated tickets.
            /// </summary>
            public List<uint> ActiveTicketsCRC { get; private set; }
            /// <summary>
            /// Number of message in sequence.
            /// </summary>
            public uint MessageSequence { get; private set; }

            internal TicketAcceptedCallback( JobID jobId, CMsgClientAuthListAck body )
            {
                JobID = jobId;
                AppIDs = body.app_ids;
                ActiveTicketsCRC = body.ticket_crc;
                MessageSequence = body.message_sequence;
            }
        }

        /// <summary>
        /// This callback is fired when generated ticket was successfully used to authenticate user.
        /// </summary>
        public sealed class TicketAuthCompleteCallback : CallbackMsg
        {
            /// <summary>
            /// Steam response to authentication request.
            /// </summary>
            public EAuthSessionResponse AuthSessionResponse { get; }
            /// <summary>
            /// Authentication state.
            /// </summary>
            public uint State { get; }
            /// <summary>
            /// ID of the game the token was generated for.
            /// </summary>
            public GameID GameID { get; }
            /// <summary>
            /// <see cref="SteamKit2.SteamID"/> of the game owner.
            /// </summary>
            public SteamID OwnerSteamID { get; }
            /// <summary>
            /// <see cref="SteamKit2.SteamID"/> of the game server.
            /// </summary>
            public SteamID SteamID { get; }
            /// <summary>
            /// CRC of the ticket.
            /// </summary>
            public uint TicketCRC { get; }
            /// <summary>
            /// Sequence of the ticket.
            /// </summary>
            public uint TicketSequence { get; }

            internal TicketAuthCompleteCallback( JobID targetJobID, CMsgClientTicketAuthComplete body )
            {
                JobID = targetJobID;
                AuthSessionResponse = ( EAuthSessionResponse )body.eauth_session_response;
                State = body.estate;
                GameID = body.game_id;
                OwnerSteamID = body.owner_steam_id;
                SteamID = body.steam_id;
                TicketCRC = body.ticket_crc;
                TicketSequence = body.ticket_sequence;
            }
        }
    }
}
