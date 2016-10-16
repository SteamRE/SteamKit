namespace SteamKit2.GC.Dota.Internal
{
    /// <summary>
    /// Cache types
    /// </summary>
    internal enum CacheSubscritionTypes
    {
        /// <summary>
        /// An economy item.
        /// </summary>
        EconItem = 1,

        /// <summary>
        /// An econ item recipe.
        /// </summary>
        ItemRecipe = 5,

        /// <summary>
        /// Game account client for Econ.
        /// </summary>
        EconGameAccountClient = 7,

        /// <summary>
        /// Selected item preset.
        /// </summary>
        SelectedItemPreset = 35,

        /// <summary>
        /// Item preset instance.
        /// </summary>
        ItemPresetInstance = 36,

        /// <summary>
        /// Active drop rate bonus.
        /// </summary>
        DropRateBonus = 38,

        /// <summary>
        /// Pass to view a league.
        /// </summary>
        LeagueViewPass = 39,

        /// <summary>
        /// Event ticket.
        /// </summary>
        EventTicket = 40,

        /// <summary>
        /// Item tournament passport.
        /// </summary>
        ItemTournamentPassport = 42,

        /// <summary>
        /// DOTA 2 game account client.
        /// </summary>
        GameAccountClient = 2002,

        /// <summary>
        /// A Dota 2 party.
        /// </summary>
        Party = 2003,

        /// <summary>
        /// A Dota 2 lobby.
        /// </summary>
        Lobby = 2004,

        /// <summary>
        /// A party invite.
        /// </summary>
        Partyinvite = 2006,

        /// <summary>
        /// Game hero favorites.
        /// </summary>
        GameHeroFavorites = 2007,

        /// <summary>
        /// Ping map location state.
        /// </summary>
        MapLocationState = 2008,

        /// <summary>
        /// Tournament.
        /// </summary>
        Tournament = 2009,

        /// <summary>
        /// A player challenge.
        /// </summary>
        PlayerChallenge = 2010,

        /// <summary>
        /// A lobby invite, introduced in Reborn.
        /// </summary>
        Lobbyinvite = 2011
    }
}
