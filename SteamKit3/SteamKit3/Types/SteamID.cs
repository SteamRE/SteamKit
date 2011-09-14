/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SteamKit3
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

    /// <summary>
    /// Represents an entity on the steam network. Every object is assigned a <see cref="SteamID"/> which can be temporary or permanent.
    /// </summary>
    public class SteamID
    {
        private BitVector64 steamid;

        static readonly Regex SteamIDRegex = new Regex(
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


        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        public SteamID()
        {
            steamid = new BitVector64();

            AccountID = 0;
            AccountType = EAccountType.Invalid;
            AccountUniverse = EUniverse.Invalid;
            AccountInstance = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="unAccountID">Account ID.</param>
        /// <param name="eUniverse">Universe this account belongs to.</param>
        /// <param name="eAccountType">Type of account.</param>
        public SteamID( uint unAccountID, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            Set( unAccountID, eUniverse, eAccountType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="unAccountID">Account ID.</param>
        /// <param name="unInstance">The instance of this account.</param>
        /// <param name="eUniverse">Universe this account belongs to.</param>
        /// <param name="eAccountType">Type of account.</param>
        public SteamID( uint unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            InstancedSet( unAccountID, unInstance, eUniverse, eAccountType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class from a 64-bit representation.
        /// </summary>
        /// <param name="id">The 64bit <see cref="SteamID"/>.</param>
        public SteamID( ulong id )
            : this()
        {
            SetFromUint64( id );
        }

        /// <summary>
        /// Attempts initialize this instance from a <see cref="System.String"/> representation of a rendered <see cref="SteamID"/>.
        /// The universe is assumed to be public.
        /// </summary>
        /// <param name="steamId">The string <see cref="SteamID"/>.</param>
        public SteamID( string steamId )
            : this ( steamId, EUniverse.Public )
        {
        }

        /// <summary>
        /// Attempts initialize this instance from a <see cref="System.String"/> representation of a rendered <see cref="SteamID"/>.
        /// </summary>
        /// <param name="steamId">The string <see cref="SteamID"/>.</param>
        /// <param name="eUniverse">The universe this <see cref="SteamID"/> belongs to.</param>
        public SteamID( string steamId, EUniverse eUniverse )
            : this()
        {
            SetFromString( steamId, eUniverse );
        }


        /// <summary>
        /// Sets parameters for this <see cref="SteamID"/>.
        /// </summary>
        /// <param name="unAccountID">Account ID.</param>
        /// <param name="eUniverse">Universe this account belongs to.</param>
        /// <param name="eAccountType">Type of account.</param>
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
                this.AccountInstance = DesktopInstance;
            }
        }

        /// <summary>
        /// Sets parameters for this <see cref="SteamID"/>.
        /// </summary>
        /// <param name="unAccountID">Account ID.</param>
        /// <param name="unInstance">The instance of this account.</param>
        /// <param name="eUniverse">Universe this account belongs to.</param>
        /// <param name="eAccountType">Type of account.</param>
        public void InstancedSet( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;
            this.AccountInstance = unInstance;
        }

        /// <summary>
        /// Initializes this <see cref="SteamID"/> from its 52 bit parts and universe/type.
        /// </summary>
        /// <param name="identifier">The account id and instance data.</param>
        /// <param name="eUniverse">Universe this account belongs to.</param>
        /// <param name="eAccountType">Type of account.</param>
        public void FullSet( ulong identifier, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = ( uint )( identifier & AccountIDMask );
            this.AccountInstance = ( uint )( ( identifier >> 32 ) & AccountInstanceMask );
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;
        }

        /// <summary>
        /// Initializes this <see cref="SteamID"/> from its 64-bit representation.
        /// </summary>
        /// <param name="ulSteamID">The 64bit <see cref="SteamID"/>.</param>
        public void SetFromUint64( ulong ulSteamID )
        {
            this.steamid.Data = ulSteamID;
        }


        /// <summary>
        /// Attempts initialize this instance from a <see cref="System.String"/> representation of a rendered <see cref="SteamID"/>.
        /// </summary>
        /// <param name="steamId">The string <see cref="SteamID"/>.</param>
        /// <param name="eUniverse">The universe this <see cref="SteamID"/> belongs to.</param>
        /// <returns>
        ///   <c>true</c> if this instance was successfully initialized from the string; otherwise, <c>false</c>.
        /// </returns>
        public bool SetFromString( string steamId, EUniverse eUniverse )
        {
            Match m = SteamIDRegex.Match( steamId );

            if ( !m.Success )
                return false;

            uint accId = uint.Parse( m.Groups[ "accountid" ].Value );
            uint authServer = uint.Parse( m.Groups[ "authserver" ].Value );

            this.AccountUniverse = eUniverse;
            this.AccountInstance = 1;
            this.AccountType = EAccountType.Individual;
            this.AccountID = ( accId << 1 ) | authServer;

            return true;
        }

        /// <summary>
        /// Converts this <see cref="SteamID"/> to its 64-bit representation.
        /// </summary>
        /// <returns>The 64bit representation of this <see cref="SteamID"/>.</returns>
        public UInt64 ConvertToUint64()
        {
            return this.steamid.Data;
        }


        /// <summary>
        /// Converts the static parts of this <see cref="SteamID"/> to a 64bit representation.
        /// This is used so that multiseat accounts all share the same static account key.
        /// </summary>
        /// <returns>A 64bit static account key.</returns>
        ulong GetStaticAccountKey()
        {
            return ( ( ulong )AccountUniverse << 56 ) + ( ( ulong )AccountType << 52 ) + AccountID;
        }

        /// <summary>
        /// Initializes this <see cref="SteamID"/> to a blank anonymous game server <see cref="SteamID"/> to be filled in by the AM.
        /// </summary>
        /// <param name="eUniverse">The universe this <see cref="SteamID"/> belongs to.</param>
        public void CreateBlankAnonLogon( EUniverse eUniverse )
        {
            AccountID = 0;
            AccountType = EAccountType.AnonGameServer;
            AccountUniverse = eUniverse;
            AccountInstance = 0;
        }

        /// <summary>
        /// Initializes this <see cref="SteamID"/> to a blank anonymous user <see cref="SteamID"/> to be filled in by the AM.
        /// </summary>
        /// <param name="eUniverse">The universe this <see cref="SteamID"/> belongs to.</param>
        public void CreateBlankAnonUserLogon( EUniverse eUniverse )
        {
            AccountID = 0;
            AccountType = EAccountType.AnonUser;
            AccountUniverse = eUniverse;
            AccountInstance = 0;
        }

        /// <summary>
        /// Initializes this <see cref="SteamID"/> to a blank individual user <see cref="SteamID"/> to be filled in by the AM.
        /// </summary>
        /// <param name="eUniverse">The universe this <see cref="SteamID"/> belongs to.</param>
        public void CreateBlankUserLogon( EUniverse eUniverse )
        {
            AccountID = 0;
            AccountType = EAccountType.Individual;
            AccountUniverse = eUniverse;
            AccountInstance = 0;
        }

        /// <summary>
        /// Determines if this instance is a blank anon user or game server account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a blank anon account; otherwise, <c>false</c>.
        /// </returns>
        public bool BBlankAnonAccount()
        {
            return this.AccountID == 0 && BAnonAccount() && this.AccountInstance == 0;
        }

        /// <summary>
        /// Determines if this instance is a game server account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a game server account; otherwise, <c>false</c>.
        /// </returns>
        public bool BGameServerAccount()
        {
            return this.AccountType == EAccountType.GameServer || this.AccountType == EAccountType.AnonGameServer;
        }
        /// <summary>
        /// Determines if this instance is a content server account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a content server account; otherwise, <c>false</c>.
        /// </returns>
        public bool BContentServerAccount()
        {
            return this.AccountType == EAccountType.ContentServer;
        }
        /// <summary>
        /// Determines whether this instance is a clan account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a clan account; otherwise, <c>false</c>.
        /// </returns>
        public bool BClanAccount()
        {
            return this.AccountType == EAccountType.Clan;
        }
        /// <summary>
        /// Determines whether this instance is a chat account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a chat account; otherwise, <c>false</c>.
        /// </returns>
        public bool BChatAccount()
        {
            return this.AccountType == EAccountType.Chat;
        }
        /// <summary>
        /// Determines whether this instance is a chat lobby account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a lobby; otherwise, <c>false</c>.
        /// </returns>
        public bool IsLobby()
        {
            return ( this.AccountType == EAccountType.Chat ) && ( ( this.AccountInstance & ( uint )ChatInstanceFlags.Lobby ) != 0 );
        }

        /// <summary>
        /// Determines whether this instance is an individual user account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a an individual account; otherwise, <c>false</c>.
        /// </returns>
        public bool BIndividualAccount()
        {
            return this.AccountType == EAccountType.Individual || this.AccountType == EAccountType.ConsoleUser;
        }
        /// <summary>
        /// Determines whether this instance is an anon user or game server account.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is an anon user or game server; otherwise, <c>false</c>.
        /// </returns>
        public bool BAnonAccount()
        {
            return this.AccountType == EAccountType.AnonUser || this.AccountType == EAccountType.AnonGameServer;
        }
        /// <summary>
        /// Determines whether this instance is an anon user account.
        /// This account type is used to create new accounts or reset passwords.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a anon user account; otherwise, <c>false</c>.
        /// </returns>
        public bool BAnonUserAccount()
        {
            return this.AccountType == EAccountType.AnonUser;
        }
        /// <summary>
        /// Determines whether this instance is a console user account.
        /// These are faked <see cref="SteamID">SteamIDs</see> for PSN friend accounts.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a console user account; otherwise, <c>false</c>.
        /// </returns>
        public bool BConsoleUserAccount()
        {
            return this.AccountType == EAccountType.ConsoleUser;
        }

        /// <summary>
        /// Clears the instance of this account if it's an individual account.
        /// </summary>
        public void ClearIndividualInstance()
        {
            if ( !BIndividualAccount() )
                return;

            AccountInstance = 0;
        }

        /// <summary>
        /// Determines whether this instance is a valid <see cref="SteamID">SteamID</see>.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            if ( this.AccountType <= EAccountType.Invalid || this.AccountType >= EAccountType.Max )
                return false;

            if ( this.AccountUniverse <= EUniverse.Invalid || this.AccountUniverse >= EUniverse.Max )
                return false;

            if ( this.AccountType == EAccountType.Individual )
            {
                if ( this.AccountID == 0 || this.AccountInstance > ConsoleInstance )
                    return false;
            }

            if ( this.AccountType == EAccountType.Clan )
            {
                if ( this.AccountID == 0 || this.AccountInstance != 0 )
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the account id of this <see cref="SteamID">SteamID</see>.
        /// </summary>
        /// <value>
        /// The account id.
        /// </value>
        public UInt32 AccountID
        {
            get { return ( UInt32 )steamid[ 0, 0xFFFFFFFF ]; }
            set { steamid[ 0, 0xFFFFFFFF ] = value; }
        }
        /// <summary>
        /// Gets or sets the account instance of this <see cref="SteamID"/>.
        /// </summary>
        /// <value>
        /// The account instance.
        /// </value>
        public UInt32 AccountInstance
        {
            get { return ( UInt32 )steamid[ 32, 0xFFFFF ]; }
            set { steamid[ 32, 0xFFFFF ] = ( UInt64 )value; }
        }
        /// <summary>
        /// Gets or sets the account type of this <see cref="SteamID"/>.
        /// </summary>
        /// <value>
        /// The account type.
        /// </value>
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
        /// <summary>
        /// Gets or sets the steam universe this account belongs to.
        /// </summary>
        /// <value>
        /// The steam universe.
        /// </value>
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

        /// <summary>
        /// Renders this <see cref="SteamID"/> into the format most commonly used by the source engine.
        /// </summary>
        /// <returns>A rendered string representation of this <see cref="SteamID"/>.</returns>
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

        /// <summary>
        /// Returns a rendered <see cref="System.String"/> that represents this <see cref="SteamID"/> instance.
        /// </summary>
        /// <returns>
        /// A rendered <see cref="System.String"/> that represents this <see cref="SteamID"/> instance.
        /// </returns>
        public override string ToString()
        {
            return Render();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit3.SteamID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="sid">The <see cref="SteamID"/> to convert from.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator UInt64( SteamID sid )
        {
            return sid.ConvertToUint64();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit3.SteamID"/>.
        /// </summary>
        /// <param name="id">The 64bit integer to convert from.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator SteamID( UInt64 id )
        {
            return new SteamID( id );
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( System.Object obj )
        {
            if ( obj == null )
                return false;

            SteamID sid = obj as SteamID;
            if ( ( System.Object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        /// <summary>
        /// Determines whether the specified <see cref="SteamID"/> is equal to this instance.
        /// </summary>
        /// <param name="sid">The <see cref="SteamID"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="SteamID"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals( SteamID sid )
        {
            if ( ( object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        /// <summary>
        /// Determines whether the two specified <see cref="SteamID">SteamIDs</see> are equal to each other.
        /// </summary>
        /// <param name="a">The first <see cref="SteamID"/>.</param>
        /// <param name="b">The second <see cref="SteamID"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==( SteamID a, SteamID b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.steamid.Data == b.steamid.Data;
        }

        /// <summary>
        /// Determines whether the two specified <see cref="SteamID">SteamIDs</see> are not equal to each other.
        /// </summary>
        /// <param name="a">The first <see cref="SteamID"/>.</param>
        /// <param name="b">The second <see cref="SteamID"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=( SteamID a, SteamID b )
        {
            return !( a == b );
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return steamid.Data.GetHashCode();
        }

    }
}
