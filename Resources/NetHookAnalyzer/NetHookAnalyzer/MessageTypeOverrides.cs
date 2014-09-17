using System;
using System.Collections.Generic;
using SteamKit2;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;

namespace NetHookAnalyzer
{
    static class MessageTypeOverrides
    {
        public static Dictionary<EMsg, Type> BodyMap = new Dictionary<EMsg, Type>
        {
            { EMsg.ClientLogonGameServer, typeof(CMsgClientLogon) },
            { EMsg.ClientGamesPlayed, typeof(CMsgClientGamesPlayed) },
            { EMsg.ClientGamesPlayedNoDataBlob, typeof(CMsgClientGamesPlayed) },
            { EMsg.ClientGamesPlayedWithDataBlob, typeof(CMsgClientGamesPlayed) },
            { EMsg.ClientFriendMsgIncoming, typeof(CMsgClientFriendMsgIncoming) },
            { EMsg.ClientFriendMsgEchoToSender, typeof(CMsgClientFriendMsgIncoming) },

            { EMsg.AMGameServerUpdate, typeof(CMsgGameServerData) },

            { EMsg.ClientDPUpdateAppJobReport, typeof(CMsgClientUpdateAppJobReport) },
        };

        public static Dictionary<uint, Type> GCBodyMap = new Dictionary<uint, Type>
        {
            { (uint)EGCBaseClientMsg.k_EMsgGCClientHello, typeof(CMsgClientHello) },
            { (uint)EGCBaseClientMsg.k_EMsgGCClientWelcome, typeof(CMsgClientWelcome) },
            { (uint)EGCBaseClientMsg.k_EMsgGCServerHello, typeof(CMsgClientHello) },
            { (uint)EGCBaseClientMsg.k_EMsgGCServerWelcome, typeof(CMsgClientWelcome) },

            { (uint)ESOMsg.k_ESOMsg_Create, typeof(CMsgSOSingleObject) },
            { (uint)ESOMsg.k_ESOMsg_Destroy, typeof(CMsgSOSingleObject) },
            { (uint)ESOMsg.k_ESOMsg_Update, typeof(CMsgSOSingleObject) },
            { (uint)ESOMsg.k_ESOMsg_UpdateMultiple, typeof(CMsgSOMultipleObjects) },
        };
    }
}
