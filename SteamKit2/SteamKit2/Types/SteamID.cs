/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        static Regex SteamIDRegex = new Regex(
            @"STEAM_(?<universe>[0-5]):(?<authserver>[0-1]):(?<accountid>\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase );

        /// <summary>
        /// The account instance value when representing all instanced <see cref="SteamID">SteamIDs</see>.
        /// </summary>
        public const uint AllInstances = 0;
        /// <summary>
        /// The account instance value for a desktop <see cref="SteamID"/>.
        /// </summary>
        public const uint DesktopInstance = 1;
        /// <summary>
        /// The account instance value for a console <see cref="SteamID"/>.
        /// </summary>
        public const uint ConsoleInstance = 2;

        /// <summary>
        /// Masking vlaue used for the account id.
        /// </summary>
        public const uint AccountIDMask = 0xFFFFFFFF;
        /// <summary>
        /// Masking value used for packing chat instance flags into a <see cref="SteamID"/>.
        /// </summary>
        public const uint AccountInstanceMask = 0x000FFFFF;


        /// <summary>
        /// Represents various flags a chat <see cref="SteamID"/> may have, packed into its instance.
        /// </summary>
        [Flags]
        public enum ChatInstanceFlags : uint
        {
            /// <summary>
            /// This flag is set for clan based chat <see cref="SteamID">SteamIDs</see>.
            /// </summary>
            Clan = ( AccountInstanceMask + 1 ) >> 1,
            /// <summary>
            /// This flag is set for lobby based chat <see cref="SteamID">SteamIDs</see>.
            /// </summary>
            Lobby = ( AccountInstanceMask + 1 ) >> 2,
            /// <summary>
            /// This flag is set for matchmaking lobby based chat <see cref="SteamID">SteamIDs</see>.
            /// </summary>
            MMSLobby = ( AccountInstanceMask + 1 ) >> 3,
        }

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

        public SteamID( string steamId )
            : this ( steamId, EUniverse.Public )
        {
        }

        public SteamID( string steamId, EUniverse eUniverse )
            : this()
        {
            SetFromString( steamId, eUniverse );
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

        public void SetFromString( string steamId, EUniverse eUniverse )
        {
            Match m = SteamIDRegex.Match( steamId );

            if ( !m.Success )
                return;

            uint accId = uint.Parse( m.Groups[ "accountid" ].Value );
            uint authServer = uint.Parse( m.Groups[ "authserver" ].Value );

            this.AccountUniverse = eUniverse;
            this.AccountInstance = 1;
            this.AccountType = EAccountType.Individual;
            this.AccountID = ( accId << 1 ) | authServer;
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
