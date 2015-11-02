using System;
using System.Collections.Generic;
using SteamKit2;
using SteamKit2.Internal;

using CSGO = SteamKit2.GC.CSGO.Internal;
using Dota = SteamKit2.GC.Dota.Internal;
using TF2 = SteamKit2.GC.TF2.Internal;

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

            { EMsg.AMGameServerUpdate, typeof(CMsgGameServerData) },

            { EMsg.ClientDPUpdateAppJobReport, typeof(CMsgClientUpdateAppJobReport) },
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

                [(uint)Dota.ESOMsg.k_ESOMsg_Create] = typeof(Dota.CMsgSOSingleObject),
                [(uint)Dota.ESOMsg.k_ESOMsg_Destroy] = typeof(Dota.CMsgSOSingleObject),
                [(uint)Dota.ESOMsg.k_ESOMsg_Update] = typeof(Dota.CMsgSOSingleObject),
                [(uint)Dota.ESOMsg.k_ESOMsg_UpdateMultiple] = typeof(Dota.CMsgSOMultipleObjects),
            },
            [WellKnownAppIDs.CounterStrikeGlobalOffensive] = new Dictionary<uint, Type>
            {
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCClientHello] = typeof(CSGO.CMsgClientHello),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCClientWelcome] = typeof(CSGO.CMsgClientWelcome),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCServerHello] = typeof(CSGO.CMsgClientHello),
                [(uint)CSGO.EGCBaseClientMsg.k_EMsgGCServerWelcome] = typeof(CSGO.CMsgClientWelcome),

                [(uint)CSGO.ESOMsg.k_ESOMsg_Create] = typeof(CSGO.CMsgSOSingleObject),
                [(uint)CSGO.ESOMsg.k_ESOMsg_Destroy] = typeof(CSGO.CMsgSOSingleObject),
                [(uint)CSGO.ESOMsg.k_ESOMsg_Update] = typeof(CSGO.CMsgSOSingleObject),
                [(uint)CSGO.ESOMsg.k_ESOMsg_UpdateMultiple] = typeof(CSGO.CMsgSOMultipleObjects),
            },
        };
    }
}
