using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit2
{
    internal class BitVector64
    {
        private UInt64 data;

        public BitVector64()
        {
        }
        public BitVector64( UInt64 value )
        {
            data = value;
        }

        public UInt64 Data
        {
            get { return data; }
            set { data = value; }
        }

        public UInt64 this[ uint bitoffset, UInt64 valuemask ]
        {
            get
            {
                return ( data >> ( ushort )bitoffset ) & valuemask;
            }
            set
            {
                data = ( data & ~( valuemask << ( ushort )bitoffset ) ) | ( ( value & valuemask ) << ( ushort )bitoffset );
            }
        }
    }

    public class SteamID
    {
        private BitVector64 steamid;

        public SteamID()
            : this( 0 )
        {
        }

        public SteamID( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            Set( unAccountID, eUniverse, eAccountType );
        }

        public SteamID( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            InstancedSet( unAccountID, unInstance, eUniverse, eAccountType );
        }

        public SteamID( UInt64 id )
        {
            this.steamid = new BitVector64( id );
        }

        public void Set( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;

            if ( eAccountType == EAccountType.Clan )
            {
                this.AccountInstance = 0;
            }
            else
            {
                this.AccountInstance = 1;
            }
        }

        public void InstancedSet( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;
            this.AccountInstance = unInstance;
        }

        public void SetFromUint64( UInt64 ulSteamID )
        {
            this.steamid.Data = ulSteamID;
        }

        public void SetFromSteam2( SteamGlobalUserID steam2id, EUniverse universe )
        {
            BitVector64 usersplit = new BitVector64( steam2id.AccountID );

            this.AccountType = EAccountType.Individual;
            this.AccountInstance = steam2id.Instance;
            this.AccountUniverse = universe;
            this.AccountID = ( uint )usersplit[ 0, 0xFFFFFFFF ] * 2 + ( uint )usersplit[ 32, 0xFFFFFFFF ];
        }

        public UInt64 ConvertToUint64()
        {
            return this.steamid.Data;
        }

        public bool BBlankAnonAccount()
        {
            return this.AccountID == 0 && BAnonAccount() && this.AccountInstance == 0;
        }
        public bool BGameServerAccount()
        {
            return this.AccountType == EAccountType.GameServer || this.AccountType == EAccountType.AnonGameServer;
        }
        public bool BContentServerAccount()
        {
            return this.AccountType == EAccountType.ContentServer;
        }
        public bool BClanAccount()
        {
            return this.AccountType == EAccountType.Clan;
        }
        public bool BChatAccount()
        {
            return this.AccountType == EAccountType.Chat;
        }
        public bool IsLobby()
        {
            return ( this.AccountType == EAccountType.Chat ) && ( ( this.AccountInstance & ( 0x000FFFFF + 1 ) >> 2 ) != 0 );
        }
        public bool BIndividualAccount()
        {
            return this.AccountType == EAccountType.Individual;
        }
        public bool BAnonAccount()
        {
            return this.AccountType == EAccountType.AnonUser || this.AccountType == EAccountType.AnonGameServer;
        }
        public bool BAnonUserAccount()
        {
            return this.AccountType == EAccountType.AnonUser;
        }

        public bool IsValid()
        {
            if ( this.AccountType <= EAccountType.Invalid || this.AccountType >= EAccountType.Max )
                return false;

            if ( this.AccountUniverse <= EUniverse.Invalid || this.AccountUniverse >= EUniverse.Max )
                return false;

            if ( this.AccountType == EAccountType.Individual )
            {
                if ( this.AccountID == 0 || this.AccountInstance != 1 )
                    return false;
            }

            if ( this.AccountType == EAccountType.Clan )
            {
                if ( this.AccountID == 0 || this.AccountInstance != 0 )
                    return false;
            }

            return true;
        }

        public UInt32 AccountID
        {
            get
            {
                return ( UInt32 )steamid[ 0, 0xFFFFFFFF ];
            }
            set
            {
                steamid[ 0, 0xFFFFFFFF ] = value;
            }
        }

        public UInt32 AccountInstance
        {
            get
            {
                return ( UInt32 )steamid[ 32, 0xFFFFF ];
            }
            set
            {
                steamid[ 32, 0xFFFFF ] = ( UInt64 )value;
            }
        }

        public EAccountType AccountType
        {
            get
            {
                return ( EAccountType )steamid[ 52, 0xF ];
            }
            set
            {
                steamid[ 52, 0xF ] = ( UInt64 )value;
            }
        }

        public EUniverse AccountUniverse
        {
            get
            {
                return ( EUniverse )steamid[ 56, 0xFF ];
            }
            set
            {
                steamid[ 56, 0xFF ] = ( UInt64 )value;
            }
        }

        public string Render()
        {
            switch ( AccountType )
            {
                case EAccountType.Invalid:
                case EAccountType.Individual:
                    if ( AccountUniverse <= EUniverse.Public )
                        return String.Format( "STEAM_0:{0}:{1}", AccountID & 1, AccountID >> 1 );
                    else
                        return String.Format( "STEAM_{2}:{0}:{1}", AccountID & 1, AccountID >> 1, ( int )AccountUniverse );
                default:
                    return Convert.ToString( this );
            }
        }

        public override string ToString()
        {
            return Render();
        }

        public static implicit operator UInt64( SteamID sid )
        {
            return sid.steamid.Data;
        }

        public static implicit operator SteamID( UInt64 id )
        {
            return new SteamID( id );
        }

        public override bool Equals( System.Object obj )
        {
            if ( obj == null )
                return false;

            SteamID sid = obj as SteamID;
            if ( ( System.Object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        public bool Equals( SteamID sid )
        {
            if ( ( object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        public static bool operator ==( SteamID a, SteamID b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.steamid.Data == b.steamid.Data;
        }

        public static bool operator !=( SteamID a, SteamID b )
        {
            return !( a == b );
        }

        public override int GetHashCode()
        {
            return steamid.Data.GetHashCode();
        }

    }
}
