using System;
using System.Collections.Generic;
using System.Text;

namespace SteamLib
{
    public enum EChatEntryType : int
    {

        Invalid = 0,
        ChatMsg = 1,
        Typing = 2,
        InviteGame = 3,
        Emote = 4,
        LobbyGameStart = 5,
        LeftConversation = 6,

    };
}
