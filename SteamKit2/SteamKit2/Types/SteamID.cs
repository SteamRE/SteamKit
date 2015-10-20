/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteamKit2
{
    class BitVector64
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
    /// This 64-bit structure is used for identifying various objects on the Steam network.
    /// </summary>
    [DebuggerDisplay( "{Render()}, {ConvertToUInt64()}" )]
    public class SteamID
    {
        BitVector64 steamid;

        static Regex Steam2Regex = new Regex(
            @"STEAM_(?<universe>[0-4]):(?<authserver>[0-1]):(?<accountid>\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase );

        static Regex Steam3Regex = new Regex(
            @"\[(?<type>[AGMPCgcLTIUai]):(?<universe>[0-4]):(?<account>\d+)(:(?<instance>\d+))?\]",
            RegexOptions.Compiled );

        static Regex Steam3FallbackRegex = new Regex(
            @"\[(?<type>[AGMPCgcLTIUai]):(?<universe>[0-4]):(?<account>\d+)(\((?<instance>\d+)\))?\]",
            RegexOptions.Compiled );

        static Dictionary<EAccountType, char> AccountTypeChars = new Dictionary<EAccountType, char>
        {
            { EAccountType.AnonGameServer, 'A' },
            { EAccountType.GameServer, 'G' },
            { EAccountType.Multiseat, 'M' },
            { EAccountType.Pending, 'P' },
            { EAccountType.ContentServer, 'C' },
            { EAccountType.Clan, 'g' },
            { EAccountType.Chat, 'T' }, // Lobby chat is 'L', Clan chat is 'c'
            { EAccountType.Invalid, 'I' },
            { EAccountType.Individual, 'U' },
            { EAccountType.AnonUser, 'a' },
        };

        const char UnknownAccountTypeChar = 'i';

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
        /// The account instance for mobile or web based <see cref="SteamID">SteamIDs</see>.
        /// </summary>
        public const uint WebInstance = 4;

        /// <summary>
        /// Masking value used for the account id.
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
            : this( 0 )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public SteamID( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            Set( unAccountID, eUniverse, eAccountType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="unInstance">The instance.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public SteamID( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            InstancedSet( unAccountID, unInstance, eUniverse, eAccountType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="id">The 64bit integer to assign this SteamID from.</param>
        public SteamID( UInt64 id )
        {
            this.steamid = new BitVector64( id );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class from a Steam2 "STEAM_" rendered form.
        /// This constructor assumes the rendered SteamID is in the public universe.
        /// </summary>
        /// <param name="steamId">A "STEAM_" rendered form of the SteamID.</param>
        public SteamID( string steamId )
            : this ( steamId, EUniverse.Public )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class from a Steam2 "STEAM_" rendered form and universe.
        /// </summary>
        /// <param name="steamId">A "STEAM_" rendered form of the SteamID.</param>
        /// <param name="eUniverse">The universe the SteamID belongs to.</param>
        public SteamID( string steamId, EUniverse eUniverse )
            : this()
        {
            SetFromString( steamId, eUniverse );
        }


        /// <summary>
        /// Sets the various components of this SteamID instance.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public void Set( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;

            if ( eAccountType == EAccountType.Clan || eAccountType == EAccountType.GameServer )
            {
                this.AccountInstance = 0;
            }
            else
            {
                this.AccountInstance = DesktopInstance;
            }
        }

        /// <summary>
        /// Sets the various components of this SteamID instance.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="unInstance">The instance.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public void InstancedSet( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;
            this.AccountInstance = unInstance;
        }


        /// <summary>
        /// Sets the various components of this SteamID from a Steam2 "STEAM_" rendered form and universe.
        /// </summary>
        /// <param name="steamId">A "STEAM_" rendered form of the SteamID.</param>
        /// <param name="eUniverse">The universe the SteamID belongs to.</param>
        /// <returns><c>true</c> if this instance was successfully assigned; otherwise, <c>false</c> if the given string was in an invalid format.</returns>
        public bool SetFromString( string steamId, EUniverse eUniverse )
        {
            if ( string.IsNullOrEmpty( steamId ) )
                return false;

            Match m = Steam2Regex.Match( steamId );

            if ( !m.Success )
                return false;

            uint accId, authServer;
            if ( !uint.TryParse( m.Groups[ "accountid" ].Value, out accId ) || 
                 !uint.TryParse( m.Groups[ "authserver" ].Value, out authServer ) )
                return false;

            this.AccountUniverse = eUniverse;
            this.AccountInstance = 1;
            this.AccountType = EAccountType.Individual;
            this.AccountID = ( accId << 1 ) | authServer;

            return true;
        }

        /// <summary>
        /// Sets the various components of this SteamID from a Steam3 "[X:1:2:3]" rendered form and universe.
        /// </summary>
        /// <param name="steamId">A "[X:1:2:3]" rendered form of the SteamID.</param>
        /// <returns><c>true</c> if this instance was successfully assigned; otherwise, <c>false</c> if the given string was in an invalid format.</returns>
        public bool SetFromSteam3String( string steamId )
        {
            if ( string.IsNullOrEmpty( steamId ) )
                return false;

            Match m = Steam3Regex.Match( steamId );

            if ( !m.Success )
            {
                m = Steam3FallbackRegex.Match( steamId );

                if ( !m.Success )
                    return false;
            }

            uint accId;
            if ( !uint.TryParse( m.Groups[ "account" ].Value, out accId ) )
                return false;

            uint universe;
            if ( !uint.TryParse( m.Groups[ "universe" ].Value, out universe ) )
                return false;

            char type;
            var typeString = m.Groups[ "type" ].Value;
            if ( typeString.Length != 1 )
                return false;
            type = typeString[ 0 ];

            uint instance;
            var instanceGroup = m.Groups[ "instance" ];
            if ( instanceGroup != null && !string.IsNullOrEmpty( instanceGroup.Value ) )
            {
                instance = uint.Parse( instanceGroup.Value );
            }
            else
            {
                switch ( type )
                {
                    case 'g':
                    case 'T':
                    case 'c':
                    case 'L':
                        instance = 0;
                        break;

                    default:
                        instance = 1;
                        break;
                }
            }

            if ( type == 'c' )
            {
                instance = ( uint )( ( ChatInstanceFlags )instance | ChatInstanceFlags.Clan );
                this.AccountType = EAccountType.Chat;
            }
            else if ( type == 'L' )
            {
                instance = ( uint )( ( ChatInstanceFlags )instance | ChatInstanceFlags.Lobby );
                this.AccountType = EAccountType.Chat;
            }
            else if ( type == UnknownAccountTypeChar )
            {
                this.AccountType = EAccountType.Invalid;
            }
            else
            {
                this.AccountType = AccountTypeChars.First( x => x.Value == type ).Key;
            }

            this.AccountUniverse = ( EUniverse )universe;
            this.AccountInstance = instance;
            this.AccountID = accId;

            return true;
        }

        /// <summary>
        /// Sets the various components of this SteamID from a 64bit integer form.
        /// </summary>
        /// <param name="ulSteamID">The 64bit integer to assign this SteamID from.</param>
        public void SetFromUInt64( UInt64 ulSteamID )
        {
            this.steamid.Data = ulSteamID;
        }

        /// <summary>
        /// Converts this SteamID into it's 64bit integer form.
        /// </summary>
        /// <returns>A 64bit integer representing this SteamID.</returns>
        public UInt64 ConvertToUInt64()
        {
            return this.steamid.Data;
        }

        /// <summary>
        /// Returns a static account key used for grouping accounts with differing instances.
        /// </summary>
        /// <returns>A 64bit static account key.</returns>
        public ulong GetStaticAccountKey()
        {
            return ( ( ulong )AccountUniverse << 56 ) + ( ( ulong )AccountType << 52 ) + AccountID;
        }




        /// <summary>
        /// Gets a value indicating whether this instance is a blank anonymous account
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a blank anon account; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlankAnonAccount
        {
            get { return this.AccountID == 0 && IsAnonAccount && this.AccountInstance == 0; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a game server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a game server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsGameServerAccount
        {
            get { return this.AccountType == EAccountType.GameServer || this.AccountType == EAccountType.AnonGameServer; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a persistent game server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a persistent game server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersistentGameServerAccount
        {
            get { return this.AccountType == EAccountType.GameServer; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is an anonymous game server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an anon game server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnonGameServerAccount
        {
            get { return this.AccountType == EAccountType.AnonGameServer; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a content server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a content server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsContentServerAccount
        {
            get { return this.AccountType == EAccountType.ContentServer; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a clan account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a clan account; otherwise, <c>false</c>.
        /// </value>
        public bool IsClanAccount
        {
            get { return this.AccountType == EAccountType.Clan; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a chat account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a chat account; otherwise, <c>false</c>.
        /// </value>
        public bool IsChatAccount
        {
            get { return this.AccountType == EAccountType.Chat; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a lobby.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a lobby; otherwise, <c>false</c>.
        /// </value>
        public bool IsLobby
        {
            get {
                return ( this.AccountType == EAccountType.Chat ) &&
                    ( ( this.AccountInstance & ( uint )ChatInstanceFlags.Lobby ) > 0 );
            }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is an individual account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an individual account; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndividualAccount
        {
            get { return this.AccountType == EAccountType.Individual || this.AccountType == EAccountType.ConsoleUser; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is an anonymous account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an anon account; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnonAccount
        {
            get { return this.AccountType == EAccountType.AnonUser || this.AccountType == EAccountType.AnonGameServer; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is an anonymous user account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an anon user account; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnonUserAccount
        {
            get { return this.AccountType == EAccountType.AnonUser; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a console user account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a console user account; otherwise, <c>false</c>.
        /// </value>
        public bool IsConsoleUserAccount
        {
            get { return this.AccountType == EAccountType.ConsoleUser; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                if ( this.AccountType <= EAccountType.Invalid || this.AccountType >= EAccountType.Max )
                    return false;

                if ( this.AccountUniverse <= EUniverse.Invalid || this.AccountUniverse >= EUniverse.Max )
                    return false;

                if ( this.AccountType == EAccountType.Individual )
                {
                    if ( this.AccountID == 0 || this.AccountInstance > WebInstance )
                        return false;
                }

                if ( this.AccountType == EAccountType.Clan )
                {
                    if ( this.AccountID == 0 || this.AccountInstance != 0 )
                        return false;
                }

                if ( this.AccountType == EAccountType.GameServer )
                {
                    if ( this.AccountID == 0 )
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets or sets the account id.
        /// </summary>
        /// <value>
        /// The account id.
        /// </value>
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
        /// <summary>
        /// Gets or sets the account instance.
        /// </summary>
        /// <value>
        /// The account instance.
        /// </value>
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
        /// <summary>
        /// Gets or sets the account type.
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
        /// Gets or sets the account universe.
        /// </summary>
        /// <value>
        /// The account universe.
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
        /// Renders this instance into it's Steam2 "STEAM_" or Steam3 representation.
        /// </summary>
        /// <param name="steam3">If set to <c>true</c>, the Steam3 rendering will be returned; otherwise, the Steam2 STEAM_ rendering.</param>
        /// <returns>
        /// A string Steam2 "STEAM_" representation of this SteamID, or a Steam3 representation.
        /// </returns>
        public string Render( bool steam3 = false )
        {
            if ( steam3 )
                return RenderSteam3();

            return RenderSteam2();
        }

        string RenderSteam2()
        {
            switch ( AccountType )
            {
                case EAccountType.Invalid:
                case EAccountType.Individual:
                    var universeDigit = ( AccountUniverse <= EUniverse.Public ) ? "0" : Enum.Format( typeof( EUniverse ), AccountUniverse, "D" );
                    return String.Format( "STEAM_{0}:{1}:{2}", universeDigit, AccountID & 1, AccountID >> 1 );
                default:
                    return Convert.ToString( this );
            }
        }

        string RenderSteam3()
        {
            char accountTypeChar;
            if ( !AccountTypeChars.TryGetValue( AccountType, out accountTypeChar ) )
            {
                accountTypeChar = UnknownAccountTypeChar;
            }

            if ( AccountType == EAccountType.Chat )
            {
                if ( ( ( ChatInstanceFlags )AccountInstance ).HasFlag( ChatInstanceFlags.Clan ) )
                   accountTypeChar = 'c';
                else if ( ( ( ChatInstanceFlags )AccountInstance ).HasFlag( ChatInstanceFlags.Lobby ) )
                    accountTypeChar = 'L';
            }

            bool renderInstance = false;

            switch ( AccountType )
            {
                case EAccountType.AnonGameServer:
                case EAccountType.Multiseat:
                    renderInstance = true;
                    break;

                case EAccountType.Individual:
                    renderInstance = (AccountInstance != DesktopInstance);
                    break;
            }

            if ( renderInstance )
            {
                    return string.Format( "[{0}:{1}:{2}:{3}]", accountTypeChar, ( uint )AccountUniverse, AccountID, AccountInstance );
            }
            else 
            {
                    return string.Format( "[{0}:{1}:{2}]", accountTypeChar, ( uint )AccountUniverse, AccountID );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // for compatibility, we will always return a Steam2 rendering when ToString()'d
            return Render( false );
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.SteamID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="sid">The SteamID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator UInt64( SteamID sid )
        {
            return sid.steamid.Data;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit2.SteamID"/>.
        /// </summary>
        /// <param name="id">A 64bit integer representing the SteamID.</param>
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
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The left side SteamID.</param>
        /// <param name="b">The right side SteamID.</param>
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
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The left side SteamID.</param>
        /// <param name="b">The right side SteamID.</param>
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
