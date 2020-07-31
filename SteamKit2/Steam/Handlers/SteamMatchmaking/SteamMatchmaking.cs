using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for creating, joining and obtaining lobby information.
    /// </summary>
    public partial class SteamMatchmaking : ClientMsgHandler
    {
        readonly Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        readonly ConcurrentDictionary<JobID, ProtoBuf.IExtensible> lobbyManipulationRequests = new ConcurrentDictionary<JobID, ProtoBuf.IExtensible>();

        readonly LobbyCache lobbyCache = new LobbyCache();

        internal SteamMatchmaking()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientMMSCreateLobbyResponse, HandleCreateLobbyResponse },
                { EMsg.ClientMMSSetLobbyDataResponse, HandleSetLobbyDataResponse },
                { EMsg.ClientMMSSetLobbyOwnerResponse, HandleSetLobbyOwnerResponse },
                { EMsg.ClientMMSLobbyData, HandleLobbyData },
                { EMsg.ClientMMSGetLobbyListResponse, HandleGetLobbyListResponse },
                { EMsg.ClientMMSJoinLobbyResponse, HandleJoinLobbyResponse },
                { EMsg.ClientMMSLeaveLobbyResponse, HandleLeaveLobbyResponse },
                { EMsg.ClientMMSUserJoinedLobby, HandleUserJoinedLobby },
                { EMsg.ClientMMSUserLeftLobby, HandleUserLeftLobby },
            };
        }

        /// <summary>
        /// Sends a request to create a new lobby.
        /// </summary>
        /// <param name="appId">ID of the app the lobby will belong to.</param>
        /// <param name="lobbyType">The new lobby type.</param>
        /// <param name="maxMembers">The new maximum number of members that may occupy the lobby.</param>
        /// <param name="lobbyFlags">The new lobby flags. Defaults to 0.</param>
        /// <param name="metadata">The new metadata for the lobby. Defaults to <c>null</c> (treated as an empty dictionary).</param>
        /// <returns><c>null</c>, if the request could not be submitted i.e. not yet logged in. Otherwise, an <see cref="AsyncJob{CreateLobbyCallback}"/>.</returns>
        public AsyncJob<CreateLobbyCallback>? CreateLobby( uint appId, ELobbyType lobbyType, int maxMembers, int lobbyFlags = 0,
            IReadOnlyDictionary<string, string>? metadata = null )
        {
            if ( Client.CellID == null )
            {
                return null;
            }

            var personaName = Client.GetHandler<SteamFriends>()!.GetPersonaName();

            var createLobby = new ClientMsgProtobuf<CMsgClientMMSCreateLobby>( EMsg.ClientMMSCreateLobby )
            {
                Body =
                {
                    app_id = appId,
                    lobby_type = ( int )lobbyType,
                    max_members = maxMembers,
                    lobby_flags = lobbyFlags,
                    metadata = Lobby.EncodeMetadata( metadata ),
                    cell_id = Client.CellID.Value,
                    public_ip = NetHelpers.GetMsgIPAddress( Client.PublicIP! ),
                    persona_name_owner = personaName
                },
                SourceJobID = Client.GetNextJobID()
            };

            Send( createLobby, appId );

            lobbyManipulationRequests[ createLobby.SourceJobID ] = createLobby.Body;
            return AttachIncompleteManipulationHandler( new AsyncJob<CreateLobbyCallback>( Client, createLobby.SourceJobID ) );
        }

        /// <summary>
        /// Sends a request to update a lobby.
        /// </summary>
        /// <param name="appId">ID of app the lobby belongs to.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby that should be updated.</param>
        /// <param name="lobbyType">The new lobby type.</param>
        /// <param name="maxMembers">The new maximum number of members that may occupy the lobby.</param>
        /// <param name="lobbyFlags">The new lobby flags. Defaults to 0.</param>
        /// <param name="metadata">The new metadata for the lobby. Defaults to <c>null</c> (treated as an empty dictionary).</param>
        /// <returns>An <see cref="AsyncJob{SetLobbyDataCallback}"/>.</returns>
        public AsyncJob<SetLobbyDataCallback> SetLobbyData( uint appId, SteamID lobbySteamId, ELobbyType lobbyType, int maxMembers, int lobbyFlags = 0,
            IReadOnlyDictionary<string, string>? metadata = null )
        {
            var setLobbyData = new ClientMsgProtobuf<CMsgClientMMSSetLobbyData>( EMsg.ClientMMSSetLobbyData )
            {
                Body =
                {
                    app_id = appId,
                    steam_id_lobby = lobbySteamId,
                    steam_id_member = 0,
                    lobby_type = ( int )lobbyType,
                    max_members = maxMembers,
                    lobby_flags = lobbyFlags,
                    metadata = Lobby.EncodeMetadata( metadata ),
                },
                SourceJobID = Client.GetNextJobID()
            };

            Send( setLobbyData, appId );

            lobbyManipulationRequests[ setLobbyData.SourceJobID ] = setLobbyData.Body;
            return AttachIncompleteManipulationHandler( new AsyncJob<SetLobbyDataCallback>( Client, setLobbyData.SourceJobID ) );
        }

        /// <summary>
        /// Sends a request to update the current user's lobby metadata.
        /// </summary>
        /// <param name="appId">ID of app the lobby belongs to.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby that should be updated.</param>
        /// <param name="metadata">The new metadata for the lobby.</param>
        /// <returns><c>null</c>, if the request could not be submitted i.e. not yet logged in. Otherwise, an <see cref="AsyncJob{SetLobbyDataCallback}"/>.</returns>
        public AsyncJob<SetLobbyDataCallback>? SetLobbyMemberData( uint appId, SteamID lobbySteamId, IReadOnlyDictionary<string, string> metadata )
        {
            if ( Client.SteamID == null )
            {
                return null;
            }

            var setLobbyData = new ClientMsgProtobuf<CMsgClientMMSSetLobbyData>( EMsg.ClientMMSSetLobbyData )
            {
                Body =
                {
                    app_id = appId,
                    steam_id_lobby = lobbySteamId,
                    steam_id_member = Client.SteamID,
                    metadata = Lobby.EncodeMetadata( metadata )
                },
                SourceJobID = Client.GetNextJobID()
            };

            Send( setLobbyData, appId );

            lobbyManipulationRequests[ setLobbyData.SourceJobID ] = setLobbyData.Body;
            return AttachIncompleteManipulationHandler( new AsyncJob<SetLobbyDataCallback>( Client, setLobbyData.SourceJobID ) );
        }

        /// <summary>
        /// Sends a request to update the owner of a lobby.
        /// </summary>
        /// <param name="appId">ID of app the lobby belongs to.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby that should have its owner updated.</param>
        /// <param name="newOwner">The SteamID of the new owner.</param>
        /// <returns>An <see cref="AsyncJob{SetLobbyOwnerCallback}"/>.</returns>
        public AsyncJob<SetLobbyOwnerCallback> SetLobbyOwner( uint appId, SteamID lobbySteamId, SteamID newOwner )
        {
            var setLobbyOwner = new ClientMsgProtobuf<CMsgClientMMSSetLobbyOwner>( EMsg.ClientMMSSetLobbyOwner )
            {
                Body =
                {
                    app_id = appId,
                    steam_id_lobby = lobbySteamId,
                    steam_id_new_owner = newOwner
                },
                SourceJobID = Client.GetNextJobID()
            };

            Send( setLobbyOwner, appId );

            lobbyManipulationRequests[ setLobbyOwner.SourceJobID ] = setLobbyOwner.Body;
            return AttachIncompleteManipulationHandler( new AsyncJob<SetLobbyOwnerCallback>( Client, setLobbyOwner.SourceJobID ) );
        }

        /// <summary>
        /// Sends a request to obtains a list of lobbies matching the specified criteria.
        /// </summary>
        /// <param name="appId">The ID of app for which we're requesting a list of lobbies.</param>
        /// <param name="filters">An optional list of filters.</param>
        /// <param name="maxLobbies">An optional maximum number of lobbies that will be returned.</param>
        /// <returns><c>null</c>, if the request could not be submitted i.e. not yet logged in. Otherwise, an <see cref="AsyncJob{GetLobbyListCallback}"/>.</returns>
        public AsyncJob<GetLobbyListCallback>? GetLobbyList( uint appId, List<Lobby.Filter>? filters = null, int maxLobbies = -1 )
        {
            if ( Client.CellID == null )
            {
                return null;
            }

            var getLobbies = new ClientMsgProtobuf<CMsgClientMMSGetLobbyList>( EMsg.ClientMMSGetLobbyList )
            {
                Body =
                {
                    app_id = appId,
                    cell_id = Client.CellID.Value,
                    public_ip = NetHelpers.GetMsgIPAddress( Client.PublicIP! ),
                    num_lobbies_requested = maxLobbies
                },
                SourceJobID = Client.GetNextJobID()
            };

            if ( filters != null )
            {
                foreach ( var filter in filters )
                {
                    getLobbies.Body.filters.Add( filter.Serialize() );
                }
            }

            Send( getLobbies, appId );

            return new AsyncJob<GetLobbyListCallback>( Client, getLobbies.SourceJobID );
        }

        /// <summary>
        /// Sends a request to join a lobby.
        /// </summary>
        /// <param name="appId">ID of app the lobby belongs to.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby that should be joined.</param>
        /// <returns><c>null</c>, if the request could not be submitted i.e. not yet logged in. Otherwise, an <see cref="AsyncJob{JoinLobbyCallback}"/>.</returns>
        public AsyncJob<JoinLobbyCallback>? JoinLobby( uint appId, SteamID lobbySteamId )
        {
            var personaName = Client.GetHandler<SteamFriends>()?.GetPersonaName();

            if ( personaName == null )
            {
                return null;
            }

            var joinLobby = new ClientMsgProtobuf<CMsgClientMMSJoinLobby>( EMsg.ClientMMSJoinLobby )
            {
                Body =
                {
                    app_id = appId,
                    persona_name = personaName,
                    steam_id_lobby = lobbySteamId
                },
                SourceJobID = Client.GetNextJobID()
            };

            Send( joinLobby, appId );

            return new AsyncJob<JoinLobbyCallback>( Client, joinLobby.SourceJobID );
        }

        /// <summary>
        /// Sends a request to leave a lobby.
        /// </summary>
        /// <param name="appId">ID of app the lobby belongs to.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby that should be left.</param>
        /// <returns>An <see cref="AsyncJob{LeaveLobbyCallback}"/>.</returns>
        public AsyncJob<LeaveLobbyCallback> LeaveLobby( uint appId, SteamID lobbySteamId )
        {
            var leaveLobby = new ClientMsgProtobuf<CMsgClientMMSLeaveLobby>( EMsg.ClientMMSLeaveLobby )
            {
                Body =
                {
                    app_id = appId,
                    steam_id_lobby = lobbySteamId
                },
                SourceJobID = Client.GetNextJobID()
            };

            Send( leaveLobby, appId );

            return new AsyncJob<LeaveLobbyCallback>( Client, leaveLobby.SourceJobID );
        }

        /// <summary>
        /// Sends a request to obtain a lobby's data.
        /// </summary>
        /// <param name="appId">The ID of app which we're attempting to obtain lobby data for.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby whose data is being requested.</param>
        /// <returns>An <see cref="AsyncJob{LobbyDataCallback}"/>.</returns>
        public AsyncJob<LobbyDataCallback> GetLobbyData( uint appId, SteamID lobbySteamId )
        {
            var getLobbyData = new ClientMsgProtobuf<CMsgClientMMSGetLobbyData>( EMsg.ClientMMSGetLobbyData )
            {
                Body =
                {
                    app_id = appId,
                    steam_id_lobby = lobbySteamId
                },
                SourceJobID = Client.GetNextJobID()
            };

            Send( getLobbyData, appId );

            return new AsyncJob<LobbyDataCallback>( Client, getLobbyData.SourceJobID );
        }

        /// <summary>
        /// Sends a lobby invite request.
        /// NOTE: Steam provides no functionality to determine if the user was successfully invited.
        /// </summary>
        /// <param name="appId">The ID of app which owns the lobby we're inviting a user to.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby we're inviting a user to.</param>
        /// <param name="userSteamId">The SteamID of the user we're inviting.</param>
        public void InviteToLobby( uint appId, SteamID lobbySteamId, SteamID userSteamId )
        {
            var getLobbyData = new ClientMsgProtobuf<CMsgClientMMSInviteToLobby>( EMsg.ClientMMSInviteToLobby )
            {
                Body =
                {
                    app_id = appId,
                    steam_id_lobby = lobbySteamId,
                    steam_id_user_invited = userSteamId
                }
            };

            Send( getLobbyData, appId );
        }

        /// <summary>
        /// Obtains a <see cref="Lobby"/>, by its SteamID, if the data is cached locally.
        /// This method does not send a network request.
        /// </summary>
        /// <param name="appId">The ID of app which we're attempting to obtain a lobby for.</param>
        /// <param name="lobbySteamId">The SteamID of the lobby that should be returned.</param>
        /// <returns>The <see cref="Lobby"/> corresponding with the specified app and lobby ID, if cached. Otherwise, <c>null</c>.</returns>
        public Lobby? GetLobby( uint appId, SteamID lobbySteamId )
        {
            return lobbyCache.GetLobby( appId, lobbySteamId );
        }

        /// <summary>
        /// Sends a matchmaking message for a specific app.
        /// </summary>
        /// <param name="msg">The matchmaking message to send.</param>
        /// <param name="appId">The ID of the app this message pertains to.</param>
        public void Send( ClientMsgProtobuf msg, uint appId )
        {
            if ( msg == null )
            {
                throw new ArgumentNullException( nameof(msg) );
            }

            msg.ProtoHeader.routing_appid = appId;
            Client.Send( msg );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                throw new ArgumentNullException( nameof(packetMsg) );
            }

            if ( dispatchMap.TryGetValue( packetMsg.MsgType, out var handler ) )
            {
                handler( packetMsg );
            }
        }

        internal void ClearLobbyCache()
        {
            lobbyCache.Clear();
        }

        AsyncJob<T> AttachIncompleteManipulationHandler<T>( AsyncJob<T> job )
            where T : CallbackMsg
        {
            // Manipulation requests typically complete (and are removed from lobbyManipulationRequests) when
            // a message is handled. However, jobs can also be faulted, or be cancelled (e.g. when SteamClient
            // disconnects.) Thus, when a job fails we remove the JobID/request from lobbyManipulationRequests.
            job.ToTask().ContinueWith( task =>
            {
                lobbyManipulationRequests.TryRemove( job.JobID, out _ );
            }, TaskContinuationOptions.NotOnRanToCompletion );
            return job;
        }

        #region ClientMsg Handlers

        void HandleCreateLobbyResponse( IPacketMsg packetMsg )
        {
            var createLobbyResponse = new ClientMsgProtobuf<CMsgClientMMSCreateLobbyResponse>( packetMsg );
            var body = createLobbyResponse.Body;

            if ( lobbyManipulationRequests.TryRemove( createLobbyResponse.TargetJobID, out var request ) )
            {
                if ( body.eresult == ( int )EResult.OK && request != null )
                {
                    var createLobby = ( CMsgClientMMSCreateLobby )request;
                    var members = new List<Lobby.Member>( 1 ) { new Lobby.Member( Client.SteamID!, createLobby.persona_name_owner ) };

                    lobbyCache.CacheLobby(
                        createLobby.app_id,
                        new Lobby(
                            body.steam_id_lobby,
                            ( ELobbyType )createLobby.lobby_type,
                            createLobby.lobby_flags,
                            Client.SteamID,
                            Lobby.DecodeMetadata( createLobby.metadata ),
                            createLobby.max_members,
                            1,
                            members.AsReadOnly(),
                            null,
                            null
                        )
                    );
                }
            }

            Client.PostCallback( new CreateLobbyCallback(
                createLobbyResponse.TargetJobID,
                body.app_id,
                ( EResult )body.eresult,
                body.steam_id_lobby
            ) );
        }

        void HandleSetLobbyDataResponse( IPacketMsg packetMsg )
        {
            var setLobbyDataResponse = new ClientMsgProtobuf<CMsgClientMMSSetLobbyDataResponse>( packetMsg );
            var body = setLobbyDataResponse.Body;

            if ( lobbyManipulationRequests.TryRemove( setLobbyDataResponse.TargetJobID, out var request ) )
            {
                if ( body.eresult == ( int )EResult.OK && request != null )
                {
                    var setLobbyData = ( CMsgClientMMSSetLobbyData )request;
                    var lobby = lobbyCache.GetLobby( setLobbyData.app_id, setLobbyData.steam_id_lobby );

                    if ( lobby != null )
                    {
                        var metadata = Lobby.DecodeMetadata( setLobbyData.metadata );

                        if ( setLobbyData.steam_id_member == 0 )
                        {
                            lobbyCache.CacheLobby(
                                setLobbyData.app_id,
                                new Lobby(
                                    lobby.SteamID,
                                    ( ELobbyType )setLobbyData.lobby_type,
                                    setLobbyData.lobby_flags,
                                    lobby.OwnerSteamID,
                                    metadata,
                                    setLobbyData.max_members,
                                    lobby.NumMembers,
                                    lobby.Members,
                                    lobby.Distance,
                                    lobby.Weight
                                )
                            );
                        }
                        else
                        {
                            var members = lobby.Members.Select( m =>
                                m.SteamID == setLobbyData.steam_id_member ? new Lobby.Member( m.SteamID, m.PersonaName, metadata ) : m
                            ).ToList();

                            lobbyCache.UpdateLobbyMembers( setLobbyData.app_id, lobby, members );
                        }
                    }
                }
            }

            Client.PostCallback( new SetLobbyDataCallback(
                setLobbyDataResponse.TargetJobID,
                body.app_id,
                ( EResult )body.eresult,
                body.steam_id_lobby
            ) );
        }

        void HandleSetLobbyOwnerResponse( IPacketMsg packetMsg )
        {
            var setLobbyOwnerResponse = new ClientMsgProtobuf<CMsgClientMMSSetLobbyOwnerResponse>( packetMsg );
            var body = setLobbyOwnerResponse.Body;

            if ( lobbyManipulationRequests.TryRemove( setLobbyOwnerResponse.TargetJobID, out var request ) )
            {
                if ( body.eresult == ( int )EResult.OK && request != null )
                {
                    var setLobbyOwner = ( CMsgClientMMSSetLobbyOwner )request;
                    lobbyCache.UpdateLobbyOwner( body.app_id, body.steam_id_lobby, setLobbyOwner.steam_id_new_owner );
                }
            }

            Client.PostCallback( new SetLobbyOwnerCallback(
                setLobbyOwnerResponse.TargetJobID,
                body.app_id,
                ( EResult )body.eresult,
                body.steam_id_lobby
            ) );
        }

        void HandleGetLobbyListResponse( IPacketMsg packetMsg )
        {
            var lobbyListResponse = new ClientMsgProtobuf<CMsgClientMMSGetLobbyListResponse>( packetMsg );
            var body = lobbyListResponse.Body;

            var lobbyList =
                body.lobbies.ConvertAll( lobby =>
                {
                    var existingLobby = lobbyCache.GetLobby( body.app_id, lobby.steam_id );
                    var members = existingLobby?.Members;

                    return new Lobby(
                        lobby.steam_id,
                        ( ELobbyType )lobby.lobby_type,
                        lobby.lobby_flags,
                        existingLobby?.OwnerSteamID,
                        Lobby.DecodeMetadata( lobby.metadata ),
                        lobby.max_members,
                        lobby.num_members,
                        members,
                        lobby.distance,
                        lobby.weight
                    );
                } );

            foreach ( var lobby in lobbyList )
            {
                lobbyCache.CacheLobby( body.app_id, lobby );
            }

            Client.PostCallback( new GetLobbyListCallback(
                lobbyListResponse.TargetJobID,
                body.app_id,
                ( EResult )body.eresult,
                lobbyList
            ) );
        }

        void HandleJoinLobbyResponse( IPacketMsg packetMsg )
        {
            var joinLobbyResponse = new ClientMsgProtobuf<CMsgClientMMSJoinLobbyResponse>( packetMsg );
            var body = joinLobbyResponse.Body;

            Lobby? joinedLobby = null;

            if ( body.ShouldSerializesteam_id_lobby() )
            {
                var members =
                    body.members.ConvertAll( member => new Lobby.Member(
                        member.steam_id,
                        member.persona_name,
                        Lobby.DecodeMetadata( member.metadata )
                    ) );

                var cachedLobby = lobbyCache.GetLobby( body.app_id, body.steam_id_lobby );

                joinedLobby = new Lobby(
                    body.steam_id_lobby,
                    ( ELobbyType )body.lobby_type,
                    body.lobby_flags,
                    body.steam_id_owner,
                    Lobby.DecodeMetadata( body.metadata ),
                    body.max_members,
                    members.Count,
                    members,
                    cachedLobby?.Distance,
                    cachedLobby?.Weight
                );

                lobbyCache.CacheLobby( body.app_id, joinedLobby );
            }

            Client.PostCallback( new JoinLobbyCallback(
                joinLobbyResponse.TargetJobID,
                body.app_id,
                ( EChatRoomEnterResponse )body.chat_room_enter_response,
                joinedLobby
            ) );
        }

        void HandleLeaveLobbyResponse( IPacketMsg packetMsg )
        {
            var leaveLobbyResponse = new ClientMsgProtobuf<CMsgClientMMSLeaveLobbyResponse>( packetMsg );
            var body = leaveLobbyResponse.Body;

            if ( body.eresult == ( int )EResult.OK )
            {
                lobbyCache.ClearLobbyMembers( body.app_id, body.steam_id_lobby );
            }

            Client.PostCallback( new LeaveLobbyCallback(
                leaveLobbyResponse.TargetJobID,
                body.app_id,
                ( EResult )body.eresult,
                body.steam_id_lobby
            ) );
        }

        void HandleLobbyData( IPacketMsg packetMsg )
        {
            var lobbyDataResponse = new ClientMsgProtobuf<CMsgClientMMSLobbyData>( packetMsg );

            var body = lobbyDataResponse.Body;

            var cachedLobby = lobbyCache.GetLobby( body.app_id, body.steam_id_lobby );
            var members = body.members.Count == 0
                ? cachedLobby?.Members
                : body.members.ConvertAll( member => new Lobby.Member(
                    member.steam_id,
                    member.persona_name,
                    Lobby.DecodeMetadata( member.metadata )
                ) );

            var updatedLobby = new Lobby(
                body.steam_id_lobby,
                ( ELobbyType )body.lobby_type,
                body.lobby_flags,
                body.steam_id_owner,
                Lobby.DecodeMetadata( body.metadata ),
                body.max_members,
                body.num_members,
                members,
                cachedLobby?.Distance,
                cachedLobby?.Weight
            );

            lobbyCache.CacheLobby( body.app_id, updatedLobby );

            Client.PostCallback( new LobbyDataCallback(
                lobbyDataResponse.TargetJobID,
                body.app_id,
                updatedLobby
            ) );
        }

        void HandleUserJoinedLobby( IPacketMsg packetMsg )
        {
            var userJoinedLobby = new ClientMsgProtobuf<CMsgClientMMSUserJoinedLobby>( packetMsg );
            var body = userJoinedLobby.Body;

            var lobby = lobbyCache.GetLobby( body.app_id, body.steam_id_lobby );

            if ( lobby != null && lobby.Members.Count > 0 )
            {
                var joiningMember = lobbyCache.AddLobbyMember( body.app_id, lobby, body.steam_id_user, body.persona_name );

                if ( joiningMember != null )
                {
                    Client.PostCallback( new UserJoinedLobbyCallback(
                        body.app_id,
                        body.steam_id_lobby,
                        joiningMember
                    ) );
                }
            }
        }

        void HandleUserLeftLobby( IPacketMsg packetMsg )
        {
            var userLeftLobby = new ClientMsgProtobuf<CMsgClientMMSUserLeftLobby>( packetMsg );
            var body = userLeftLobby.Body;

            var lobby = lobbyCache.GetLobby( body.app_id, body.steam_id_lobby );

            if ( lobby != null && lobby.Members.Count > 0 )
            {
                var leavingMember = lobbyCache.RemoveLobbyMember( body.app_id, lobby, body.steam_id_user );
                if ( leavingMember is null )
                {
                    return;
                }

                if ( leavingMember.SteamID == Client.SteamID )
                {
                    lobbyCache.ClearLobbyMembers( body.app_id, body.steam_id_lobby );
                }

                Client.PostCallback( new UserLeftLobbyCallback(
                    body.app_id,
                    body.steam_id_lobby,
                    leavingMember
                ) );
            }
        }

        #endregion
    }
}
