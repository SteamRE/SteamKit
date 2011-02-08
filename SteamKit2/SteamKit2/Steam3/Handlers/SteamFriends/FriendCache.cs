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
    class FriendsObj
    {
        public SteamID SteamID;

        public FriendsObj( SteamID steamId )
        {
            this.SteamID = steamId;
        }
    }

    class Friend : FriendsObj
    {
        public string Name;
        public EPersonaState PersonaState;

        public string GameName;

        public Friend( SteamID steamId )
            : base( steamId )
        {
        }

    }
    class Clan : FriendsObj
    {
        public string Name;

        public Clan( SteamID steamId )
            : base( steamId )
        {
        }
    }


    class FriendsObjList<T>
        where T : FriendsObj
    {
        List<T> list;

        public int Count { get { return list.Count; } }

        public FriendsObjList()
        {
            list = new List<T>();
        }

        public T GetByID( SteamID steamId )
        {
            foreach ( T obj in list )
            {
                if ( obj.SteamID == steamId )
                    return obj;
            }

            return null;
        }
        public T GetByIndex( int index )
        {
            if ( index >= list.Count )
                return null;

            return list[ index ];
        }
        public void Add( T obj )
        {
            if ( GetByID( obj.SteamID ) != null )
                return; // don't duplicate objects

            list.Add( obj );
        }
    }

    class FriendCache
    {
        FriendsObjList<Friend> friendList;
        FriendsObjList<Clan> clanList;

        public FriendCache()
        {
            friendList = new FriendsObjList<Friend>();
            clanList = new FriendsObjList<Clan>();
        }

        public Friend GetFriend( SteamID steamId )
        {
            return friendList.GetByID( steamId );
        }
        public Friend GetFriendByIndex( int index )
        {
            return friendList.GetByIndex( index );
        }
        public int GetFriendCount()
        {
            return friendList.Count;
        }
        public void AddFriend( Friend friend )
        {
            friendList.Add( friend );
        }

        public Clan GetClan( SteamID steamId )
        {
            return clanList.GetByID( steamId );
        }
        public Clan GetClanByIndex( int index )
        {
            return clanList.GetByIndex( index );
        }
        public int GetClanCount()
        {
            return clanList.Count;
        }
        public void AddClan( Clan clan )
        {
            clanList.Add( clan );
        }
    }
}
