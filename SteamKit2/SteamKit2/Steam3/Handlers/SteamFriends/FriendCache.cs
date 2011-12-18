/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            public EPersonaState PersonaState { get; set; }

            public uint GameAppID { get; set; }
            public GameID GameID { get; set; }
            public string GameName { get; set; }


            public User()
            {
                GameID = new GameID();
            }
        }

        sealed class Clan : Account
        {
        }


        class AccountList<T> : List<T>
            where T : Account, new()
        {
            object accessLock = new object();


            public new T this[ int index ]
            {
                get
                {
                    lock ( accessLock )
                    {
                        return base[ index ];
                    }
                }
            }


            public T GetAccount( SteamID steamId )
            {
                lock ( accessLock )
                {
                    EnsureAccount( steamId );

                    return this.Find( accObj => accObj.SteamID == steamId );
                }
            }


            void EnsureAccount( SteamID steamId )
            {
                if ( !this.Contains( steamId ) )
                {
                    T accObj = new T()
                    {
                        SteamID = steamId,
                    };

                    this.Add( accObj );
                }
            }

            public bool Contains( SteamID steamId )
            {
                return this.Any( accObj => accObj.SteamID == steamId );
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
