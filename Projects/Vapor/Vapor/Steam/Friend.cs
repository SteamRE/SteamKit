using System;
using System.Collections.Generic;
using System.Text;
using SteamKit2;

namespace Vapor
{
    class Friend
    {
        public ulong SteamID { get; set; }


        public Friend()
        {
        }
        public Friend( ulong steamid )
        {
            this.SteamID = steamid;
        }

        public string GetGameName()
        {
            if ( !IsInGame() )
                return "";
            try
            {
                return Steam3.SteamFriends.GetFriendGamePlayedExtraInfo( this.SteamID );
            }
            catch
            {
                return "";
            }
        }


        public bool IsInGame()
        {
            try
            {
                string gameName = Steam3.SteamFriends.GetFriendGamePlayedExtraInfo( this.SteamID );
                return !string.IsNullOrEmpty( gameName );
            }
            catch
            {
                return false;
            }
        }

        public bool IsOnline()
        {
            try
            {
                return Steam3.SteamFriends.GetFriendPersonaState( this.SteamID ) != EPersonaState.Offline;
            }
            catch { return false; }
        }

        public string GetName()
        {
            try
            {
                return Steam3.SteamFriends.GetFriendPersonaName( this.SteamID );
            }
            catch
            {
                return "[unknown]";
            }
        }

        public string GetStatus()
        {
            try
            {
                if ( this.IsInGame() )
                    return "In-Game";

                EPersonaState state = Steam3.SteamFriends.GetFriendPersonaState( this.SteamID );

                switch ( state )
                {
                    case EPersonaState.Away:
                        return "Away";

                    case EPersonaState.Busy:
                        return "Busy";

                    case EPersonaState.Online:
                        return "Online";

                    case EPersonaState.Snooze:
                        return "Snooze";
                }

                return "Offline";
            }
            catch
            {
                return "Offline";
            }
        }

        public EPersonaState GetState()
        {
            try
            {
                return Steam3.SteamFriends.GetFriendPersonaState( this.SteamID );
            }
            catch
            {
                return EPersonaState.Offline;
            }
        }
    }
}
