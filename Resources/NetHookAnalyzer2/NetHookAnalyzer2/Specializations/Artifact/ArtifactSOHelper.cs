using System;
using System.Collections.Generic;
using SteamKit2.GC.Artifact.Internal;

namespace NetHookAnalyzer2.Specializations
{
    static class ArtifactSOHelper
    {
        public static Dictionary<int, Type> SOTypes = new Dictionary<int, Type>()
        {
            {1, typeof(CSOEconItem)},
            {7, typeof(CSOEconGameAccountClient)},


            {101, typeof(CSODCGLobby) },
            {102, typeof(CSOPlayerLimitedProgress) },
            {104, typeof(CSODCGServerLobby) },
            {106, typeof(CSOGameAccountClient) },
            {107, typeof(CSOPhantomItem) },
            {108, typeof(CSOCardAchievement) },
            {109, typeof(CSOGauntlet) },
            {110, typeof(CSODCGPrivateLobby) },
            {111, typeof(CSOTourneyMembership) },
            {112, typeof(CSODCGTourneyInvite) },
        };
    }
}
