using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;

namespace Vapor
{
    class ChatManager : ICallbackHandler
    {
        Dictionary<SteamID, ChatDialog> chatMap;

        public ChatManager()
        {
            chatMap = new Dictionary<SteamID, ChatDialog>();

            Steam3.AddHandler( this );
        }

        public ChatDialog GetChat( SteamID steamId )
        {
            if ( chatMap.ContainsKey( steamId ) )
                return chatMap[ steamId ];


            ChatDialog cd = new ChatDialog( steamId );
            chatMap[ steamId ] = cd;

            cd.Show();
            return cd;
        }

        public void Remove( SteamID steamId )
        {
            if ( !chatMap.ContainsKey( steamId ) )
                return;

            chatMap.Remove( steamId );
        }

        public void HandleCallback( CallbackMsg msg )
        {
            if ( !( msg is FriendMsgCallback ) )
                return;

            var friendMsg = ( FriendMsgCallback )msg;

            EChatEntryType type = friendMsg.EntryType;

            if ( type == EChatEntryType.ChatMsg || type == EChatEntryType.Emote )
            {
                ChatDialog cd = GetChat( friendMsg.Sender );
                cd.HandleChat( friendMsg );
            }
        }

    }
}
