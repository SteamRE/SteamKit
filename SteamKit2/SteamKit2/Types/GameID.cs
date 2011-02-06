using System;

namespace SteamKit2
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

        public GameID()
            : this( 0 )
        {
        }
        public GameID( UInt64 id )
        {
            gameid = new BitVector64( id );
        }
        public GameID( Int32 nAppID )
            : this( ( UInt64 )nAppID )
        {
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