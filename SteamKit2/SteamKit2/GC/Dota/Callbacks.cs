/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;

namespace SteamKit2.GC.Dota
{
    public partial class DotaGCHandler
    {
        /// <summary>
        ///     An unhandled cache unsubscribe
        /// </summary>
        public sealed class CacheUnsubscribed : CallbackMsg
        {
            public CMsgSOCacheUnsubscribed result;

            internal CacheUnsubscribed(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Chat message received from a channel
        /// </summary>
        public sealed class ChatMessage : CallbackMsg
        {
            public CMsgDOTAChatMessage result;

            internal ChatMessage(CMsgDOTAChatMessage msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Connection status
        /// </summary>
        public sealed class ConnectionStatus : CallbackMsg
        {
            public CMsgConnectionStatus result;

            internal ConnectionStatus(CMsgConnectionStatus msg)
            {
                result = msg;
            }
        }

        public sealed class GCWelcomeCallback : CallbackMsg
        {
            public uint Version;

            internal GCWelcomeCallback(CMsgClientWelcome msg)
            {
                Version = msg.version;
            }
        }

        /// <summary>
        ///     Handle invitation created
        /// </summary>
        public sealed class InvitationCreated : CallbackMsg
        {
            public CMsgInvitationCreated invitation;

            internal InvitationCreated(CMsgInvitationCreated msg)
            {
                invitation = msg;
            }
        }

        /// <summary>
        ///     Reponse when trying to join a chat chanel
        /// </summary>
        public sealed class JoinChatChannelResponse : CallbackMsg
        {
            public CMsgDOTAJoinChatChannelResponse result;

            internal JoinChatChannelResponse(CMsgDOTAJoinChatChannelResponse msg)
            {
                result = msg;
            }
        }

        public sealed class LiveLeagueGameUpdate : CallbackMsg
        {
            public CMsgDOTALiveLeagueGameUpdate result;

            internal LiveLeagueGameUpdate(CMsgDOTALiveLeagueGameUpdate msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Match result response.
        /// </summary>
        public sealed class MatchResultResponse : CallbackMsg
        {
            public CMsgGCMatchDetailsResponse result;

            internal MatchResultResponse(CMsgGCMatchDetailsResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     A user joined our chat channel
        /// </summary>
        public sealed class OtherJoinedChannel : CallbackMsg
        {
            public CMsgDOTAOtherJoinedChatChannel result;

            internal OtherJoinedChannel(CMsgDOTAOtherJoinedChatChannel msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     A user left out chat chanel
        /// </summary>
        public sealed class OtherLeftChannel : CallbackMsg
        {
            public CMsgDOTAOtherLeftChatChannel result;

            internal OtherLeftChannel(CMsgDOTAOtherLeftChatChannel msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When the party invite is cleared this is sent ou
        /// </summary>
        public sealed class PartyInviteLeave : CallbackMsg
        {
            public CMsgSOCacheUnsubscribed result;

            internal PartyInviteLeave(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When receiving a party invite a snapshot is sent out.
        /// </summary>
        public sealed class PartyInviteSnapshot : CallbackMsg
        {
            public CSODOTAPartyInvite invite;

            internal PartyInviteSnapshot(CSODOTAPartyInvite msg)
            {
                invite = msg;
            }
        }

        /// <summary>
        ///     Party invite was updated
        /// </summary>
        public sealed class PartyInviteUpdate : CallbackMsg
        {
            public CSODOTAPartyInvite invite;
            public CSODOTAPartyInvite oldInvite;

            internal PartyInviteUpdate(CSODOTAPartyInvite msg, CSODOTAPartyInvite oldLob)
            {
                invite = msg;
                oldInvite = oldLob;
            }
        }

        /// <summary>
        ///     When leaving a party this is sent out
        /// </summary>
        public sealed class PartyLeave : CallbackMsg
        {
            public CMsgSOCacheUnsubscribed result;

            internal PartyLeave(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When joining a party a snapshot is sent out.
        /// </summary>
        public sealed class PartySnapshot : CallbackMsg
        {
            public CSODOTAParty party;

            internal PartySnapshot(CSODOTAParty msg)
            {
                party = msg;
            }
        }

        /// <summary>
        ///     Party was updated
        /// </summary>
        public sealed class PartyUpdate : CallbackMsg
        {
            public CSODOTAParty oldParty;
            public CSODOTAParty party;

            internal PartyUpdate(CSODOTAParty msg, CSODOTAParty oldLob)
            {
                party = msg;
                oldParty = oldLob;
            }
        }

        /// <summary>
        ///     Ping request from GC
        /// </summary>
        public sealed class PingRequest : CallbackMsg
        {
            public CMsgGCClientPing request;

            internal PingRequest(CMsgGCClientPing msg)
            {
                request = msg;
            }
        }

        /// <summary>
        ///     We receive a popup. (e.g. Kicked from lobby)
        /// </summary>
        public sealed class Popup : CallbackMsg
        {
            public CMsgDOTAPopup result;

            internal Popup(CMsgDOTAPopup msg)
            {
                result = msg;
            }
        }

        public sealed class PracticeLobbyJoinResponse : CallbackMsg
        {
            public CMsgPracticeLobbyJoinResponse result;

            internal PracticeLobbyJoinResponse(CMsgPracticeLobbyJoinResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When leaving a practice lobby this is sent out
        /// </summary>
        public sealed class PracticeLobbyLeave : CallbackMsg
        {
            public CMsgSOCacheUnsubscribed result;

            internal PracticeLobbyLeave(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        public sealed class PracticeLobbyListResponse : CallbackMsg
        {
            public CMsgPracticeLobbyListResponse result;

            internal PracticeLobbyListResponse(CMsgPracticeLobbyListResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When joining a lobby a snapshot is sent out.
        /// </summary>
        public sealed class PracticeLobbySnapshot : CallbackMsg
        {
            public CSODOTALobby lobby;

            internal PracticeLobbySnapshot(CSODOTALobby msg)
            {
                lobby = msg;
            }
        }

        /// <summary>
        ///     Lobby was updated
        /// </summary>
        public sealed class PracticeLobbyUpdate : CallbackMsg
        {
            public CSODOTALobby lobby;
            public CSODOTALobby oldLobby;

            internal PracticeLobbyUpdate(CSODOTALobby msg, CSODOTALobby oldLob)
            {
                lobby = msg;
                oldLobby = oldLob;
            }
        }

        /// <summary>
        ///     When receiving a steam component of the party invite
        /// </summary>
        public sealed class SteamPartyInvite : CallbackMsg
        {
            public CMsgClientUDSInviteToGame result;

            internal SteamPartyInvite(CMsgClientUDSInviteToGame msg)
            {
                result = msg;
            }
        }

        public sealed class UnhandledDotaGCCallback : CallbackMsg
        {
            public IPacketGCMsg Message;

            internal UnhandledDotaGCCallback(IPacketGCMsg msg)
            {
                Message = msg;
            }
        }
    }
}