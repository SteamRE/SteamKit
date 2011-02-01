using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public struct Friend
    {
        public ulong SteamID { get; set; }
        public EFriendRelationship Relationship { get; set; }

        public override string ToString()
        {
            return SteamID.ToString();
        }
    }

    public class FriendsListEventArgs : EventArgs
    {
        public List<Friend> List { get; private set; }

        public FriendsListEventArgs( List<Friend> friends )
        {
            this.List = friends;
        }
    }

    public class SteamFriends : ClientMsgHandler
    {
        public const string NAME = "SteamFriends";

        public event EventHandler<FriendsListEventArgs> FriendsList;
        protected virtual void OnFriendsList( FriendsListEventArgs e )
        {
            if ( FriendsList != null )
                FriendsList( this, e );
        }

        public SteamFriends()
            : base( SteamFriends.NAME )
        {
        }

        internal override void HandleMsg( EMsg eMsg, byte[] data )
        {
            switch ( eMsg )
            {
                case EMsg.ClientFriendsList:
                    HandleFriendsList( data );
                    break;
            }
        }

        void HandleFriendsList( byte[] data )
        {
            var friendsList = new ClientMsgProtobuf<MsgClientFriendsList>( data );

            List<Friend> list = friendsList.Msg.Proto.friends.ConvertAll<Friend>( ( input ) =>
            {
                return new Friend()
                {
                    SteamID = input.ulfriendid,
                    Relationship = ( EFriendRelationship )input.efriendrelationship,
                };
            } );

            OnFriendsList( new FriendsListEventArgs( list ) );
        }
    }
}
