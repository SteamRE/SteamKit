/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Diagnostics;

namespace SteamKit2
{
    /// <summary>
    /// This 64bit structure represents an app, mod, shortcut, or p2p file on the Steam network.
    /// </summary>
    [DebuggerDisplay( "{ToUInt64()}" )]
    public class GameID
    {
        /// <summary>
        /// Represents various types of games.
        /// </summary>
        public enum GameType
        {
            /// <summary>
            /// A Steam application.
            /// </summary>
            App = 0,
            /// <summary>
            /// A game modification.
            /// </summary>
            GameMod = 1,
            /// <summary>
            /// A shortcut to a program.
            /// </summary>
            Shortcut = 2,
            /// <summary>
            /// A peer-to-peer file.
            /// </summary>
            P2P = 3
        }

        BitVector64 gameid;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        public GameID()
            : this( 0 )
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        /// <param name="id">The 64bit integer to assign this GameID from.</param>
        public GameID( UInt64 id )
        {
            gameid = new BitVector64( id );
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        /// <param name="nAppID">The 32bit app id to assign this GameID from.</param>
        public GameID( Int32 nAppID )
            : this( ( UInt64 )nAppID )
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        /// <param name="nAppID">The base app id of the mod.</param>
        /// <param name="modPath">The game folder name of the mod.</param>
        public GameID( UInt32 nAppID, string modPath )
            : this(0)
        {
            AppID = nAppID;
            AppType = GameType.GameMod;
            ModID = Crc32.Compute(System.Text.Encoding.UTF8.GetBytes(modPath));
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GameID"/> class.
        /// </summary>
        /// <param name="exePath">The path to the executable, usually quoted.</param>
        /// <param name="appName">The name of the application shortcut.</param>
        public GameID( string exePath, string appName )
            : this(0)
        {
            string combined = string.Empty;
            if (exePath != null)
                combined += exePath;
            if (appName != null)
                combined += appName;

            AppID = 0;
            AppType = GameType.Shortcut;
            ModID = Crc32.Compute(System.Text.Encoding.UTF8.GetBytes(combined));
        }


        /// <summary>
        /// Sets the various components of this GameID from a 64bit integer form.
        /// </summary>
        /// <param name="gameId">The 64bit integer to assign this GameID from.</param>
        public void Set( ulong gameId )
        {
            gameid.Data = gameId;
        }
        /// <summary>
        /// Converts this GameID into it's 64bit integer form.
        /// </summary>
        /// <returns>A 64bit integer representing this GameID.</returns>
        public ulong ToUInt64()
        {
            return gameid.Data;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.GameID"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="gid">The GameID to convert..</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string( GameID? gid )
        {
            if ( gid is null )
            {
                throw new ArgumentNullException( nameof(gid) );
            }

            return gid.gameid.Data.ToString();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.GameID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="gid">The GameId to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator UInt64( GameID? gid )
        {
            if ( gid is null )
            {
                throw new ArgumentNullException( nameof(gid) );
            }

            return gid.gameid.Data;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit2.GameID"/>.
        /// </summary>
        /// <param name="id">The 64bit integer representing a GameID to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator GameID( UInt64 id )
        {
            return new GameID( id );
        }


        /// <summary>
        /// Gets or sets the app id.
        /// </summary>
        /// <value>
        /// The app IDid
        /// </value>
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
        /// <summary>
        /// Gets or sets the type of the app.
        /// </summary>
        /// <value>
        /// The type of the app.
        /// </value>
        public GameType AppType
        {
            get
            {
                return ( GameType )gameid[ 24, 0xFF ];
            }
            set
            {
                gameid[ 24, 0xFF ] = ( UInt64 )value;
            }
        }
        /// <summary>
        /// Gets or sets the mod id.
        /// </summary>
        /// <value>
        /// The mod ID.
        /// </value>
        public UInt32 ModID
        {
            get
            {
                return ( UInt32 )gameid[ 32, 0xFFFFFFFF ];
            }
            set
            {
                gameid[ 32, 0xFFFFFFFF ] = ( UInt64 )value;
                gameid[ 63, 0xFF ] = 1;
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance is a mod.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a mod; otherwise, <c>false</c>.
        /// </value>
        public bool IsMod
        {
            get { return ( this.AppType == GameType.GameMod ); }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a shortcut.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a shortcut; otherwise, <c>false</c>.
        /// </value>
        public bool IsShortcut
        {
            get { return ( this.AppType == GameType.Shortcut ); }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a peer-to-peer file.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a p2p file; otherwise, <c>false</c>.
        /// </value>
        public bool IsP2PFile
        {
            get { return ( this.AppType == GameType.P2P ); }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a steam app.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a steam app; otherwise, <c>false</c>.
        /// </value>
        public bool IsSteamApp
        {
            get { return ( this.AppType == GameType.App ); }
        }


        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( object? obj )
        {
            if ( !( obj is GameID gid ) )
            {
                return false;
            }

            return gameid.Data == gid.gameid.Data;
        }

        /// <summary>
        /// Determines whether the specified <see cref="GameID"/> is equal to this instance.
        /// </summary>
        /// <param name="gid">The <see cref="GameID"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GameID"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals( GameID? gid )
        {
            if ( gid is null )
            {
                return false;
            }

            return gameid.Data == gid.gameid.Data;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The left side GameID.</param>
        /// <param name="b">The right side GameID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==( GameID? a, GameID? b )
        {
            if ( object.ReferenceEquals( a, b ) )
            {
                return true;
            }

            if ( a is null || b is null )
            {
                return false;
            }

            return a.gameid.Data == b.gameid.Data;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The left side GameID.</param>
        /// <param name="b">The right side GameID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=( GameID? a, GameID? b )
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToUInt64().ToString();
        }
    }
}
