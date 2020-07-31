using System;
using System.Collections.Generic;
using SteamKit2.GC.Underlords.Internal;

namespace NetHookAnalyzer2.Specializations
{
    static class UnderlordsSOHelper
    {
        public static Dictionary<int, Type> SOTypes = new Dictionary<int, Type>()
        {
            {1, typeof(CSOEconItem)},
            {3, typeof(CSOEconClaimCode)},
            {5, typeof(CSOItemRecipe)},
            {7, typeof(CSOEconGameAccountClient)},
            {38, typeof(CSOEconItemDropRateBonus)},
            {39, typeof(CSOEconItemLeagueViewPass)},
            {40, typeof(CSOEconItemEventTicket)},
            {42, typeof(CSOEconItemTournamentPassport)},

            {101, typeof(CSODACLobby)},
            {102, typeof(CSODACServerDynamicLobby)},
            {103, typeof(CSOGameAccountClient)},
            {104, typeof(CSODACParty)},
            {105, typeof(CSODACServerStaticLobby)},
            {106, typeof(CSOAccountSyncStorage)},
        };
    }
}
