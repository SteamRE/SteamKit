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


            public Account()
            {
                SteamID = new SteamID();
            }
        }

        sealed class Friend : Account
        {
            public string Name { get; set; }

            public EPersonaState PersonaState { get; set; }
            public EFriendRelationship Relationship { get; set; }

            public uint GameAppID { get; set; }
            public GameID GameID { get; set; }
            public string GameName { get; set; }


            public Friend()
            {
                Name = "[unknown]";

                GameID = new GameID();
            }
        }

        sealed class Clan : Account
        {
            public string Name { get; set; }


            public Clan()
            {
                Name = "[unknown]";
            }
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
            public Friend LocalFriend { get; private set; }

            public AccountList<Friend> Friends { get; private set; }
            public AccountList<Clan> Clans { get; private set; }


            public AccountCache()
            {
                LocalFriend = new Friend();

                Friends = new AccountList<Friend>();
                Clans = new AccountList<Clan>();
            }


            public Friend GetFriend( SteamID steamId )
            {
                if ( IsLocalUser( steamId ) )
                {
                    return LocalFriend;
                }
                else
                {
                    return Friends.GetAccount( steamId );
                }
            }

            public bool IsLocalUser( SteamID steamId )
            {
                return LocalFriend.SteamID == steamId;
            }
        }
    }
}
