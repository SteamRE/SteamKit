using System;
using System.Collections.Generic;
using SteamKit2.GC.CSGO.Internal;

namespace NetHookAnalyzer2.Specializations
{
    class CSGOSOHelper
    {
        public static Dictionary<int, Type> SOTypes = new Dictionary<int, Type>()
        {
            {1, typeof(CSOEconItem)},
            {2, typeof(CSOPersonaDataPublic)},
            {5, typeof(CSOItemRecipe)},
            {7, typeof(CSOEconGameAccountClient)},
            {38, typeof(CSOEconItemDropRateBonus)},
            {40, typeof(CSOAccountSeasonalOperation)},
            {41, typeof(CSOAccountSeasonalOperation)},
            {43, typeof(CSOEconDefaultEquippedDefinitionInstanceClient)},
            {45, typeof(CSOEconCoupon)},
            {46, typeof(CSOQuestProgress)},
        };
    }
}
