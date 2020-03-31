using System.Collections.Generic;

namespace SteamKit2
{
    public partial class SteamMatchmaking
    {
        /// <summary>
        /// This callback is fired in response to <see cref="GetLobbyList"/>.
        /// </summary>
        public sealed class GetLobbyListCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the lobbies belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The result of the request.
            /// </summary>
            public EResult Result { get; }

            /// <summary>
            /// The list of lobbies matching the criteria specified with <see cref="GetLobbyList"/>.
            /// </summary>
            public List<Lobby> Lobbies { get; }

            internal GetLobbyListCallback( JobID jobId, uint appId, EResult res, List<Lobby> lobbies )
            {
                JobID = jobId;
                AppID = appId;
                Result = res;
                Lobbies = lobbies;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="CreateLobby"/>.
        /// </summary>
        public sealed class CreateLobbyCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the created lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The result of the request.
            /// </summary>
            public EResult Result { get; }

            /// <summary>
            /// The SteamID of the created lobby.
            /// </summary>
            public SteamID LobbySteamID { get; }

            internal CreateLobbyCallback( JobID jobId, uint appId, EResult res, SteamID lobbySteamId )
            {
                JobID = jobId;
                AppID = appId;
                Result = res;
                LobbySteamID = lobbySteamId;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="SetLobbyData"/>.
        /// </summary>
        public sealed class SetLobbyDataCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the targeted lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The result of the request.
            /// </summary>
            public EResult Result { get; }

            /// <summary>
            /// The SteamID of the targeted Lobby.
            /// </summary>
            public SteamID LobbySteamID { get; }

            internal SetLobbyDataCallback( JobID jobId, uint appId, EResult res, SteamID lobbySteamId )
            {
                JobID = jobId;
                AppID = appId;
                Result = res;
                LobbySteamID = lobbySteamId;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="SetLobbyOwner"/>.
        /// </summary>
        public sealed class SetLobbyOwnerCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the targeted lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The result of the request.
            /// </summary>
            public EResult Result { get; }

            /// <summary>
            /// The SteamID of the targeted Lobby.
            /// </summary>
            public SteamID LobbySteamID { get; }

            internal SetLobbyOwnerCallback( JobID jobId, uint appId, EResult res, SteamID lobbySteamId )
            {
                JobID = jobId;
                AppID = appId;
                Result = res;
                LobbySteamID = lobbySteamId;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="JoinLobby"/>.
        /// </summary>
        public sealed class JoinLobbyCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the targeted lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The result of the request.
            /// </summary>
            public EChatRoomEnterResponse ChatRoomEnterResponse { get; }

            /// <summary>
            /// The joined <see cref="Lobby"/>, when <see cref="ChatRoomEnterResponse"/> equals
            /// <see cref="EChatRoomEnterResponse.Success"/>, otherwise <c>null</c>
            /// </summary>
            public Lobby? Lobby { get; }

            internal JoinLobbyCallback( JobID jobId, uint appId, EChatRoomEnterResponse res, Lobby? lobby )
            {
                JobID = jobId;
                AppID = appId;
                ChatRoomEnterResponse = res;
                Lobby = lobby;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="LeaveLobby"/>.
        /// </summary>
        public sealed class LeaveLobbyCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the targeted lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The result of the request.
            /// </summary>
            public EResult Result { get; }

            /// <summary>
            /// The SteamID of the targeted Lobby.
            /// </summary>
            public SteamID LobbySteamID { get; }

            internal LeaveLobbyCallback( JobID jobId, uint appId, EResult res, SteamID lobbySteamId )
            {
                JobID = jobId;
                AppID = appId;
                Result = res;
                LobbySteamID = lobbySteamId;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="GetLobbyData"/>, as well as whenever Steam sends us updated lobby data.
        /// </summary>
        public sealed class LobbyDataCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the updated lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The lobby that was updated.
            /// </summary>
            public Lobby Lobby { get; }

            internal LobbyDataCallback( JobID jobId, uint appId, Lobby lobby )
            {
                JobID = jobId;
                AppID = appId;
                Lobby = lobby;
            }
        }

        /// <summary>
        /// This callback is fired whenever Steam informs us a user has joined a lobby.
        /// </summary>
        public sealed class UserJoinedLobbyCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The SteamID of the lobby that a member joined.
            /// </summary>
            public SteamID LobbySteamID { get; }

            /// <summary>
            /// The lobby member that joined.
            /// </summary>
            public Lobby.Member User { get; }

            internal UserJoinedLobbyCallback( uint appId, SteamID lobbySteamId, Lobby.Member user )
            {
                AppID = appId;
                LobbySteamID = lobbySteamId;
                User = user;
            }
        }

        /// <summary>
        /// This callback is fired whenever Steam informs us a user has left a lobby.
        /// </summary>
        public sealed class UserLeftLobbyCallback : CallbackMsg
        {
            /// <summary>
            /// ID of the app the lobby belongs to.
            /// </summary>
            public uint AppID { get; }

            /// <summary>
            /// The SteamID of the lobby that a member left.
            /// </summary>
            public SteamID LobbySteamID { get; }

            /// <summary>
            /// The lobby member that left.
            /// </summary>
            public Lobby.Member User { get; }

            internal UserLeftLobbyCallback( uint appId, SteamID lobbySteamId, Lobby.Member user )
            {
                AppID = appId;
                LobbySteamID = lobbySteamId;
                User = user;
            }
        }
    }
}
