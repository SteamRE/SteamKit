using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
using System.Collections.ObjectModel;

namespace NetHookAnalyzer2.Specializations
{
    static class Dota2SOHelper
    {
        public static Dictionary<int, Type> SOTypes = new Dictionary<int, Type>()
        {
            {1, typeof(CSOEconItem)},
            {5, typeof(CSOItemRecipe)},
            {7, typeof(CSOEconGameAccountClient)},
            {38, typeof(CSOEconItemDropRateBonus)},
            {39, typeof(CSOEconItemLeagueViewPass)},
            {40, typeof(CSOEconItemEventTicket)},
            {42, typeof(CSOEconItemTournamentPassport)},
            {2002, typeof(CSODOTAGameAccountClient)},
            {2003, typeof(CSODOTAParty)},
            {2004, typeof(CSODOTALobby)},
            {2006, typeof(CSODOTAPartyInvite)},
            {2007, typeof(CSODOTAGameHeroFavorites)},
            {2008, typeof(CSODOTAMapLocationState)},
            {2009, typeof(CMsgDOTATournament)},
            {2010, typeof(CSODOTAPlayerChallenge)},
            {2011, typeof(CSODOTALobbyInvite)},
            {2012, typeof(CSODOTAGameAccountPlus)},
        };
    }
}
