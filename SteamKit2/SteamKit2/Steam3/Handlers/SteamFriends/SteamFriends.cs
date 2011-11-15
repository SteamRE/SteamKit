/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all interaction with other users on the Steam3 network.
    /// </summary>
    public sealed partial class SteamFriends : ClientMsgHandler
    {
        sealed class Friend
        {
            public SteamID FriendID { get; set; }
            public EFriendRelationship Relationship { get; set; }

            public override bool Equals(object obj)
            {
                return (obj as Friend).FriendID == this.FriendID;
            }

            public override int GetHashCode()
            {
                return FriendID.GetHashCode();
            }
        }

        List<Friend> friendList;
        AccountCache cache;


        internal SteamFriends()
        {
            friendList = new List<Friend>();
            cache = new AccountCache();
        }


        /// <summary>
        /// Gets the local user's persona name.
        /// </summary>
        /// <returns>The name.</returns>
        public string GetPersonaName()
        {
            return cache.LocalUser.Name;
        }
        /// <summary>
        /// Sets the local user's persona name and broadcasts it over the network.
        /// </summary>
        /// <param name="name">The name.</param>
        public void SetPersonaName( string name )
        {
            // todo: figure out the structure of this message
            throw new NotImplementedException();
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
            var stateMsg = new ClientMsg<MsgClientChangeStatus, ExtendedClientMsgHdr>();
            stateMsg.Msg.PersonaState = ( byte )state;

            this.Client.Send( stateMsg );

            // todo: figure out if we get persona state changes for our own actions
            cache.LocalUser.PersonaState = state;
        }

        /// <summary>
        /// Gets the friend count of the local user.
        /// </summary>
        /// <returns>The number of friends.</returns>
        public int GetFriendCount()
        {
            lock ( friendList )
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
            lock ( friendList )
            {
                return friendList[ index ].FriendID;
            }
        }

        /// <summary>
        /// Gets the persona name of a friend.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The name.</returns>
        public string GetFriendPersonaName( SteamID steamId )
        {
            return cache.GetUser( steamId ).Name;
        }
        /// <summary>
        /// Gets the persona state of a friend.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The persona state</returns>
        public EPersonaState GetFriendPersonaState( SteamID steamId )
        {
            return cache.GetUser( steamId ).PersonaState;
        }
        /// <summary>
        /// Gets the relationship of a friend.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns></returns>
        public EFriendRelationship GetFriendRelationship( SteamID steamId )
        {
            lock ( friendList )
            {
                var friend = friendList.Find( friendObj => friendObj.FriendID == steamId );

                if ( friend == null )
                    return EFriendRelationship.None;

                return friend.Relationship;
            }
        }

        /// <summary>
        /// Gets the game name of a friend playing a game.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The game name of a friend playing a game, or null if they haven't been cached yet.</returns>
        public string GetFriendGamePlayedName( SteamID steamId )
        {
            return cache.GetUser( steamId ).GameName;
        }
        /// <summary>
        /// Gets the GameID of a friend playing a game.
        /// </summary>
        /// <param name="steamId">The steam id.</param>
        /// <returns>The gameid of a friend playing a game, or 0 if they haven't been cached yet.</returns>
        public GameID GetFriendGamePlayed( SteamID steamId )
        {
            return cache.GetUser( steamId ).GameID;
        }

        /// <summary>
        /// Gets a SHA-1 hash representing the friend's avatar.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to get the avatar of.</param>
        /// <returns>A byte array representing a SHA-1 hash of the friend's avatar.</returns>
        public byte[] GetFriendAvatar( SteamID steamId )
        {
            return cache.GetUser( steamId ).AvatarHash;
        }

        /// <summary>
        /// Sends a chat message to a friend.
        /// </summary>
        /// <param name="target">The target to send to.</param>
        /// <param name="type">The type of message to send.</param>
        /// <param name="message">The message to send.</param>
        public void SendChatMessage( SteamID target, EChatEntryType type, string message )
        {
            var chatMsg = new ClientMsgProtobuf<MsgClientFriendMsg>();

            chatMsg.Msg.Proto.steamid = target;
            chatMsg.Msg.Proto.chat_entry_type = ( int )type;
            chatMsg.Msg.Proto.message = Encoding.UTF8.GetBytes( message );

            this.Client.Send( chatMsg );
        }

        /// <summary>
        /// Sends a friend request to a user.
        /// </summary>
        /// <param name="accountNameOrEmail">The account name or email of the user.</param>
        public void AddFriend( string accountNameOrEmail )
        {
            var addFriend = new ClientMsgProtobuf<MsgClientAddFriend>();

            addFriend.Msg.Proto.accountname_or_email_to_add = accountNameOrEmail;

            this.Client.Send( addFriend );
        }
        /// <summary>
        /// Sends a friend request to a user.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to add.</param>
        public void AddFriend( SteamID steamId )
        {
            var addFriend = new ClientMsgProtobuf<MsgClientAddFriend>();

            addFriend.Msg.Proto.steamid_to_add = steamId;

            this.Client.Send( addFriend );
        }

        /// <summary>
        /// Removes a friend from your friends list.
        /// </summary>
        /// <param name="steamId">The SteamID of the friend to remove.</param>
        public void RemoveFriend( SteamID steamId )
        {
            var removeFriend = new ClientMsgProtobuf<MsgClientRemoveFriend>();

            removeFriend.Msg.Proto.friendid = steamId;

            this.Client.Send( removeFriend );
        }


        /// <summary>
        /// Attempts to join a chat room.
        /// </summary>
        /// <param name="steamId">The SteamID of the chat room.</param>
        public void JoinChat( SteamID steamId )
        {
            var joinChat = new ClientMsg<MsgClientJoinChat, ExtendedClientMsgHdr>();

            joinChat.Msg.SteamIdChat = steamId;

            Client.Send( joinChat );
        }

        /// <summary>
        /// Sends a message to a chat room.
        /// </summary>
        /// <param name="steamIdChat">The SteamID of the chat room.</param>
        /// <param name="type">The message type.</param>
        /// <param name="message">The message.</param>
        public void SendChatRoomMessage( SteamID steamIdChat, EChatEntryType type, string message )
        {
            var chatMsg = new ClientMsg<MsgClientChatMsg, ExtendedClientMsgHdr>();

            chatMsg.Msg.ChatMsgType = type;
            chatMsg.Msg.SteamIdChatRoom = steamIdChat;
            chatMsg.Msg.SteamIdChatter = Client.SteamID;

            chatMsg.Payload.WriteNullTermString( message, Encoding.UTF8 );

            this.Client.Send( chatMsg );
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="e">The <see cref="SteamKit2.ClientMsgEventArgs"/> instance containing the event data.</param>
        public override void HandleMsg( ClientMsgEventArgs e )
        {
            switch ( e.EMsg )
            {
                case EMsg.ClientPersonaState:
                    HandlePersonaState( e );
                    break;

                case EMsg.ClientFriendsList:
                    HandleFriendsList( e );
                    break;

                case EMsg.ClientFriendMsgIncoming:
                    HandleFriendMsg( e );
                    break;

                case EMsg.ClientAccountInfo:
                    HandleAccountInfo( e );
                    break;

                case EMsg.ClientAddFriendResponse:
                    HandleFriendResponse( e );
                    break;

                case EMsg.ClientChatEnter:
                    HandleChatEnter( e );
                    break;

                case EMsg.ClientChatMsg:
                    HandleChatMsg( e );
                    break;

                case EMsg.ClientChatMemberInfo:
                    HandleChatMemberInfo( e );
                    break;
            }
        }



        #region ClientMsg Handlers
        void HandleAccountInfo( ClientMsgEventArgs e )
        {
            var accInfo = new ClientMsgProtobuf<MsgClientAccountInfo>( e.Data );

            cache.LocalUser.Name = accInfo.Msg.Proto.persona_name;
        }
        void HandleFriendMsg( ClientMsgEventArgs e )
        {
            var friendMsg = new ClientMsgProtobuf<MsgClientFriendMsgIncoming>( e.Data );

#if STATIC_CALLBACKS
            var callback = new FriendMsgCallback( Client, friendMsg.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new FriendMsgCallback( friendMsg.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleFriendsList( ClientMsgEventArgs e )
        {
            var list = new ClientMsgProtobuf<MsgClientFriendsList>( e.Data );

            cache.LocalUser.SteamID = this.Client.SteamID;

            if ( !list.Msg.Proto.bincremental )
            {
                // if we're not an incremental update, the message contains all friends, so we should clear our current list
                lock ( friendList )
                {
                    friendList.Clear();
                }
            }

            // we have to request information for all of our friends because steam only sends persona information for online friends
            var reqInfo = new ClientMsgProtobuf<MsgClientRequestFriendData>();

            reqInfo.Msg.Proto.persona_state_requested = ( uint )( EClientPersonaStateFlag.PlayerName | EClientPersonaStateFlag.Presence );

            lock (friendList)
            {
                List<Friend> friendsToRemove = new List<Friend>();

                foreach (var friendObj in list.Msg.Proto.friends)
                {
                    SteamID friendId = friendObj.ulfriendid;

                    if (!friendId.BIndividualAccount())
                        continue; // don't want to request clan information

                    Friend existingFriend = null;

                    foreach (Friend friend in friendList)
                    {
                        if (friend.FriendID == friendId)
                        {
                            existingFriend = friend;
                            break;
                        }
                    }

                    if (existingFriend != null)
                    {
                        existingFriend.Relationship = (EFriendRelationship)friendObj.efriendrelationship;

                        if (existingFriend.Relationship == EFriendRelationship.None)
                            friendsToRemove.Add(existingFriend);
                    }
                    else
                    {
                        friendList.Add(new Friend()
                        {
                            FriendID = friendId,
                            Relationship = (EFriendRelationship)friendObj.efriendrelationship,
                        });
                    }

                    if (!list.Msg.Proto.bincremental)
                    {
                        reqInfo.Msg.Proto.friends.Add(friendId);
                    }
                }

                foreach (Friend f in friendsToRemove)
                    friendList.Remove(f);
            }

            if (reqInfo.Msg.Proto.friends.Count > 0)
            {
                this.Client.Send(reqInfo);
            }

#if STATIC_CALLBACKS
            var callback = new FriendsListCallback( Client, list.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new FriendsListCallback( list.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandlePersonaState( ClientMsgEventArgs e )
        {
            var perState = new ClientMsgProtobuf<MsgClientPersonaState>( e.Data );

            EClientPersonaStateFlag flags = ( EClientPersonaStateFlag )perState.Msg.Proto.status_flags;

            foreach ( var friend in perState.Msg.Proto.friends )
            {
                SteamID friendId = friend.friendid;

                SteamID sourceId = friend.steamid_source;

                if ( friendId.BIndividualAccount() )
                {
                    User cacheFriend = cache.GetUser( friendId );

                    if ( ( flags & EClientPersonaStateFlag.PlayerName ) == EClientPersonaStateFlag.PlayerName )
                        cacheFriend.Name = friend.player_name;

                    if ( ( flags & EClientPersonaStateFlag.Presence ) == EClientPersonaStateFlag.Presence )
                    {
                        cacheFriend.AvatarHash = friend.avatar_hash;
                        cacheFriend.PersonaState = ( EPersonaState )friend.persona_state;
                    }

                    if ( ( flags & EClientPersonaStateFlag.GameExtraInfo ) == EClientPersonaStateFlag.GameExtraInfo )
                    {
                        cacheFriend.GameName = friend.game_name;
                        cacheFriend.GameID = friend.gameid;
                        cacheFriend.GameAppID = friend.game_played_app_id;
                    }
                }
                else if ( friendId.BClanAccount() )
                {
                    Clan cacheClan = cache.Clans.GetAccount( friendId );

                }

                // todo: cache other details/account types?
            }

            foreach ( var friend in perState.Msg.Proto.friends )
            {
#if STATIC_CALLBACKS
                var callback = new PersonaStateCallback( Client, friend );
                SteamClient.PostCallback( callback );
#else
                var callback = new PersonaStateCallback( friend );
                this.Client.PostCallback( callback );
#endif
            }
        }
        void HandleFriendResponse( ClientMsgEventArgs e )
        {
            var friendResponse = new ClientMsgProtobuf<MsgClientAddFriendResponse>( e.Data );

#if STATIC_CALLBACKS
            var callback = new FriendAddedCallback( Client, friendResponse.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new FriendAddedCallback( friendResponse.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleChatEnter( ClientMsgEventArgs e )
        {
            var chatEnter = new ClientMsg<MsgClientChatEnter, ExtendedClientMsgHdr>( e.Data );

#if STATIC_CALLBACKS
            var callback = new ChatEnterCallback( Client, chatEnter.Msg );
            SteamClient.PostCallback( callback );
#else
            var callback = new ChatEnterCallback( chatEnter.Msg );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleChatMsg( ClientMsgEventArgs e )
        {
            var chatMsg = new ClientMsg<MsgClientChatMsg, ExtendedClientMsgHdr>( e.Data );

            byte[] msgData = chatMsg.Payload.ToArray();

#if STATIC_CALLBACKS
            var callback = new ChatMsgCallback( Client, chatMsg.Msg, msgData );
            SteamClient.PostCallback( callback );
#else
            var callback = new ChatMsgCallback( chatMsg.Msg, msgData );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleChatMemberInfo( ClientMsgEventArgs e )
        {
            var membInfo = new ClientMsg<MsgClientChatMemberInfo, ExtendedClientMsgHdr>( e.Data );

            byte[] payload = membInfo.Payload.ToArray();

#if STATIC_CALLBACKS
            var callback = new ChatMemberInfoCallback( Client, membInfo.Msg, payload );
            SteamClient.PostCallback( callback );
#else
            var callback = new ChatMemberInfoCallback( membInfo.Msg, payload );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion
    }
}
