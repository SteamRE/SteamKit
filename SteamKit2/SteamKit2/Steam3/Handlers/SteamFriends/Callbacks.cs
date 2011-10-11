/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.ObjectModel;
using System.IO;

namespace SteamKit2
{
    public partial class SteamFriends
    {
        /// <summary>
        /// This callback is fired in response to someone changing their friend details over the network.
        /// </summary>
        public class PersonaStateCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the status flags. This shows what has changed.
            /// </summary>
            /// <value>The status flags.</value>
            public EClientPersonaStateFlag StatusFlags { get; private set; }

            /// <summary>
            /// Gets the friend ID.
            /// </summary>
            /// <value>The friend ID.</value>
            public SteamID FriendID { get; private set; }
            /// <summary>
            /// Gets the state.
            /// </summary>
            /// <value>The state.</value>
            public EPersonaState State { get; private set; }

            /// <summary>
            /// Gets the game app ID.
            /// </summary>
            /// <value>The game app ID.</value>
            public uint GameAppID { get; private set; }
            /// <summary>
            /// Gets the game ID.
            /// </summary>
            /// <value>The game ID.</value>
            public GameID GameID { get; private set; }
            /// <summary>
            /// Gets the name of the game.
            /// </summary>
            /// <value>The name of the game.</value>
            public string GameName { get; private set; }

            /// <summary>
            /// Gets the game server IP.
            /// </summary>
            /// <value>The game server IP.</value>
            public IPAddress GameServerIP { get; private set; }
            /// <summary>
            /// Gets the game server port.
            /// </summary>
            /// <value>The game server port.</value>
            public uint GameServerPort { get; private set; }
            /// <summary>
            /// Gets the query port.
            /// </summary>
            /// <value>The query port.</value>
            public uint QueryPort { get; private set; }

            /// <summary>
            /// Gets the source steam ID.
            /// </summary>
            /// <value>The source steam ID.</value>
            public SteamID SourceSteamID { get; private set; }

            /// <summary>
            /// Gets the game data blob.
            /// </summary>
            /// <value>The game data blob.</value>
            public byte[] GameDataBlob { get; private set; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the avatar hash.
            /// </summary>
            /// <value>The avatar hash.</value>
            public byte[] AvatarHash { get; private set; }

            /// <summary>
            /// Gets the last log off.
            /// </summary>
            /// <value>The last log off.</value>
            public DateTime LastLogOff { get; private set; }
            /// <summary>
            /// Gets the last log on.
            /// </summary>
            /// <value>The last log on.</value>
            public DateTime LastLogOn { get; private set; }

            /// <summary>
            /// Gets the clan rank.
            /// </summary>
            /// <value>The clan rank.</value>
            public uint ClanRank { get; private set; }
            /// <summary>
            /// Gets the clan tag.
            /// </summary>
            /// <value>The clan tag.</value>
            public string ClanTag { get; private set; }

            /// <summary>
            /// Gets the online session instances.
            /// </summary>
            /// <value>The online session instances.</value>
            public uint OnlineSessionInstances { get; private set; }
            /// <summary>
            /// Gets the published session ID.
            /// </summary>
            /// <value>The published session ID.</value>
            public uint PublishedSessionID { get; private set; }

#if STATIC_CALLBACKS
            internal PersonaStateCallback( SteamClient client, CMsgClientPersonaState.Friend friend )
                : base( client )
#else
            internal PersonaStateCallback( CMsgClientPersonaState.Friend friend )
#endif
            {
                this.StatusFlags = ( EClientPersonaStateFlag )friend.persona_state_flags;

                this.FriendID = friend.friendid;
                this.State = ( EPersonaState )friend.persona_state;

                this.GameAppID = friend.game_played_app_id;
                this.GameID = friend.gameid;
                this.GameName = friend.game_name;

                this.GameServerIP = NetHelpers.GetIPAddress( friend.game_server_ip );
                this.GameServerPort = friend.game_server_port;
                this.QueryPort = friend.query_port;

                this.SourceSteamID = friend.steamid_source;

                this.GameDataBlob = friend.game_data_blob;

                this.Name = friend.player_name;

                this.AvatarHash = friend.avatar_hash;

                this.LastLogOff = Utils.DateTimeFromUnixTime( friend.last_logoff );
                this.LastLogOn = Utils.DateTimeFromUnixTime( friend.last_logon );

                this.ClanRank = friend.clan_rank;
                this.ClanTag = friend.clan_tag;

                this.OnlineSessionInstances = friend.online_session_instances;
                this.PublishedSessionID = friend.published_instance_id;
            }
        }

        /// <summary>
        /// This callback is fired when the client receives a list of friends.
        /// </summary>
        public class FriendsListCallback : CallbackMsg
        {
            /// <summary>
            /// Represents a single friend entry in a client's friendlist.
            /// </summary>
            public sealed class Friend
            {
                /// <summary>
                /// Gets the SteamID of the friend.
                /// </summary>
                /// <value>The SteamID.</value>
                public SteamID SteamID { get; private set; }
                /// <summary>
                /// Gets the relationship to this friend.
                /// </summary>
                /// <value>The relationship.</value>
                public EFriendRelationship Relationship { get; private set; }

                internal Friend( CMsgClientFriendsList.Friend friend )
                {
                    this.SteamID = friend.ulfriendid;
                    this.Relationship = ( EFriendRelationship )friend.efriendrelationship;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this <see cref="FriendsListCallback"/> is an incremental update.
            /// </summary>
            /// <value><c>true</c> if incremental; otherwise, <c>false</c>.</value>
            public bool Incremental { get; private set; }
            /// <summary>
            /// Gets the friend list.
            /// </summary>
            /// <value>The friend list.</value>
            public ReadOnlyCollection<Friend> FriendList { get; private set; }

#if STATIC_CALLBACKS
            internal FriendsListCallback( SteamClient client, CMsgClientFriendsList msg )
                : base( client )
#else
            internal FriendsListCallback( CMsgClientFriendsList msg )
#endif
            {
                this.Incremental = msg.bincremental;

                var list = msg.friends.ConvertAll<Friend>(
                    ( input ) =>
                    {
                        return new Friend( input );
                    }
                );

                this.FriendList = new ReadOnlyCollection<Friend>( list );
            }
        }

        /// <summary>
        /// This callback is fired in response to receiving a message from a friend.
        /// </summary>
        public class FriendMsgCallback : CallbackMsg
        {
            /// <summary>
            /// Gets or sets the sender.
            /// </summary>
            /// <value>The sender.</value>
            public SteamID Sender { get; private set; }
            /// <summary>
            /// Gets the chat entry type.
            /// </summary>
            /// <value>The chat entry type.</value>
            public EChatEntryType EntryType { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this message is from a limited account.
            /// </summary>
            /// <value><c>true</c> if this message is from a limited account; otherwise, <c>false</c>.</value>
            public bool FromLimitedAccount { get; private set; }

            /// <summary>
            /// Gets the message.
            /// </summary>
            /// <value>The message.</value>
            public string Message { get; private set; }


#if STATIC_CALLBACKS
            internal FriendMsgCallback( SteamClient client, CMsgClientFriendMsgIncoming msg )
                : base( client )
#else
            internal FriendMsgCallback( CMsgClientFriendMsgIncoming msg )
#endif
            {
                this.Sender = msg.steamid_from;
                this.EntryType = ( EChatEntryType )msg.chat_entry_type;

                this.FromLimitedAccount = msg.from_limited_account;

                if ( msg.message != null && msg.message.Length > 0 )
                    this.Message = Encoding.UTF8.GetString( msg.message, 0, msg.message.Length - 1 );
            }
        }

        /// <summary>
        /// This callback is fired in response to adding a user to your friends list.
        /// </summary>
        public class FriendAddedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the SteamID of the friend that was added.
            /// </summary>
            /// <value>The SteamID.</value>
            public SteamID SteamID { get; private set; }
            /// <summary>
            /// Gets the persona name of the friend.
            /// </summary>
            /// <value>The persona name.</value>
            public string PersonaName { get; private set; }


#if STATIC_CALLBACKS
            internal FriendAddedCallback( SteamClient client, CMsgClientAddFriendResponse msg )
                : base( client )
#else
            internal FriendAddedCallback( CMsgClientAddFriendResponse msg )
#endif
            {
                this.Result = ( EResult )msg.eresult;

                this.SteamID = msg.steam_id_added;

                this.PersonaName = msg.persona_name_added;
            }
        }

        /// <summary>
        /// This callback is fired in response to attempting to join a chat.
        /// </summary>
        public class ChatEnterCallback : CallbackMsg
        {
            /// <summary>
            /// Gets SteamID of the chat room.
            /// </summary>
            public SteamID ChatID { get; private set; }
            public SteamID FriendID { get; private set; }

            /// <summary>
            /// Gets the type of the chat room.
            /// </summary>
            /// <value>
            /// The type of the chat room.
            /// </value>
            public EChatRoomType ChatRoomType { get; private set; }


            public SteamID OwnerID { get; private set; }
            /// <summary>
            /// Gets clan SteamID that owns this chat room.
            /// </summary>
            public SteamID ClanID { get; private set; }

            public byte ChatFlags { get; private set; }

            /// <summary>
            /// Gets the chat enter response.
            /// </summary>
            public EChatRoomEnterResponse EnterResponse { get; private set; }


#if STATIC_CALLBACKS
            internal ChatEnterCallback( SteamClient client, MsgClientChatEnter msg )
                : base( client )
#else
            internal ChatEnterCallback( MsgClientChatEnter msg )
#endif
            {
                ChatID = msg.SteamIdChat;
                FriendID = msg.SteamIdFriend;

                ChatRoomType = msg.ChatRoomType;

                OwnerID = msg.SteamIdOwner;
                ClanID = msg.SteamIdClan;

                ChatFlags = msg.ChatFlags;

                EnterResponse = msg.EnterResponse;
            }
        }

        /// <summary>
        /// This callback is fired when a chat room message arrives.
        /// </summary>
        public class ChatMsgCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the SteamID of the chatter.
            /// </summary>
            public SteamID ChatterID { get; private set; }
            /// <summary>
            /// Gets the SteamID of the chat room.
            /// </summary>
            public SteamID ChatRoomID { get; private set; }

            /// <summary>
            /// Gets chat entry type.
            /// </summary>
            /// <value>
            /// The chat entry type.
            /// </value>
            public EChatEntryType ChatMsgType { get; private set; }

            /// <summary>
            /// Gets the message.
            /// </summary>
            public string Message { get; private set; }


#if STATIC_CALLBACKS
            internal ChatMsgCallback( SteamClient client, MsgClientChatMsg msg, byte[] payload )
                : base( client )
#else
            internal ChatMsgCallback( MsgClientChatMsg msg, byte[] payload )
#endif
            {
                ChatterID = msg.SteamIdChatter;
                ChatRoomID = msg.SteamIdChatRoom;

                ChatMsgType = msg.ChatMsgType;

                if ( payload != null && payload.Length > 0 )
                    Message = Encoding.UTF8.GetString( payload, 0, payload.Length - 1 );
            }
        }

        /// <summary>
        /// This callback is fired in response to chat member info being recieved.
        /// </summary>
        public class ChatMemberInfoCallback : CallbackMsg
        {
            /// <summary>
            /// Gets SteamId of the chat room.
            /// </summary>
            public SteamID ChatRoomID { get; private set; }
            /// <summary>
            /// Gets the info type.
            /// </summary>
            public EChatInfoType Type { get; private set; }

            /// <summary>
            /// Gets SteamID of the chatter.
            /// </summary>
            public SteamID ChatterID { get; private set; }

            public int Action { get; private set; }


#if STATIC_CALLBACKS
            internal ChatMemberInfoCallback( SteamClient client, MsgClientChatMemberInfo msg, byte[] payload )
                : base( client )
#else
            internal ChatMemberInfoCallback( MsgClientChatMemberInfo msg, byte[] payload )
#endif
            {
                ChatRoomID = msg.SteamIdChat;
                Type = msg.Type;

                using ( MemoryStream ms = new MemoryStream( payload ) )
                using ( BinaryReader br = new BinaryReader( ms ) )
                {
                    ChatterID = br.ReadUInt64();
                    Action = br.ReadInt32();
                }
            }
        }
    }
}
