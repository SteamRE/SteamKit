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
            if ( !msg.IsType<SteamFriends.FriendMsgCallback>() )
                return;

            msg.Handle<SteamFriends.FriendMsgCallback>( friendMsg =>
            {
                EChatEntryType type = friendMsg.EntryType;

                if ( type == EChatEntryType.ChatMsg || type == EChatEntryType.Emote || type == EChatEntryType.InviteGame )
                {
                    ChatDialog cd = GetChat( friendMsg.Sender );
                    cd.HandleChat( friendMsg );
                }
            } );

            msg.Handle<SteamFriends.PersonaStateCallback>( personaState =>
            {
                if ( personaState.FriendID == Steam3.SteamClient.SteamID )
                    return;

                ChatDialog cd = GetChat( personaState.FriendID );
                cd.HandleState( personaState );
            } );
        }

    }
}
