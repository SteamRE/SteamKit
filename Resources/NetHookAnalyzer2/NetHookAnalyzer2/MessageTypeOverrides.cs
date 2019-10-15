using System;
using System.Collections.Generic;
using SteamKit2;
using SteamKit2.Internal;

using CSGO = SteamKit2.GC.CSGO.Internal;
using Dota = SteamKit2.GC.Dota.Internal;
using TF2 = SteamKit2.GC.TF2.Internal;
using Artifact = SteamKit2.GC.Artifact.Internal;
using Underlords = SteamKit2.GC.Underlords.Internal;

namespace NetHookAnalyzer2
{
    static class MessageTypeOverrides
    {
        public static Dictionary<EMsg, Type> BodyMap = new Dictionary<EMsg, Type>
        {
            { EMsg.ClientLogonGameServer, typeof(CMsgClientLogon) },
            { EMsg.ClientGamesPlayed, typeof(CMsgClientGamesPlayed) },
            { EMsg.ClientGamesPlayedNoDataBlob, typeof(CMsgClientGamesPlayed) },
            { EMsg.ClientGamesPlayedWithDataBlob, typeof(CMsgClientGamesPlayed) },
            { EMsg.ClientToGC, typeof(CMsgGCClient) },
            { EMsg.ClientFromGC, typeof(CMsgGCClient) },
            { EMsg.ClientFriendMsgIncoming, typeof(CMsgClientFriendMsgIncoming) },
            { EMsg.ClientFriendMsgEchoToSender, typeof(CMsgClientFriendMsgIncoming) },
            { EMsg.ClientCurrentUIMode, typeof(CMsgClientUIMode) },
            { EMsg.ClientGetNumberOfCurrentPlayersDP, typeof(CMsgDPGetNumberOfCurrentPlayers) },
            { EMsg.ClientGetNumberOfCurrentPlayersDPResponse, typeof(CMsgDPGetNumberOfCurrentPlayersResponse) },

            { EMsg.AMGameServerUpdate, typeof(CMsgGameServerData) },

            { EMsg.ClientDPUpdateAppJobReport, typeof(CMsgClientUpdateAppJobReport) },

            { EMsg.ClientPlayingSessionState, typeof(CMsgClientPlayingSessionState) },

            { EMsg.ClientNetworkingCertRequestResponse, typeof(CMsgClientNetworkingCertReply) }
        };

        public static Dictionary<uint, Dictionary<uint, Type>> GCBodyMap = new Dictionary<uint, Dictionary<uint, Type>>
        {
            [WellKnownAppIDs.TeamFortress2] = new Dictionary<uint, Type>
            {
                [(uint)TF2.EGCBaseClientMsg.k_EMsgGCClientHello] = typeof(TF2.CMsgClientHello),
                [(uint)TF2.EGCBaseClientMsg.k_EMsgGCClientWelcome] = typeof(TF2.CMsgClientWelcome),
                [(uint)TF2.EGCBaseClientMsg.k_EMsgGCServerHello] = typeof(TF2.CMsgClientHello),
                [(uint)TF2.EGCBaseClientMsg.k_EMsgGCServerWelcome] = typeof(TF2.CMsgClientWelcome),

                [(uint)TF2.ESOMsg.k_ESOMsg_Create] = typeof(TF2.CMsgSOSingleObject),
                [(uint)TF2.ESOMsg.k_ESOMsg_Destroy] = typeof(TF2.CMsgSOSingleObject),
                [(uint)TF2.ESOMsg.k_ESOMsg_Update] = typeof(TF2.CMsgSOSingleObject),
                [(uint)TF2.ESOMsg.k_ESOMsg_UpdateMultiple] = typeof(TF2.CMsgSOMultipleObjects),
            },
            [WellKnownAppIDs.Dota2] = new Dictionary<uint, Type>
            {
                [(uint)Dota.EGCBaseClientMsg.k_EMsgGCClientHello] = typeof(Dota.CMsgClientHello),
                [(uint)Dota.EGCBaseClientMsg.k_EMsgGCClientWelcome] = typeof(Dota.CMsgClientWelcome),
                [(uint)Dota.EGCBaseClientMsg.k_EMsgGCServerHello] = typeof(Dota.CMsgClientHello),
                [(uint)Dota.EGCBaseClientMsg.k_EMsgGCServerWelcome] = typeof(Dota.CMsgClientWelcome),
                [(uint)Dota.EGCBaseClientMsg.k_EMsgGCClientConnectionStatus] = typeof(Dota.CMsgConnectionStatus),
                [(uint)Dota.EGCBaseClientMsg.k_EMsgGCServerConnectionStatus] = typeof(Dota.CMsgConnectionStatus),

                [(uint)Dota.ESOMsg.k_ESOMsg_Create] = typeof(Dota.CMsgSOSingleObject),
                [(uint)Dota.ESOMsg.k_ESOMsg_Destroy] = typeof(Dota.CMsgSOSingleObject),
                [(uint)Dota.ESOMsg.k_ESOMsg_Update] = typeof(Dota.CMsgSOSingleObject),
                [(uint)Dota.ESOMsg.k_ESOMsg_UpdateMultiple] = typeof(Dota.CMsgSOMultipleObjects),

                [(uint)Dota.EDOTAGCMsg.k_EMsgGCToClientTeamsInfo] = typeof(Dota.CMsgDOTATeamsInfo),
                [(uint)Dota.EDOTAGCMsg.k_EMsgClientToGCMyTeamInfoRequest] = typeof(Dota.CMsgDOTAMyTeamInfoRequest),
            },
            [WellKnownAppIDs.CounterStrikeGlobalOffensive] = new Dictionary<uint, Type>
            {
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCClientHello] = typeof(CSGO.CMsgClientHello),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCClientWelcome] = typeof(CSGO.CMsgClientWelcome),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCServerHello] = typeof(CSGO.CMsgClientHello),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCServerWelcome] = typeof(CSGO.CMsgClientWelcome),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCClientConnectionStatus] = typeof(CSGO.CMsgConnectionStatus),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCServerConnectionStatus] = typeof(CSGO.CMsgConnectionStatus),

                [(uint)CSGO.ESOMsg.k_ESOMsg_Create] = typeof(CSGO.CMsgSOSingleObject),
                [(uint)CSGO.ESOMsg.k_ESOMsg_Destroy] = typeof(CSGO.CMsgSOSingleObject),
                [(uint)CSGO.ESOMsg.k_ESOMsg_Update] = typeof(CSGO.CMsgSOSingleObject),
                [(uint)CSGO.ESOMsg.k_ESOMsg_UpdateMultiple] = typeof(CSGO.CMsgSOMultipleObjects),
            },
            [WellKnownAppIDs.Artifact] = new Dictionary<uint, Type>
            {
                [(uint)Artifact.EGCBaseClientMsg.k_EMsgGCClientHello] = typeof(Artifact.CMsgClientHello),
                [(uint)Artifact.EGCBaseClientMsg.k_EMsgGCClientWelcome] = typeof(Artifact.CMsgClientWelcome),
                [(uint)Artifact.EGCBaseClientMsg.k_EMsgGCServerHello] = typeof(Artifact.CMsgClientHello),
                [(uint)Artifact.EGCBaseClientMsg.k_EMsgGCServerWelcome] = typeof(Artifact.CMsgClientWelcome),
                [(uint)Artifact.EGCBaseClientMsg.k_EMsgGCClientConnectionStatus] = typeof(Artifact.CMsgConnectionStatus),
                [(uint)Artifact.EGCBaseClientMsg.k_EMsgGCServerConnectionStatus] = typeof(Artifact.CMsgConnectionStatus),

                [(uint)Artifact.ESOMsg.k_ESOMsg_Create] = typeof(Artifact.CMsgSOSingleObject),
                [(uint)Artifact.ESOMsg.k_ESOMsg_Destroy] = typeof(Artifact.CMsgSOSingleObject),
                [(uint)Artifact.ESOMsg.k_ESOMsg_Update] = typeof(Artifact.CMsgSOSingleObject),
                [(uint)Artifact.ESOMsg.k_ESOMsg_UpdateMultiple] = typeof(Artifact.CMsgSOMultipleObjects),
            },
            [ WellKnownAppIDs.Underlords ] = new Dictionary<uint, Type>
            {
                [(uint)Underlords.EGCBaseClientMsg.k_EMsgGCClientHello] = typeof(Underlords.CMsgClientHello),
                [(uint)Underlords.EGCBaseClientMsg.k_EMsgGCClientWelcome] = typeof(Underlords.CMsgClientWelcome),
                [(uint)Underlords.EGCBaseClientMsg.k_EMsgGCServerHello] = typeof(Underlords.CMsgClientHello),
                [(uint)Underlords.EGCBaseClientMsg.k_EMsgGCServerWelcome] = typeof(Underlords.CMsgClientWelcome),
                [(uint)Underlords.EGCBaseClientMsg.k_EMsgGCClientConnectionStatus] = typeof(Underlords.CMsgConnectionStatus),
                [(uint)Underlords.EGCBaseClientMsg.k_EMsgGCServerConnectionStatus] = typeof(Underlords.CMsgConnectionStatus),

                [(uint)Underlords.ESOMsg.k_ESOMsg_Create] = typeof(Underlords.CMsgSOSingleObject),
                [(uint)Underlords.ESOMsg.k_ESOMsg_Destroy] = typeof(Underlords.CMsgSOSingleObject),
                [(uint)Underlords.ESOMsg.k_ESOMsg_Update] = typeof(Underlords.CMsgSOSingleObject),
                [(uint)Underlords.ESOMsg.k_ESOMsg_UpdateMultiple] = typeof(Underlords.CMsgSOMultipleObjects),
            },
        };
    }
}
