/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;

namespace SteamKit3
{
    public class GameID
    {

        public enum EGameID
        {
            App = 0,
            GameMod = 1,
            Shortcut = 2,
            P2P = 3
        }

        private BitVector64 gameid;

        public const uint InvalidAppID = 0;


        public GameID()
        {
            gameid = new BitVector64();

            AppType = EGameID.App;
            AppID = InvalidAppID;
            ModID = 0;
        }

        public GameID( ulong id )
            : this()
        {
            gameid.Data = id;
        }
        public GameID( uint nAppID )
            : this()
        {
            AppID = nAppID;
        }
        public GameID( uint appId, uint modId )
            : this()
        {
            AppID = appId;
            ModID = modId;
            AppType = EGameID.GameMod;
        }

        public ulong ToUint64()
        {
            return gameid.Data;
        }

        public void Set( ulong gameId )
        {
            gameid.Data = gameId;
        }

        public bool IsMod()
        {
            return ( this.AppType == EGameID.GameMod );
        }
        public bool IsShortcut()
        {
            return ( this.AppType == EGameID.Shortcut );
        }
        public bool IsP2PFile()
        {
            return ( this.AppType == EGameID.P2P );
        }
        public bool IsSteamApp()
        {
            return ( this.AppType == EGameID.App );
        }

        // provide this as implicit to make them decide what they want
        public static implicit operator string( GameID gid )
        {
            return gid.gameid.Data.ToString();
        }

        public static implicit operator UInt64( GameID gid )
        {
            return gid.gameid.Data;
        }

        public static implicit operator GameID( UInt64 id )
        {
            return new GameID( id );
        }

        public UInt32 AppID
        {
            get
            {
                return ( UInt32 )gameid[ 0, 0xFFFFFF ];
            }
            set
            {
                gameid[ 0, 0xFFFFFF ] = ( UInt64 )value;
            }
        }
        public EGameID AppType
        {
            get
            {
                return ( EGameID )gameid[ 24, 0xFF ];
            }
            set
            {
                gameid[ 24, 0xFF ] = ( UInt64 )value;
            }
        }
        public UInt32 ModID
        {
            get
            {
                return ( UInt32 )gameid[ 32, 0xFFFFFFFF ];
            }
            set
            {
                gameid[ 32, 0xFFFFFFFF ] = ( UInt64 )value;
            }
        }

        public bool IsValid()
        {
            switch ( AppType )
            {
                case EGameID.App:
                    return AppID != InvalidAppID;

                case EGameID.GameMod:
                    return ( AppID != InvalidAppID ) && ( ModID & 0x80000000 ) != 0;

                case EGameID.Shortcut:
                    return ( ModID & 0x80000000 ) != 0;

                case EGameID.P2P:
                    return AppID == InvalidAppID && ( ModID & 0x80000000 ) != 0;

                default:
                    return false;
            }
        }

        void Reset()
        {
            gameid.Data = 0;
        }


        public override bool Equals( System.Object obj )
        {
            if ( obj == null )
                return false;

            GameID gid = obj as GameID;
            if ( ( System.Object )gid == null )
                return false;

            return gameid.Data == gid.gameid.Data;
        }

        public bool Equals( GameID gid )
        {
            if ( ( object )gid == null )
                return false;

            return gameid.Data == gid.gameid.Data;
        }

        public static bool operator ==( GameID a, GameID b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.gameid.Data == b.gameid.Data;
        }

        public static bool operator !=( GameID a, GameID b )
        {
            return !( a == b );
        }

        public override int GetHashCode()
        {
            return gameid.GetHashCode();
        }
    }
}