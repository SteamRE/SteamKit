/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;

namespace SteamKit3
{
    /// <summary>
    /// Encapsulates an app id mod id pair.
    /// </summary>
    public class GameID
    {

        /// <summary>
        /// Represents all the possible types of <see cref="GameID">GameIDs</see>.
        /// </summary>
        public enum EGameID
        {
            /// <summary>
            /// Regular steam application.
            /// </summary>
            App = 0,
            /// <summary>
            /// Modification.
            /// </summary>
            GameMod = 1,
            /// <summary>
            /// Steam shortcut.
            /// </summary>
            Shortcut = 2,
            /// <summary>
            /// P2P file.
            /// </summary>
            P2P = 3
        }

        private BitVector64 gameid;

        /// <summary>
        /// Represents an invalid appid.
        /// </summary>
        public const uint InvalidAppID = 0;


        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        public GameID()
        {
            gameid = new BitVector64();

            AppType = EGameID.App;
            AppID = InvalidAppID;
            ModID = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        /// <param name="id">The 64bit game id.</param>
        public GameID( ulong id )
            : this()
        {
            Set( id );
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        /// <param name="nAppID">The app id.</param>
        public GameID( uint nAppID )
            : this()
        {
            AppID = nAppID;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        /// <param name="appId">The app id.</param>
        /// <param name="modId">The mod id.</param>
        public GameID( uint appId, uint modId )
            : this()
        {
            AppID = appId;
            ModID = modId;

            AppType = EGameID.GameMod;
        }

        /// <summary>
        /// Converts this <see cref="GameID"/> to its 64bit representation.
        /// </summary>
        /// <returns>The 64bit representation of the <see cref="GameID"/>.</returns>
        public ulong ToUint64()
        {
            return gameid.Data;
        }

        /// <summary>
        /// Initializes this <see cref="GameID"/> from 
        /// </summary>
        /// <param name="gameId">The game id.</param>
        public void Set( ulong gameId )
        {
            gameid.Data = gameId;
        }

        /// <summary>
        /// Determines whether this <see cref="GameID"/> instance is a mod.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is mod; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMod()
        {
            return ( this.AppType == EGameID.GameMod );
        }
        /// <summary>
        /// Determines whether this <see cref="GameID"/> instance is a shortcut.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is shortcut; otherwise, <c>false</c>.
        /// </returns>
        public bool IsShortcut()
        {
            return ( this.AppType == EGameID.Shortcut );
        }
        /// <summary>
        /// Determines whether this <see cref="GameID"/> instance is a p2p file.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a p2p file; otherwise, <c>false</c>.
        /// </returns>
        public bool IsP2PFile()
        {
            return ( this.AppType == EGameID.P2P );
        }
        /// <summary>
        /// Determines whether this <see cref="GameID"/> instance is a steam application.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is a steam application; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSteamApp()
        {
            return ( this.AppType == EGameID.App );
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit3.GameID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="gid">The <see cref="GameID"/> to convert from.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator UInt64( GameID gid )
        {
            return gid.gameid.Data;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit3.GameID"/>.
        /// </summary>
        /// <param name="id">The 64bit represention to convert from.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator GameID( UInt64 id )
        {
            return new GameID( id );
        }

        /// <summary>
        /// Gets or sets the application id of this <see cref="GameID"/> instance.
        /// </summary>
        /// <value>
        /// The application id.
        /// </value>
        public UInt32 AppID
        {
            get { return ( UInt32 )gameid[ 0, 0xFFFFFF ]; }
            set { gameid[ 0, 0xFFFFFF ] = ( UInt64 )value; }
        }
        /// <summary>
        /// Gets or sets the application type of this <see cref="GameID"/> instance.
        /// </summary>
        /// <value>
        /// The application type.
        /// </value>
        public EGameID AppType
        {
            get { return ( EGameID )gameid[ 24, 0xFF ]; }
            set { gameid[ 24, 0xFF ] = ( UInt64 )value; }
        }
        /// <summary>
        /// Gets or sets the mod id of this <see cref="GameID"/> instance.
        /// </summary>
        /// <value>
        /// The mod id.
        /// </value>
        public UInt32 ModID
        {
            get { return ( UInt32 )gameid[ 32, 0xFFFFFFFF ]; }
            set { gameid[ 32, 0xFFFFFFFF ] = ( UInt64 )value; }
        }

        /// <summary>
        /// Determines whether this <see cref="GameID"/> instance is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Resets this instance to default values.
        /// </summary>
        public void Reset()
        {
            gameid.Data = 0;
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

            GameID gid = obj as GameID;
            if ( ( System.Object )gid == null )
                return false;

            return gameid.Data == gid.gameid.Data;
        }

        /// <summary>
        /// Determines whether the specified <see cref="GameID"/> is equal to this instance.
        /// </summary>
        /// <param name="gid">The <see cref="GameID"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GameID"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals( GameID gid )
        {
            if ( ( object )gid == null )
                return false;

            return gameid.Data == gid.gameid.Data;
        }

        /// <summary>
        /// Determines whether the two specified <see cref="GameID">GameIDs</see> are equal to each other.
        /// </summary>
        /// <param name="a">The first <see cref="GameID"/>.</param>
        /// <param name="b">The second <see cref="GameID"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==( GameID a, GameID b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.gameid.Data == b.gameid.Data;
        }

        /// <summary>
        /// Determines whether the two specified <see cref="GameID">GameIDs</see> are not equal to each other.
        /// </summary>
        /// <param name="a">The first <see cref="GameID"/>.</param>
        /// <param name="b">The second <see cref="GameID"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=( GameID a, GameID b )
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
            return gameid.GetHashCode();
        }
    }
}