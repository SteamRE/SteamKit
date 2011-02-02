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


    public class SteamFriends : ClientMsgHandler
    {
        public const string NAME = "SteamFriends";


        public SteamFriends()
            : base( SteamFriends.NAME )
        {
        }

        public override void HandleMsg( EMsg eMsg, byte[] data )
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
        }
    }
}
