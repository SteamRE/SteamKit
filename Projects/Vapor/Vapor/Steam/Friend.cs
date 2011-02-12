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
                return Steam3.SteamFriends.GetFriendGamePlayedName( this.SteamID );
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
                string gameName = Steam3.SteamFriends.GetFriendGamePlayedName( this.SteamID );
                return !string.IsNullOrEmpty( gameName );
            }
            catch
            {
                return false;
            }
        }

        public bool IsBlocked()
        {
            EFriendRelationship relationship = Steam3.SteamFriends.GetFriendRelationship( this.SteamID );
            return ( relationship == EFriendRelationship.Ignored || relationship == EFriendRelationship.IgnoredFriend );
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
                string str = "";

                if ( this.IsInGame() )
                {
                    str = "In-Game";

                    if ( this.IsBlocked() )
                        str += " (Blocked)";

                    return str;
                }

                EPersonaState state = Steam3.SteamFriends.GetFriendPersonaState( this.SteamID );

                switch ( state )
                {
                    case EPersonaState.Away:
                        str = "Away";
                        break;

                    case EPersonaState.Busy:
                        str = "Busy";
                        break;

                    case EPersonaState.Online:
                        str = "Online";
                        break;

                    case EPersonaState.Snooze:
                        str = "Snooze";
                        break;

                    default:
                        str = "Offline";
                        break;
                }


                if ( this.IsBlocked() )
                    str += " (Blocked)";

                return str;
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
