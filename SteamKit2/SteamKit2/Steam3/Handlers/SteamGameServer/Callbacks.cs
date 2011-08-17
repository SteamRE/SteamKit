using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    partial class SteamGameServer
    {
        public sealed class StatusReplyCallback : CallbackMsg
        {
            public bool IsSecure { get; private set; }


#if STATIC_CALLBACKS
            internal StatusReplyCallback( SteamClient client, CMsgGSStatusReply reply )
                : base( client )
#else
            internal StatusReplyCallback( CMsgGSStatusReply reply )
#endif
            {
                IsSecure = reply.is_secure;
            }
        }

        public sealed class TicketAuthCallback : CallbackMsg
        {
            public SteamID SteamID { get; private set; }
            public GameID GameID { get; private set; }

            public uint State { get; private set; }

            public EAuthSessionResponse AuthSessionResponse { get; private set; }

            public uint TicketCRC { get; private set; }
            public uint TicketSequence { get; private set; }


#if STATIC_CALLBACKS
            internal TicketAuthCallback( SteamClient client, CMsgClientTicketAuthComplete tickAuth )
                : base( client )
#else
            internal TicketAuthCallback( CMsgClientTicketAuthComplete tickAuth )
#endif
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
