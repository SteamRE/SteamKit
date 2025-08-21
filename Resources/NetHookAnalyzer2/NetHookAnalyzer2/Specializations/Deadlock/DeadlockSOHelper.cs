using System;
using System.Collections.Generic;
using SteamKit2.GC.Deadlock.Internal;

namespace NetHookAnalyzer2.Specializations
{
    static class DeadlockSOHelper
    {
        public static Dictionary<int, Type> SOTypes = new()
        {
			{1, typeof(CSOEconItem)},
			{7, typeof(CSOEconGameAccountClient)},
			{101, typeof(CSOCitadelLobby)},
			{102, typeof(CSOCitadelServerStaticLobby)},
			{104, typeof(CSOGameAccountClient)},
			{105, typeof(CSOCitadelParty)},
			{106, typeof(CSOCitadelServerDynamicLobby)},
			{107, typeof(CSOAccountHeroInfo)},
			{109, typeof(CSOAccountChallenge)},
			{110, typeof(CSOCitadelHideoutLobby)},
		};
    }
}
