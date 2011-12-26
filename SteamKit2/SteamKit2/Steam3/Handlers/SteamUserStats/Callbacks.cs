using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public partial class SteamUserStats
    {
        /// <summary>
        /// This callback is fired in response to SteamUserStats.GetNumberOfCurrentPlayers
        /// </summary>
        public class NumberOfPlayersCallback : CallbackMsg
        {
            public EResult Result { get; private set; }
            public uint NumPlayers { get; private set; }


#if STATIC_CALLBACKS
            internal NumberOfPlayersCallback( SteamClient client, MsgClientGetNumberOfCurrentPlayersResponse resp )
                : base( client )
#else
            internal NumberOfPlayersCallback( MsgClientGetNumberOfCurrentPlayersResponse resp )
#endif
            {
                this.Result = resp.Result;
                this.NumPlayers = resp.NumPlayers;
            }
        }
    }
}
