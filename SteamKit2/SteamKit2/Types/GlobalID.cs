/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SteamKit2
{
    /// <summary>
    /// Represents a globally unique identifier within the Steam network.
    /// Guaranteed to be unique across all racks and servers for a given Steam universe.
    /// </summary>
    [DebuggerDisplay( "{Value}" )]
    public class GlobalID : IEquatable<GlobalID>
    {
        BitVector64 gidBits;


        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalID"/> class.
        /// </summary>
        public GlobalID()
            : this( ulong.MaxValue )
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalID"/> class.
        /// </summary>
        /// <param name="gid">The GID value.</param>
        public GlobalID( ulong gid )
        {
            this.gidBits = new BitVector64( gid );
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.GlobalID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="gid">The gid.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ulong( GlobalID gid )
        {
            if ( gid == null )
            {
                throw new ArgumentNullException( nameof(gid) );
            }

            return gid.gidBits.Data;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit2.GlobalID"/>.
        /// </summary>
        /// <param name="gid">The gid.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator GlobalID( ulong gid )
        {
            return new GlobalID( gid );
        }


        /// <summary>
        /// Gets or sets the sequential count for this GID.
        /// </summary>
        /// <value>
        /// The sequential count.
        /// </value>
        public uint SequentialCount
        {
            get { return ( uint )gidBits[ 0, 0xFFFFF ]; }
            set { gidBits[ 0, 0xFFFFF ] = ( ulong )value; }
        }

        /// <summary>
        /// Gets or sets the start time of the server that generated this GID.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime
        {
            get
            {
                uint startTime = ( uint )gidBits[ 20, 0x3FFFFFFF ];
                return new DateTime( 2005, 1, 1 ).AddSeconds( startTime );
            }
            set
            {
                uint startTime = ( uint )value.Subtract( new DateTime( 2005, 1, 1 ) ).TotalSeconds;
                gidBits[ 20, 0x3FFFFFFF ] = ( ulong )startTime;
            }
        }

        /// <summary>
        /// Gets or sets the process ID of the server that generated this GID.
        /// </summary>
        /// <value>
        /// The process ID.
        /// </value>
        public uint ProcessID
        {
            get { return ( uint )gidBits[ 50, 0xF ]; }
            set { gidBits[ 50, 0xF ] = ( ulong )value; }
        }

        /// <summary>
        /// Gets or sets the box ID of the server that generated this GID.
        /// </summary>
        /// <value>
        /// The box ID.
        /// </value>
        public uint BoxID
        {
            get { return ( uint )gidBits[ 54, 0x3FF ]; }
            set { gidBits[ 54, 0x3FF ] = ( ulong )value; }
        }


        /// <summary>
        /// Gets or sets the entire 64bit value of this GID.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public ulong Value
        {
            get { return gidBits.Data; }
            set { gidBits.Data = value; }
        }


        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( object obj )
        {
            if ( obj == null )
            {
                return false;
            }

            if ( !( obj is GlobalID gid ) )
            {
                return false;
            }

            return gidBits.Data == gid.gidBits.Data;
        }

        /// <summary>
        /// Determines whether the specified <see cref="GlobalID"/> is equal to this instance.
        /// </summary>
        /// <param name="gid">The <see cref="GlobalID"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GlobalID"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals( GlobalID gid )
        {
            if ( ( object )gid == null )
            {
                return false;
            }

            return gidBits.Data == gid.gidBits.Data;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The left side GID.</param>
        /// <param name="b">The right side GID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==( GlobalID? a, GlobalID? b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
            {
                return true;
            }

            if ( a is null || b is null )
            {
                return false;
            }

            return a.gidBits.Data == b.gidBits.Data;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The left side GID.</param>
        /// <param name="b">The right side GID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=( GlobalID? a, GlobalID? b )
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
            return gidBits.Data.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Value.ToString();
        }

    }

    /// <summary>
    /// Represents a single unique handle to a piece of User Generated Content.
    /// </summary>
    public sealed class UGCHandle : GlobalID
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UGCHandle"/> class.
        /// </summary>
        public UGCHandle()
            : base()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UGCHandle"/> class.
        /// </summary>
        /// <param name="ugcId">The UGC ID.</param>
        public UGCHandle( ulong ugcId )
            : base( ugcId )
        {
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.UGCHandle"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="handle">The UGC handle.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ulong( UGCHandle handle )
        {
            if ( handle == null )
            {
                throw new ArgumentNullException( nameof(handle) );
            }

            return handle.Value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit2.UGCHandle"/>.
        /// </summary>
        /// <param name="ugcId">The UGC ID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator UGCHandle( ulong ugcId )
        {
            return new UGCHandle( ugcId );
        }
    }
}
