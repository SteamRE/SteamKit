/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SteamKit2
{
    public partial class SteamFriends
    {
        abstract class Account
        {
            public SteamID SteamID { get; set; }

            public string Name { get; set; }
            public byte[] AvatarHash { get; set; }

            public Account()
            {
                Name = "[unknown]";
                SteamID = new SteamID();
            }
        }

        sealed class User : Account
        {
            public string Nickname { get; set; }

            public EFriendRelationship Relationship { get; set; }

            public EPersonaState PersonaState { get; set; }
            public EPersonaStateFlag PersonaStateFlags { get; set; }

            public uint GameAppID { get; set; }
            public GameID GameID { get; set; }
            public string GameName { get; set; }
            public List<int> GroupIDs { get; set; }


            public User()
            {
                GameID = new GameID();
                GroupIDs = new List<int>();
            }
        }

        sealed class Clan : Account
        {
            public EClanRelationship Relationship { get; set; }
        }

        class Group
        {
            public int GroupID;
            public string Name { get; set; }
            public AccountList<User> Members { get; set; }
            public Group()
            {
                Members = new AccountList<User>();
            }
        }

        sealed class AccountList<T> : ConcurrentDictionary<SteamID, T>
            where T : Account, new()
        {
            public T GetAccount( SteamID steamId )
            {
                return this.GetOrAdd( steamId, new T { SteamID = steamId } );
            }
        }

        sealed class GroupList<T> : ConcurrentDictionary<int, T>
            where T : Group, new()
        {
            public T GetGroup( int groupId )
            {
                return this.GetOrAdd( groupId, new T { GroupID = groupId });
            }
        }

        class AccountCache
        {
            public User LocalUser { get; private set; }

            public AccountList<User> Users { get; private set; }
            public AccountList<Clan> Clans { get; private set; }
            public GroupList<Group> Groups { get; private set; }
            public AccountCache()
            {
                LocalUser = new User();
                LocalUser.Name = "[unassigned]";

                Users = new AccountList<User>();
                Clans = new AccountList<Clan>();
                Groups = new GroupList<Group>();
            }


            public User GetUser( SteamID steamId )
            {
                if ( IsLocalUser( steamId ) )
                {
                    return LocalUser;
                }
                else
                {
                    return Users.GetAccount( steamId );
                }
            }

            public bool IsLocalUser( SteamID steamId )
            {
                return LocalUser.SteamID == steamId;
            }
        }
    }
}
