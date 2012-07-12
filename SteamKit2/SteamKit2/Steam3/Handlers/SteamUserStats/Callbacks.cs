/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamUserStats
    {
        /// <summary>
        /// This callback is fired in response to <see cref="GetNumberOfCurrentPlayers" />.
        /// </summary>
        public class NumberOfPlayersCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }
            /// <summary>
            /// Gets the current number of players according to Steam.
            /// </summary>
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
