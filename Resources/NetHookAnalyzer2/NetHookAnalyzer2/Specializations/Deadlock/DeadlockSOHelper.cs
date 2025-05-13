using System;
using System.Collections.Generic;
using SteamKit2.GC.Deadlock.Internal;

namespace NetHookAnalyzer2.Specializations
{
    static class DeadlockSOHelper
    {
        public static Dictionary<int, Type> SOTypes = new()
        {
			{101, typeof(CSOCitadelLobby)},
			{105, typeof(CSOCitadelParty)},
		};
    }
}
