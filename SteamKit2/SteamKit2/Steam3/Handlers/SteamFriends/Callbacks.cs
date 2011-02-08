/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit2
{
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

        internal PersonaStateCallback( CMsgClientPersonaState.Friend friend, EClientPersonaStateFlag flag )
        {
            this.StatusFlags = flag;

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
}
