/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System.Collections.Concurrent;

namespace SteamKit2
{
    public partial class SteamFriends
    {
        abstract class Account
        {
            public SteamID SteamID { get; set; }

            public string? Name { get; set; }
            public byte[]? AvatarHash { get; set; }

            public Account()
            {
                SteamID = new SteamID();
            }
        }

        sealed class User : Account
        {
            public EFriendRelationship Relationship { get; set; }

            public EPersonaState PersonaState { get; set; }
            public EPersonaStateFlag PersonaStateFlags { get; set; }

            public uint GameAppID { get; set; }
            public GameID GameID { get; set; }
            public string? GameName { get; set; }


            public User()
            {
                GameID = new GameID();
            }
        }

        sealed class Clan : Account
        {
            public EClanRelationship Relationship { get; set; }
        }


        sealed class AccountList<T> : ConcurrentDictionary<SteamID, T>
            where T : Account, new()
        {
            public T GetAccount( SteamID steamId )
            {
                return this.GetOrAdd( steamId, new T { SteamID = steamId } );
            }
        }

        class AccountCache
        {
            public User LocalUser { get; private set; }

            public AccountList<User> Users { get; private set; }
            public AccountList<Clan> Clans { get; private set; }

            public AccountCache()
            {
                LocalUser = new User();

                Users = new AccountList<User>();
                Clans = new AccountList<Clan>();
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
