using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SteamKit2
{
    public partial class SteamMatchmaking
    {
        class LobbyCache
        {
            readonly ConcurrentDictionary<uint, ConcurrentDictionary<SteamID, Lobby>> lobbies =
                new ConcurrentDictionary<uint, ConcurrentDictionary<SteamID, Lobby>>();

            public Lobby? GetLobby( uint appId, SteamID lobbySteamId )
            {
                return GetAppLobbies( appId ).TryGetValue( lobbySteamId, out var lobby ) ? lobby : null;
            }

            public void CacheLobby( uint appId, Lobby lobby )
            {
                GetAppLobbies( appId )[ lobby.SteamID ] = lobby;
            }

            public Lobby.Member? AddLobbyMember( uint appId, Lobby lobby, SteamID memberId, string personaName )
            {
                var existingMember = lobby.Members.FirstOrDefault( m => m.SteamID == memberId );

                if ( existingMember != null )
                {
                    // Already in lobby
                    return null;
                }

                var addedMember = new Lobby.Member( memberId, personaName );

                var members = new List<Lobby.Member>( lobby.Members.Count + 1 );
                members.AddRange( lobby.Members );
                members.Add( addedMember );

                UpdateLobbyMembers( appId, lobby, members.AsReadOnly() );

                return addedMember;
            }

            public Lobby.Member? RemoveLobbyMember( uint appId, Lobby lobby, SteamID memberId )
            {
                var removedMember = lobby.Members.FirstOrDefault( m => m.SteamID.Equals( memberId ) );

                if ( removedMember == null )
                {
                    return null;
                }

                var members = lobby.Members.Where( m => !m.Equals( removedMember ) ).ToList();

                if ( members.Count > 0 )
                {
                    UpdateLobbyMembers( appId, lobby, members.AsReadOnly() );
                }
                else
                {
                    // Steam deletes lobbies that contain no members
                    DeleteLobby( appId, lobby.SteamID );
                }

                return removedMember;
            }

            public void ClearLobbyMembers( uint appId, SteamID lobbySteamId )
            {
                var lobby = GetLobby( appId, lobbySteamId );

                if ( lobby != null )
                {
                    UpdateLobbyMembers( appId, lobby, null, null );
                }
            }

            public void UpdateLobbyOwner( uint appId, SteamID lobbySteamId, SteamID ownerSteamId )
            {
                var lobby = GetLobby( appId, lobbySteamId );

                if ( lobby != null )
                {
                    UpdateLobbyMembers( appId, lobby, ownerSteamId, lobby.Members );
                }
            }

            public void UpdateLobbyMembers( uint appId, Lobby lobby, IReadOnlyList<Lobby.Member> members )
            {
                UpdateLobbyMembers( appId, lobby, lobby.OwnerSteamID, members );
            }

            public void UpdateLobbyMembers( uint appId, Lobby lobby, List<Lobby.Member> members )
            {
                UpdateLobbyMembers( appId, lobby, lobby.OwnerSteamID, members.AsReadOnly() );
            }

            public void Clear()
            {
                lobbies.Clear();
            }

            void UpdateLobbyMembers( uint appId, Lobby lobby, SteamID? owner, IReadOnlyList<Lobby.Member>? members )
            {
                CacheLobby( appId, new Lobby(
                    lobby.SteamID,
                    lobby.LobbyType,
                    lobby.LobbyFlags,
                    owner,
                    lobby.Metadata,
                    lobby.MaxMembers,
                    lobby.NumMembers,
                    members,
                    lobby.Distance,
                    lobby.Weight
                ) );
            }

            ConcurrentDictionary<SteamID, Lobby> GetAppLobbies( uint appId )
            {
                return lobbies.GetOrAdd( appId, k => new ConcurrentDictionary<SteamID, Lobby>() );
            }

            Lobby? DeleteLobby( uint appId, SteamID lobbySteamId )
            {
                if ( !lobbies.TryGetValue( appId, out var appLobbies ) )
                {
                    return null;
                }

                appLobbies.TryRemove( lobbySteamId, out var lobby );
                return lobby;
            }
        }
    }
}
