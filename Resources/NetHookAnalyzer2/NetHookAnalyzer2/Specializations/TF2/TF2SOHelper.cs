using System;
using System.Collections.Generic;
using SteamKit2.GC.TF2.Internal;

namespace NetHookAnalyzer2.Specializations
{
    class TF2SOHelper
    {
        public static Dictionary<int, Type> SOTypes = new Dictionary<int, Type>()
        {
            {1, typeof(CSOEconItem)},
            {2, typeof(CSOTFPlayerInfo)},
            {3, typeof(CSOEconClaimCode)},
            {5, typeof(CSOItemRecipe)},
            {7, typeof(CSOEconGameAccountClient)},
            {19, typeof(CSOTFDuelSummary)},
            {28, typeof(CSOTFMapContribution)},
            {36, typeof(CSOClassPresetClientData)},
            {37, typeof(CSOEconGameAccountForGameServers)},
            {38, typeof(CSOWarData)},
            {39, typeof(CSOTFLadderPlayerStats)},
            {40, typeof(CSOTFMatchResultPlayerStats)},
            {41, typeof(CMsgTFXPSource)},
            {42, typeof(CMsgGCNotification)},
            {2003, typeof(CSOTFParty)},
            {2004, typeof(CSOTFGameServerLobby)},
            {2006, typeof(CSOTFPartyInvite)},
			{2007, typeof(CSOTFRatingData)},
            {2008, typeof(CTFLobbyInviteProto)},
        };
    }
}
