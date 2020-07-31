/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all interaction with other users on the Steam3 network.
    /// </summary>
    public sealed partial class SteamFriends : ClientMsgHandler
    {
        object listLock = new object();
        List<SteamID> friendList;
        List<SteamID> clanList;

        AccountCache cache;

        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamFriends()
        {
            friendList = new List<SteamID>();
            clanList = new List<SteamID>();

            cache = new AccountCache();

            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientPersonaState, HandlePersonaState },
                { EMsg.ClientClanState, HandleClanState },
                { EMsg.ClientFriendsList, HandleFriendsList },
                { EMsg.ClientFriendMsgIncoming, HandleFriendMsg },
                { EMsg.ClientFriendMsgEchoToSender, HandleFriendEchoMsg },
                { EMsg.ClientChatGetFriendMessageHistoryResponse, HandleFriendMessageHistoryResponse },
                { EMsg.ClientAccountInfo, HandleAccountInfo },
                { EMsg.ClientAddFriendResponse, HandleFriendResponse },
                { EMsg.ClientChatEnter, HandleChatEnter },
                { EMsg.ClientChatMsg, HandleChatMsg },
                { EMsg.ClientChatMemberInfo, HandleChatMemberInfo },
                { EMsg.ClientChatRoomInfo, HandleChatRoomInfo },
                { EMsg.ClientChatActionResult, HandleChatActionResult },
                { EMsg.ClientChatInvite, HandleChatInvite },
                { EMsg.ClientSetIgnoreFriendResponse, HandleIgnoreFriendResponse },
                { EMsg.ClientFriendProfileInfoResponse, HandleProfileInfoResponse },
                { EMsg.ClientPersonaChangeResponse, HandlePersonaChangeResponse },
            };
        }


        /// <summary>
        /// Gets the local user's persona name. Will be null before user initialization.
        /// User initialization is performed prior to <see cref="SteamUser.AccountInfoCallback"/> callback.
        /// </summary>
        /// <returns>The name.</returns>
        public string? GetPersonaName()
        {
            return cache.LocalUser.Name;
        }
        /// <summary>
        /// Sets the local user's persona name and broadcasts it over the network.
        /// </summary>
        /// <param name="name">The name.</param>
        public void SetPersonaName( string name )
        {
            // cache the local name right away, so that early calls to SetPersonaState don't reset the set name
            cache.LocalUser.Name = name ?? throw new ArgumentNullException( nameof(name) );

            var stateMsg = new ClientMsgProtobuf<CMsgClientChangeStatus>( EMsg.ClientChangeStatus );

            stateMsg.Body.persona_state = ( uint )cache.LocalUser.PersonaState;
            stateMsg.Body.player_name = name;

            this.Client.Send( stateMsg );
        }

        /// <summary>
        /// Gets the local user's persona state.
        /// </summary>
        /// <returns>The persona state.</returns>
        public EPersonaState GetPersonaState()
        {
            return cache.LocalUser.PersonaState;
        }
        /// <summary>
        /// Sets the local user's persona state and broadcasts it over the network.
        /// </summary>
        /// <param name="state">The state.</param>
        public void SetPersonaState( EPersonaState state )
        {
            cache.LocalUser.PersonaState = state;

            var stateMsg = new ClientMsgProtobuf<CMsgClientChangeStatus>( EMsg.ClientChangeStatus );

            stateMsg.Body.persona_state = ( uint )state;

            this.Client.Send( stateMsg );
        }

        /// <summary>
        /// Gets the friend count of the local user.
        /// </summary>
        /// <returns>The number of friends.</returns>
        public int GetFriendCount()
        {
            lock ( listLock )
            {
                return friendList.Count;
            }
        }
        /// <summary>
        /// Gets a friend by index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>A valid steamid of a friend if the index is in range; otherwise a steamid representing 0.</returns>
        public SteamID GetFriendByIndex( int index )
        {
            lock ( listLock )
            {
                if ( index < 0 || index >= friendList.Count )
                    return 0;

                return friendList[ index ];
            }
        }

        /// <summary>
        /// Gets the persona name of a friend.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The name.</returns>
        public string? GetFriendPersonaName( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.GetUser( steamId ).Name;
        }
        /// <summary>
        /// Gets the persona state of a friend.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The persona state.</returns>
        public EPersonaState GetFriendPersonaState( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.GetUser( steamId ).PersonaState;
        }
        /// <summary>
        /// Gets the relationship of a friend.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The relationship of the friend to the local user.</returns>
        public EFriendRelationship GetFriendRelationship( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.GetUser( steamId ).Relationship;
        }
        /// <summary>
        /// Gets the game name of a friend playing a game.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The game name of a friend playing a game, or null if they haven't been cached yet.</returns>
        public string? GetFriendGamePlayedName( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.GetUser( steamId ).GameName;
        }
        /// <summary>
        /// Gets the GameID of a friend playing a game.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The gameid of a friend playing a game, or 0 if they haven't been cached yet.</returns>
        public GameID GetFriendGamePlayed( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.GetUser( steamId ).GameID;
        }
        /// <summary>
        /// Gets a SHA-1 hash representing the friend's avatar.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to get the avatar of.</param>
        /// <returns>A byte array representing a SHA-1 hash of the friend's avatar.</returns>
        public byte[]? GetFriendAvatar( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.GetUser( steamId ).AvatarHash;
        }

        /// <summary>
        /// Gets the count of clans the local user is a member of.
        /// </summary>
        /// <returns>The number of clans this user is a member of.</returns>
        public int GetClanCount()
        {
            lock ( listLock )
            {
                return clanList.Count;
            }
        }
        /// <summary>
        /// Gets a clan SteamID by index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>A valid steamid of a clan if the index is in range; otherwise a steamid representing 0.</returns>
        public SteamID GetClanByIndex( int index )
        {
            lock ( listLock )
            {
                if ( index < 0 || index >= clanList.Count )
                    return 0;

                return clanList[ index ];
            }
        }

        /// <summary>
        /// Gets the name of a clan.
        /// </summary>
        /// <param name="steamId">The clan SteamID.</param>
        /// <returns>The name.</returns>
        public string? GetClanName( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.Clans.GetAccount( steamId ).Name;
        }
        /// <summary>
        /// Gets the relationship of a clan.
        /// </summary>
        /// <param name="steamId">The clan steamid.</param>
        /// <returns>The relationship of the clan to the local user.</returns>
        public EClanRelationship GetClanRelationship( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.Clans.GetAccount( steamId ).Relationship;
        }
        /// <summary>
        /// Gets a SHA-1 hash representing the clan's avatar.
        /// </summary>
        /// <param name="steamId">The SteamID of the clan to get the avatar of.</param>
        /// <returns>A byte array representing a SHA-1 hash of the clan's avatar, or null if the clan could not be found.</returns>
        public byte[]? GetClanAvatar( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            return cache.Clans.GetAccount( steamId ).AvatarHash;
        }

        /// <summary>
        /// Sends a chat message to a friend.
        /// </summary>
        /// <param name="target">The target to send to.</param>
        /// <param name="type">The type of message to send.</param>
        /// <param name="message">The message to send.</param>
        public void SendChatMessage( SteamID target, EChatEntryType type, string message )
        {
            if ( target == null )
            {
                throw new ArgumentNullException( nameof(target) );
            }

            if ( message == null )
            {
                throw new ArgumentNullException( nameof(message) );
            }

            var chatMsg = new ClientMsgProtobuf<CMsgClientFriendMsg>( EMsg.ClientFriendMsg );

            chatMsg.Body.steamid = target;
            chatMsg.Body.chat_entry_type = ( int )type;
            chatMsg.Body.message = Encoding.UTF8.GetBytes( message );

            this.Client.Send( chatMsg );
        }

        /// <summary>
        /// Sends a friend request to a user.
        /// </summary>
        /// <param name="accountNameOrEmail">The account name or email of the user.</param>
        public void AddFriend( string accountNameOrEmail )
        {
            if ( accountNameOrEmail == null )
            {
                throw new ArgumentNullException( nameof(accountNameOrEmail) );
            }

            var addFriend = new ClientMsgProtobuf<CMsgClientAddFriend>( EMsg.ClientAddFriend );

            addFriend.Body.accountname_or_email_to_add = accountNameOrEmail;

            this.Client.Send( addFriend );
        }
        /// <summary>
        /// Sends a friend request to a user.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to add.</param>
        public void AddFriend( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            var addFriend = new ClientMsgProtobuf<CMsgClientAddFriend>( EMsg.ClientAddFriend );

            addFriend.Body.steamid_to_add = steamId;

            this.Client.Send( addFriend );
        }
        /// <summary>
        /// Removes a friend from your friends list.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to remove.</param>
        public void RemoveFriend( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            var removeFriend = new ClientMsgProtobuf<CMsgClientRemoveFriend>( EMsg.ClientRemoveFriend );

            removeFriend.Body.friendid = steamId;

            this.Client.Send( removeFriend );
        }


        /// <summary>
        /// Attempts to join a chat room.
        /// </summary>
        /// <param name="steamId">The SteamID of the chat room.</param>
        public void JoinChat( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            SteamID chatId = steamId.ConvertToUInt64(); // copy the steamid so we don't modify it

            var joinChat = new ClientMsg<MsgClientJoinChat>();

            if ( chatId.IsClanAccount )
            {
                chatId = chatId.ToChatID();
            }

            joinChat.Body.SteamIdChat = chatId;

            Client.Send( joinChat );
        }

        /// <summary>
        /// Attempts to leave a chat room.
        /// </summary>
        /// <param name="steamId">The SteamID of the chat room.</param>
        public void LeaveChat( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            SteamID chatId = steamId.ConvertToUInt64(); // copy the steamid so we don't modify it

            var leaveChat = new ClientMsg<MsgClientChatMemberInfo>();

            if ( chatId.IsClanAccount )
            {
                // this steamid is incorrect, so we'll fix it up
                chatId.AccountInstance = ( uint )SteamID.ChatInstanceFlags.Clan;
                chatId.AccountType = EAccountType.Chat;
            }

            leaveChat.Body.SteamIdChat = chatId;
            leaveChat.Body.Type = EChatInfoType.StateChange;

            var localSteamID = Client.SteamID?.ConvertToUInt64() ?? default; // SteamID can be null if not connected - will be ultimately ignored in Client.Send.

            leaveChat.Write( localSteamID ); // ChatterActedOn
            leaveChat.Write( ( uint )EChatMemberStateChange.Left ); // StateChange
            leaveChat.Write( localSteamID ); // ChatterActedBy

            Client.Send( leaveChat );
        }

        /// <summary>
        /// Sends a message to a chat room.
        /// </summary>
        /// <param name="steamIdChat">The SteamID of the chat room.</param>
        /// <param name="type">The message type.</param>
        /// <param name="message">The message.</param>
        public void SendChatRoomMessage( SteamID steamIdChat, EChatEntryType type, string message )
        {
            if ( steamIdChat == null )
            {
                throw new ArgumentNullException( nameof(steamIdChat) );
            }

            if ( message == null )
            {
                throw new ArgumentNullException( nameof(message) );
            }

            SteamID chatId = steamIdChat.ConvertToUInt64(); // copy the steamid so we don't modify it

            if ( chatId.IsClanAccount )
            {
                // this steamid is incorrect, so we'll fix it up
                chatId.AccountInstance = ( uint )SteamID.ChatInstanceFlags.Clan;
                chatId.AccountType = EAccountType.Chat;
            }

            var chatMsg = new ClientMsg<MsgClientChatMsg>();

            chatMsg.Body.ChatMsgType = type;
            chatMsg.Body.SteamIdChatRoom = chatId;
            chatMsg.Body.SteamIdChatter = Client.SteamID ?? new SteamID(); // Can be null if not connected - will ultimately be ignored in Client.Send.

            chatMsg.WriteNullTermString( message, Encoding.UTF8 );

            this.Client.Send( chatMsg );
        }

        /// <summary>
        /// Invites a user to a chat room.
        /// The results of this action will be available through the <see cref="ChatActionResultCallback"/> callback.
        /// </summary>
        /// <param name="steamIdUser">The SteamID of the user to invite.</param>
        /// <param name="steamIdChat">The SteamID of the chat room to invite the user to.</param>
        public void InviteUserToChat( SteamID steamIdUser, SteamID steamIdChat )
        {
            if ( steamIdUser == null )
            {
                throw new ArgumentNullException( nameof(steamIdUser) );
            }

            if ( steamIdChat == null )
            {
                throw new ArgumentNullException( nameof(steamIdChat) );
            }

            SteamID chatId = steamIdChat.ConvertToUInt64(); // copy the steamid so we don't modify it

            if ( chatId.IsClanAccount )
            {
                // this steamid is incorrect, so we'll fix it up
                chatId.AccountInstance = (uint)SteamID.ChatInstanceFlags.Clan;
                chatId.AccountType = EAccountType.Chat;
            }

            var inviteMsg = new ClientMsgProtobuf<CMsgClientChatInvite>( EMsg.ClientChatInvite );

            inviteMsg.Body.steam_id_chat = chatId;
            inviteMsg.Body.steam_id_invited = steamIdUser;
            // steamclient also sends the steamid of the user that did the invitation
            // we'll mimic that behavior
            inviteMsg.Body.steam_id_patron = Client.SteamID ?? new SteamID();

            this.Client.Send( inviteMsg );
        }

        /// <summary>
        /// Kicks the specified chat member from the given chat room.
        /// </summary>
        /// <param name="steamIdChat">The SteamID of chat room to kick the member from.</param>
        /// <param name="steamIdMember">The SteamID of the member to kick from the chat.</param>
        public void KickChatMember( SteamID steamIdChat, SteamID steamIdMember )
        {
            if ( steamIdChat == null )
            {
                throw new ArgumentNullException( nameof(steamIdChat) );
            }

            if ( steamIdMember == null )
            {
                throw new ArgumentNullException( nameof(steamIdMember) );
            }

            SteamID chatId = steamIdChat.ConvertToUInt64(); // copy the steamid so we don't modify it

            var kickMember = new ClientMsg<MsgClientChatAction>();

            if ( chatId.IsClanAccount )
            {
                // this steamid is incorrect, so we'll fix it up
                chatId.AccountInstance = ( uint )SteamID.ChatInstanceFlags.Clan;
                chatId.AccountType = EAccountType.Chat;
            }

            kickMember.Body.SteamIdChat = chatId;
            kickMember.Body.SteamIdUserToActOn = steamIdMember;

            kickMember.Body.ChatAction = EChatAction.Kick;

            this.Client.Send( kickMember );
        }

        /// <summary>
        /// Bans the specified chat member from the given chat room.
        /// </summary>
        /// <param name="steamIdChat">The SteamID of chat room to ban the member from.</param>
        /// <param name="steamIdMember">The SteamID of the member to ban from the chat.</param>
        public void BanChatMember( SteamID steamIdChat, SteamID steamIdMember )
        {
            if ( steamIdChat == null )
            {
                throw new ArgumentNullException( nameof(steamIdChat) );
            }

            if ( steamIdMember == null )
            {
                throw new ArgumentNullException( nameof(steamIdMember) );
            }

            SteamID chatId = steamIdChat.ConvertToUInt64(); // copy the steamid so we don't modify it

            var banMember = new ClientMsg<MsgClientChatAction>();

            if ( chatId.IsClanAccount )
            {
                // this steamid is incorrect, so we'll fix it up
                chatId.AccountInstance = ( uint )SteamID.ChatInstanceFlags.Clan;
                chatId.AccountType = EAccountType.Chat;
            }

            banMember.Body.SteamIdChat = chatId;
            banMember.Body.SteamIdUserToActOn = steamIdMember;

            banMember.Body.ChatAction = EChatAction.Ban;

            this.Client.Send( banMember );
        }
        /// <summary>
        /// Unbans the specified SteamID from the given chat room.
        /// </summary>
        /// <param name="steamIdChat">The SteamID of chat room to unban the member from.</param>
        /// <param name="steamIdMember">The SteamID of the member to unban from the chat.</param>
        public void UnbanChatMember( SteamID steamIdChat, SteamID steamIdMember )
        {
            if ( steamIdChat == null )
            {
                throw new ArgumentNullException( nameof(steamIdChat) );
            }

            if ( steamIdMember == null )
            {
                throw new ArgumentNullException( nameof(steamIdMember) );
            }

            SteamID chatId = steamIdChat.ConvertToUInt64(); // copy the steamid so we don't modify it

            var unbanMember = new ClientMsg<MsgClientChatAction>();

            if ( chatId.IsClanAccount )
            {
                // this steamid is incorrect, so we'll fix it up
                chatId.AccountInstance = ( uint )SteamID.ChatInstanceFlags.Clan;
                chatId.AccountType = EAccountType.Chat;
            }

            unbanMember.Body.SteamIdChat = chatId;
            unbanMember.Body.SteamIdUserToActOn = steamIdMember;

            unbanMember.Body.ChatAction = EChatAction.UnBan;

            this.Client.Send( unbanMember );
        }

        /// <summary>
        /// Requests persona state for a list of specified SteamID.
        /// Results are returned in <see cref="SteamFriends.PersonaStateCallback"/>.
        /// </summary>
        /// <param name="steamIdList">A list of SteamIDs to request the info of.</param>
        /// <param name="requestedInfo">The requested info flags. If none specified, this uses <see cref="SteamConfiguration.DefaultPersonaStateFlags"/>.</param>
        public void RequestFriendInfo( IEnumerable<SteamID> steamIdList, EClientPersonaStateFlag requestedInfo = default(EClientPersonaStateFlag) )
        {
            if ( steamIdList == null )
            {
                throw new ArgumentNullException( nameof(steamIdList) );
            }

            if ( requestedInfo == default(EClientPersonaStateFlag) )
            {
                requestedInfo = Client.Configuration.DefaultPersonaStateFlags;
            }

            var request = new ClientMsgProtobuf<CMsgClientRequestFriendData>( EMsg.ClientRequestFriendData );

            request.Body.friends.AddRange( steamIdList.Select( sID => sID.ConvertToUInt64() ) );
            request.Body.persona_state_requested = ( uint )requestedInfo;

            this.Client.Send( request );
        }
        /// <summary>
        /// Requests persona state for a specified SteamID.
        /// Results are returned in <see cref="SteamFriends.PersonaStateCallback"/>.
        /// </summary>
        /// <param name="steamId">A SteamID to request the info of.</param>
        /// <param name="requestedInfo">The requested info flags. If none specified, this uses <see cref="SteamConfiguration.DefaultPersonaStateFlags"/>.</param>
        public void RequestFriendInfo( SteamID steamId, EClientPersonaStateFlag requestedInfo = 0 )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            RequestFriendInfo( new SteamID[] { steamId }, requestedInfo );
        }

        /// <summary>
        /// Ignores or unignores a friend on Steam.
        /// Results are returned in a <see cref="IgnoreFriendCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to ignore or unignore.</param>
        /// <param name="setIgnore">if set to <c>true</c>, the friend will be ignored; otherwise, they will be unignored.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="IgnoreFriendCallback"/>.</returns>
        public AsyncJob<IgnoreFriendCallback> IgnoreFriend( SteamID steamId, bool setIgnore = true )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            var ignore = new ClientMsg<MsgClientSetIgnoreFriend>();
            ignore.SourceJobID = Client.GetNextJobID();

            ignore.Body.MySteamId = Client.SteamID ?? new SteamID(); // Can be null if not connected - will ultimately be ignored in Client.Send.
            ignore.Body.Ignore = ( byte )( setIgnore ? 1 : 0 );
            ignore.Body.SteamIdFriend = steamId;

            this.Client.Send( ignore );

            return new AsyncJob<IgnoreFriendCallback>( this.Client, ignore.SourceJobID );
        }


        /// <summary>
        /// Requests profile information for the given <see cref="SteamID"/>.
        /// Results are returned in a <see cref="ProfileInfoCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to request the details of.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="ProfileInfoCallback"/>.</returns>
        public AsyncJob<ProfileInfoCallback> RequestProfileInfo( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            var request = new ClientMsgProtobuf<CMsgClientFriendProfileInfo>( EMsg.ClientFriendProfileInfo );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.steamid_friend = steamId;

            this.Client.Send( request );

            return new AsyncJob<ProfileInfoCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Requests the last few chat messages with a friend.
        /// Results are returned in a <see cref="FriendMsgHistoryCallback"/>
        /// </summary>
        /// <param name="steamId">SteamID of the friend</param>
        public void RequestMessageHistory( SteamID steamId )
        {
            if ( steamId == null )
            {
                throw new ArgumentNullException( nameof(steamId) );
            }

            var request = new ClientMsgProtobuf<CMsgClientChatGetFriendMessageHistory>( EMsg.ClientChatGetFriendMessageHistory );

            request.Body.steamid = steamId;

            this.Client.Send(request);
        }

        /// <summary>
        /// Requests all offline messages.
        /// This also marks them as read server side.
        /// Results are returned in a <see cref="FriendMsgHistoryCallback"/>.
        /// </summary>
        public void RequestOfflineMessages()
        {
            var request = new ClientMsgProtobuf<CMsgClientChatGetFriendMessageHistoryForOfflineMessages>( EMsg.ClientChatGetFriendMessageHistoryForOfflineMessages );
            
            this.Client.Send( request );
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

            bool haveFunc = dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc );

            if ( !haveFunc )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleAccountInfo( IPacketMsg packetMsg )
        {
            var accInfo = new ClientMsgProtobuf<CMsgClientAccountInfo>( packetMsg );

            // cache off our local name
            cache.LocalUser.Name = accInfo.Body.persona_name;
        }
        void HandleFriendMsg( IPacketMsg packetMsg )
        {
            var friendMsg = new ClientMsgProtobuf<CMsgClientFriendMsgIncoming>( packetMsg );

            var callback = new FriendMsgCallback( friendMsg.Body );
            this.Client.PostCallback( callback );
        }

        void HandleFriendEchoMsg( IPacketMsg packetMsg )
        {
            var friendEchoMsg = new ClientMsgProtobuf<CMsgClientFriendMsgIncoming>( packetMsg );

            var callback = new FriendMsgEchoCallback( friendEchoMsg.Body );
            this.Client.PostCallback( callback );
        }

        private void HandleFriendMessageHistoryResponse( IPacketMsg packetMsg )
        {
            var historyResponse = new ClientMsgProtobuf<CMsgClientChatGetFriendMessageHistoryResponse>( packetMsg );

            var callback = new FriendMsgHistoryCallback( historyResponse.Body, this.Client.Universe );
            this.Client.PostCallback( callback );
        }

        void HandleFriendsList( IPacketMsg packetMsg )
        {
            var list = new ClientMsgProtobuf<CMsgClientFriendsList>( packetMsg );

            cache.LocalUser.SteamID = this.Client.SteamID!;

            if ( !list.Body.bincremental )
            {
                // if we're not an incremental update, the message contains all friends, so we should clear our current list
                lock ( listLock )
                {
                    friendList.Clear();
                    clanList.Clear();
                }
            }

            // we have to request information for all of our friends because steam only sends persona information for online friends
            var reqInfo = new ClientMsgProtobuf<CMsgClientRequestFriendData>( EMsg.ClientRequestFriendData );

            reqInfo.Body.persona_state_requested = ( uint )Client.Configuration.DefaultPersonaStateFlags;

            lock ( listLock )
            {
                List<SteamID> friendsToRemove = new List<SteamID>();
                List<SteamID> clansToRemove = new List<SteamID>();

                foreach ( var friendObj in list.Body.friends )
                {
                    SteamID friendId = friendObj.ulfriendid;

                    if ( friendId.IsIndividualAccount )
                    {
                        var user = cache.GetUser( friendId );

                        user.Relationship = ( EFriendRelationship )friendObj.efriendrelationship;

                        if ( friendList.Contains( friendId ) )
                        {
                            // if this is a friend on our list, and they removed us, mark them for removal
                            if ( user.Relationship == EFriendRelationship.None )
                                friendsToRemove.Add( friendId );
                        }
                        else
                        {
                            // we don't know about this friend yet, lets add them
                            friendList.Add( friendId );
                        }

                    }
                    else if ( friendId.IsClanAccount )
                    {
                        var clan = cache.Clans.GetAccount( friendId );

                        clan.Relationship = ( EClanRelationship )friendObj.efriendrelationship;

                        if ( clanList.Contains( friendId ) )
                        {
                            // mark clans we were removed/kicked from
                            // note: not actually sure about the kicked relationship, but i'm using it for good measure
                            if ( clan.Relationship == EClanRelationship.None || clan.Relationship == EClanRelationship.Kicked )
                                clansToRemove.Add( friendId );
                        }
                        else
                        {
                            // don't know about this clan, add it
                            clanList.Add( friendId );
                        }
                    }

                    if ( !list.Body.bincremental )
                    {
                        // request persona state for our friend & clan list when it's a non-incremental update
                        reqInfo.Body.friends.Add( friendId );
                    }
                }

                // remove anything we marked for removal
                friendsToRemove.ForEach( f => friendList.Remove( f ) );
                clansToRemove.ForEach( c => clanList.Remove( c ) );

            }

            if ( reqInfo.Body.friends.Count > 0 )
            {
                this.Client.Send( reqInfo );
            }

            var callback = new FriendsListCallback( list.Body );
            this.Client.PostCallback( callback );
        }
        void HandlePersonaState( IPacketMsg packetMsg )
        {
            var perState = new ClientMsgProtobuf<CMsgClientPersonaState>( packetMsg );

            EClientPersonaStateFlag flags = ( EClientPersonaStateFlag )perState.Body.status_flags;

            foreach ( var friend in perState.Body.friends )
            {
                SteamID friendId = friend.friendid;

                if ( friendId.IsIndividualAccount )
                {
                    User cacheFriend = cache.GetUser( friendId );

                    if ( ( flags & EClientPersonaStateFlag.PlayerName ) == EClientPersonaStateFlag.PlayerName )
                        cacheFriend.Name = friend.player_name;

                    if ( ( flags & EClientPersonaStateFlag.Presence ) == EClientPersonaStateFlag.Presence )
                    {
                        cacheFriend.AvatarHash = friend.avatar_hash;
                        cacheFriend.PersonaState = ( EPersonaState )friend.persona_state;
                        cacheFriend.PersonaStateFlags = ( EPersonaStateFlag )friend.persona_state_flags;
                    }

                    if ( ( flags & EClientPersonaStateFlag.GameDataBlob ) == EClientPersonaStateFlag.GameDataBlob )
                    {
                        cacheFriend.GameName = friend.game_name;
                        cacheFriend.GameID = friend.gameid;
                        cacheFriend.GameAppID = friend.game_played_app_id;
                    }
                }
                else if ( friendId.IsClanAccount )
                {
                    Clan cacheClan = cache.Clans.GetAccount( friendId );

                    if ( ( flags & EClientPersonaStateFlag.PlayerName ) == EClientPersonaStateFlag.PlayerName )
                    {
                        cacheClan.Name = friend.player_name;
                    }

                    if ( (flags & EClientPersonaStateFlag.Presence) == EClientPersonaStateFlag.Presence )
                    {
                        cacheClan.AvatarHash = friend.avatar_hash;
                    }
                }
                else
                {
                }

                // todo: cache other details/account types?
            }

            foreach ( var friend in perState.Body.friends )
            {
                var callback = new PersonaStateCallback( friend );
                this.Client.PostCallback( callback );
            }
        }
        void HandleClanState( IPacketMsg packetMsg )
        {
            var clanState = new ClientMsgProtobuf<CMsgClientClanState>( packetMsg );

            var callback = new ClanStateCallback( clanState.Body );
            this.Client.PostCallback( callback );
        }
        void HandleFriendResponse( IPacketMsg packetMsg )
        {
            var friendResponse = new ClientMsgProtobuf<CMsgClientAddFriendResponse>( packetMsg );

            var callback = new FriendAddedCallback( friendResponse.Body );
            this.Client.PostCallback( callback );
        }
        void HandleChatEnter( IPacketMsg packetMsg )
        {
            var chatEnter = new ClientMsg<MsgClientChatEnter>( packetMsg );

            byte[] payload = chatEnter.Payload.ToArray();

            var callback = new ChatEnterCallback( chatEnter.Body, payload );
            this.Client.PostCallback( callback );
        }
        void HandleChatMsg( IPacketMsg packetMsg )
        {
            var chatMsg = new ClientMsg<MsgClientChatMsg>( packetMsg );

            byte[] msgData = chatMsg.Payload.ToArray();

            var callback = new ChatMsgCallback( chatMsg.Body, msgData );
            this.Client.PostCallback( callback );
        }
        void HandleChatMemberInfo( IPacketMsg packetMsg )
        {
            var membInfo = new ClientMsg<MsgClientChatMemberInfo>( packetMsg );

            byte[] payload = membInfo.Payload.ToArray();

            var callback = new ChatMemberInfoCallback( membInfo.Body, payload );
            this.Client.PostCallback( callback );
        }
        void HandleChatRoomInfo( IPacketMsg packetMsg )
        {
            var roomInfo = new ClientMsg<MsgClientChatRoomInfo>( packetMsg );

            byte[] payload = roomInfo.Payload.ToArray();

            var callback = new ChatRoomInfoCallback( roomInfo.Body, payload );
            this.Client.PostCallback( callback );
        }
        void HandleChatActionResult( IPacketMsg packetMsg )
        {
            var actionResult = new ClientMsg<MsgClientChatActionResult>( packetMsg );

            var callback = new ChatActionResultCallback( actionResult.Body );
            this.Client.PostCallback( callback );
        }
        void HandleChatInvite( IPacketMsg packetMsg )
        {
            var chatInvite = new ClientMsgProtobuf<CMsgClientChatInvite>( packetMsg );

            var callback = new ChatInviteCallback( chatInvite.Body );
            this.Client.PostCallback( callback );
        }
        void HandleIgnoreFriendResponse( IPacketMsg packetMsg )
        {
            var response = new ClientMsg<MsgClientSetIgnoreFriendResponse>( packetMsg );

            var callback = new IgnoreFriendCallback(response.TargetJobID, response.Body);
            this.Client.PostCallback( callback );
        }
        void HandleProfileInfoResponse( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientFriendProfileInfoResponse>( packetMsg );

            var callback = new ProfileInfoCallback( packetMsg.TargetJobID, response.Body );
            Client.PostCallback( callback );
        }
        void HandlePersonaChangeResponse( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgPersonaChangeResponse>( packetMsg );

            // update our cache to what steam says our name is
            cache.LocalUser.Name = response.Body.player_name;

            var callback = new PersonaChangeCallback( packetMsg.TargetJobID, response.Body );
            Client.PostCallback( callback );
        }
        #endregion
    }
}
