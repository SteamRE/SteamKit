using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit2
{
    public class Friend
    {
        public ulong SteamID { get; set; }

        public string Name { get; set; }
        public EPersonaState State { get; set; }

        internal Friend( ulong steamid )
        {
            this.SteamID = steamid;
        }

    }

    public class FriendCache : List<Friend>
    {
        public Friend FindByID( SteamID friendId )
        {
            foreach ( Friend friend in this )
            {
                if ( friend.SteamID == friendId )
                    return friend;
            }

            return null;
        }

        public Friend FindByIndex( int index )
        {
            return this[ index ];
        }
    }

    public class PersonaStateCallback : CallbackMsg
    {
        public EClientPersonaStateFlag StatusFlags { get; set; }

        public SteamID FriendID { get; set; }
        public EPersonaState State { get; set; }

        public uint GameAppID { get; set; }
        public ulong GameID { get; set; }
        public string GameName { get; set; }

        public IPAddress GameServerIP { get; set; }
        public uint GameServerPort { get; set; }
        public uint QueryPort { get; set; }

        public SteamID SourceSteamID { get; set; }

        public byte[] GameDataBlob { get; set; }

        public string Name { get; set; }

        public IPAddress CMIPAddress { get; set; }

        public byte[] AvatarHash { get; set; }
        public byte[] ChatMetaData { get; set; }

        public DateTime LastLogOff { get; set; }
        public DateTime LastLogOn { get; set; }

        public uint ClanRank { get; set; }
        public string ClanTag { get; set; }

        internal PersonaStateCallback( CMsgClientPersonaState perState )
        {
            this.StatusFlags = ( EClientPersonaStateFlag )perState.status_flags;

            CMsgClientPersonaState.Friend friend = perState.friends[ 0 ];

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

            this.CMIPAddress = NetHelpers.GetIPAddress( friend.cm_ip );

            this.AvatarHash = friend.avatar_hash;
            this.ChatMetaData = friend.chat_metadata;

            this.LastLogOff = Utils.DateTimeFromUnixTime( friend.last_logoff );
            this.LastLogOn = Utils.DateTimeFromUnixTime( friend.last_logon );

            this.ClanRank = friend.clan_rank;
            this.ClanTag = friend.clan_tag;
        }
    }
    public class FriendsListCallback : CallbackMsg
    {
    }
    public class FriendMsgCallback : CallbackMsg
    {
        public SteamID Sender { get; set; }
        public EChatEntryType EntryType { get; set; }

        public bool FromLimitedAccount { get; set; }

        public string Message { get; set; }

        internal FriendMsgCallback( MsgClientFriendMsgIncoming msg, byte[] msgData )
        {
            this.Sender = msg.SteamID;
            this.EntryType = msg.EntryType;

            this.FromLimitedAccount = ( msg.FromLimitedAccount == 1 );

            this.Message = Encoding.UTF8.GetString( msgData, 0, msgData.Length - 1 );
        }
    }


    public class SteamFriends : ClientMsgHandler
    {
        public const string NAME = "SteamFriends";


        FriendCache cache;


        public SteamFriends()
            : base( SteamFriends.NAME )
        {
            cache = new FriendCache();
        }


        public string GetPersonaName()
        {
            return GetFriendPersonaName( this.Client.SteamID );
        }

        public EPersonaState GetPersonaState()
        {
            return GetFriendPersonaState( this.Client.SteamID );
        }
        public void SetPersonaState( EPersonaState state )
        {
            var stateMsg = new ClientMsg<MsgClientChangeStatus, ExtendedClientMsgHdr>();
            stateMsg.Msg.PersonaState = ( byte )state;

            this.Client.Send( stateMsg );
        }

        public string GetFriendPersonaName( SteamID friend )
        {
            Friend cachedFriend = cache.FindByID( friend );

            if ( cachedFriend == null )
                return "[unknown]";

            if ( string.IsNullOrEmpty( cachedFriend.Name ) )
                return "[unknown]";

            return cachedFriend.Name;
        }
        public EPersonaState GetFriendPersonaState( SteamID friend )
        {
            Friend cachedFriend = cache.FindByID( friend );

            if ( cachedFriend == null )
                return ( EPersonaState )( -1 );

            return cachedFriend.State;
        }

        public uint GetFriendCount()
        {
            return (uint)cache.Count;
        }
        public SteamID GetFriendByIndex( uint index )
        {
            if ( index >= cache.Count )
                return 0;

            Friend cachedFriend = cache.FindByIndex( ( int )index );
            return cachedFriend.SteamID;
        }

        public void SendChatMessage( SteamID target, EChatEntryType type, string message )
        {
            var chatMsg = new ClientMsg<MsgClientFriendMsg, ExtendedClientMsgHdr>();

            byte[] msgData = Encoding.ASCII.GetBytes( message );

            chatMsg.Msg.EntryType = type;
            chatMsg.Msg.SteamID = target;

            chatMsg.Payload.Append( msgData );
            chatMsg.Payload.Append<byte>( 0 );

            this.Client.Send( chatMsg );
        }


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
            }
        }

        void HandleFriendMsg( ClientMsgEventArgs e )
        {
            var friendMsg = new ClientMsg<MsgClientFriendMsgIncoming, ExtendedClientMsgHdr>( e.Data );

            byte[] msgData = friendMsg.Payload.ToArray();

            var callback = new FriendMsgCallback( friendMsg.Msg, msgData );
            this.Client.PostCallback( callback );
        }
        void HandleFriendsList( ClientMsgEventArgs e )
        {
            var list = new ClientMsgProtobuf<MsgClientFriendsList>( e.Data );

            if ( !list.Msg.Proto.bincremental )
                cache.Add( new Friend( this.Client.SteamID ) );

            foreach ( var friend in list.Msg.Proto.friends )
            {
                Friend cacheFriend = new Friend( friend.ulfriendid );
                cache.Add( cacheFriend );
            }

            this.Client.PostCallback( new FriendsListCallback() );
        }
        void HandlePersonaState( ClientMsgEventArgs e )
        {
            var perState = new ClientMsgProtobuf<MsgClientPersonaState>( e.Data );

            EClientPersonaStateFlag stateFlags = ( EClientPersonaStateFlag )perState.Msg.Proto.status_flags;
            CMsgClientPersonaState.Friend friend = perState.Msg.Proto.friends[ 0 ];

            Friend cachedFriend = cache.FindByID( friend.friendid );

            if ( cachedFriend != null )
            {
                if ( ( stateFlags & EClientPersonaStateFlag.PlayerName ) == EClientPersonaStateFlag.PlayerName )
                    cachedFriend.Name = friend.player_name;

                if ( ( stateFlags & EClientPersonaStateFlag.Presence ) == EClientPersonaStateFlag.Presence )
                    cachedFriend.State = ( EPersonaState )friend.persona_state;
            }

            var callback = new PersonaStateCallback( perState.Msg.Proto );
            this.Client.PostCallback( callback );
        }
    }
}
