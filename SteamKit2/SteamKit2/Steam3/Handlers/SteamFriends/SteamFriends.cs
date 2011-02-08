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

    public class SteamFriends : ClientMsgHandler
    {
        public const string NAME = "SteamFriends";

        Friend localUser;
        FriendCache cache;


        public SteamFriends()
            : base( SteamFriends.NAME )
        {
            localUser = new Friend( 0 );
            cache = new FriendCache();
        }

        public string GetPersonaName()
        {
            return localUser.Name;
        }
        public void SetPersonaName( string name )
        {
            // todo: figure out the structure of this message
            throw new NotImplementedException();
        }

        public EPersonaState GetPersonaState()
        {
            return localUser.PersonaState;
        }
        public void SetPersonaState( EPersonaState state )
        {
            var stateMsg = new ClientMsg<MsgClientChangeStatus, ExtendedClientMsgHdr>();
            stateMsg.Msg.PersonaState = ( byte )state;

            this.Client.Send( stateMsg );

            // todo: figure out if we get persona state changes for our own actions
            localUser.PersonaState = state;
        }

        public int GetFriendCount()
        {
            return cache.GetFriendCount();
        }
        public SteamID GetFriendByIndex( int index )
        {
            Friend friend = cache.GetFriendByIndex( index );

            if ( friend == null )
                return 0;

            return friend.SteamID;
        }

        public string GetFriendPersonaName( SteamID steamId )
        {
            Friend friend = cache.GetFriend( steamId );

            if ( steamId == localUser.SteamID )
                friend = localUser;

            if ( friend == null )
                return "[unknown]";

            if ( string.IsNullOrEmpty( friend.Name ) )
                return "[unknown]";

            return friend.Name;
        }
        public EPersonaState GetFriendPersonaState( SteamID steamId )
        {
            Friend friend = cache.GetFriend( steamId );

            if ( steamId == localUser.SteamID )
                friend = localUser;

            if ( friend == null )
                return EPersonaState.Offline;

            return friend.PersonaState;
        }

        public string GetFriendGamePlayedExtraInfo( SteamID steamId )
        {
            Friend friend = cache.GetFriend( steamId );

            if ( steamId == localUser.SteamID )
                friend = localUser;

            if ( friend == null )
                return "";

            return friend.GameName;
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
            ClientMsg<MsgClientFriendMsgIncoming, ExtendedClientMsgHdr> friendMsg = null;

            try
            {
                friendMsg = new ClientMsg<MsgClientFriendMsgIncoming, ExtendedClientMsgHdr>( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamFriends", "HandleFriendsMsg encountered an exception when trying to read clientmsg.\n{0}", ex.ToString() );
                return;
            }

            byte[] msgData = friendMsg.Payload.ToArray();

            var callback = new FriendMsgCallback( friendMsg.Msg, msgData );
            this.Client.PostCallback( callback );
        }
        void HandleFriendsList( ClientMsgEventArgs e )
        {
            ClientMsgProtobuf<MsgClientFriendsList> list = null;

            try
            {
                list = new ClientMsgProtobuf<MsgClientFriendsList>( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamFriends", "HandleFriendsList encountered an exception when trying to read clientmsg.\n{0}", ex.ToString() );
                return;
            }

            localUser.SteamID = this.Client.SteamID;

            var reqLocalData = new ClientMsgProtobuf<MsgClientRequestFriendData>();
            reqLocalData.Msg.Proto.persona_state_requested = ( uint )( EClientPersonaStateFlag.PlayerName | EClientPersonaStateFlag.Presence );
            reqLocalData.Msg.Proto.friends.Add( this.Client.SteamID ); // request our own information as well

            foreach ( var friend in list.Msg.Proto.friends )
            {
                if ( friend.efriendrelationship != ( uint )EFriendRelationship.Friend )
                    continue; // ignore non-friends

                SteamID friendId = new SteamID( friend.ulfriendid );

                if ( !friendId.BIndividualAccount() )
                    continue; // ignore clans and other non-individual accounts

                Friend cacheFriend = new Friend( friendId );
                cache.AddFriend( cacheFriend );

                reqLocalData.Msg.Proto.friends.Add( friend.ulfriendid );
            }

            this.Client.Send( reqLocalData );

            this.Client.PostCallback( new FriendsListCallback() );
        }
        void HandlePersonaState( ClientMsgEventArgs e )
        {
            ClientMsgProtobuf<MsgClientPersonaState> perState = null;

            try
            {
                perState = new ClientMsgProtobuf<MsgClientPersonaState>( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamFriends", "HandlePersonaState encountered an exception when trying to read clientmsg.\n{0}", ex.ToString() );
                return;
            }

            EClientPersonaStateFlag flags = ( EClientPersonaStateFlag )perState.Msg.Proto.status_flags;

            foreach ( var friend in perState.Msg.Proto.friends )
            {
                Friend cacheFriend = cache.GetFriend( friend.friendid );

                if ( friend.friendid == localUser.SteamID )
                    cacheFriend = localUser;

                if ( cacheFriend == null )
                    continue; // persona info was for someone not in our cache

                if ( ( flags & EClientPersonaStateFlag.PlayerName ) == EClientPersonaStateFlag.PlayerName )
                    cacheFriend.Name = friend.player_name;

                if ( ( flags & EClientPersonaStateFlag.Presence ) == EClientPersonaStateFlag.Presence )
                    cacheFriend.PersonaState = ( EPersonaState )friend.persona_state;

                if ( ( flags & EClientPersonaStateFlag.GameExtraInfo ) == EClientPersonaStateFlag.GameExtraInfo )
                    cacheFriend.GameName = friend.game_name;

                var callback = new PersonaStateCallback( friend, flags );
                this.Client.PostCallback( callback );
            }
        }
    }
}
