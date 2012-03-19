/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    /// <summary>
    /// This class holds the 64-bit representation of time using ticks expressed as microseconds
    /// </summary>
    public class MicroTime : IEquatable<ulong>, IComparable<ulong>
    {
        private const ulong TicksInMicrosecond = 10; // 10 ticks to the microsecond
        private const ulong UnixEpoch = 0xDCBFFEFF2BC000UL; // Unix Epoch (UTC) expressed in ticks

        private ulong microSeconds;

        /// <summary>
        /// Returns the MiroTime of the current local time
        /// </summary>
        public static MicroTime Now
        {
            get
            {
                return new MicroTime(DateTime.Now);
            }
        }

        /// <summary>
        /// Returns the MiroTime of the current UTC time
        /// </summary>
        public static MicroTime UtcNow
        {
            get
            {
                return new MicroTime(DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Constructs a MicroTime representing a time of 0
        /// </summary>
        public MicroTime()
            : this( 0 )
        {
        }

        /// <summary>
        /// Constructs a MicroTime from an input time in microseconds
        /// </summary>
        public MicroTime( ulong time )
        {
            this.microSeconds = time;
        }

        /// <summary>
        /// Constructs a MicroTime from an input DateTime
        /// </summary>
        public MicroTime(DateTime time)
        {
            this.microSeconds = (ulong)time.Ticks / TicksInMicrosecond;
        }

        /// <summary>
        /// Implicit conversion of MicroTime to ulong representing the time in microseconds
        /// </summary>
        public static implicit operator ulong( MicroTime microTime )
        {
            return microTime.microSeconds;
        }

        /// <summary>
        /// Implicit conversion of ulong representing the time in microseconds to a MicroTime
        /// </summary>
        public static implicit operator MicroTime( ulong microTime )
        {
            return new MicroTime( microTime );
        }

        /// <summary>
        /// Returns MicroTime as ticks (one tick is 100 nanoseconds)
        /// </summary>
        public ulong ToTicks()
        {
            return microSeconds * TicksInMicrosecond;
        }

        /// <summary>
        /// Returns a DateTime as a UTC datetime
        /// </summary>
        public DateTime ToDateTimeUTC()
        {
            return new DateTime( (long)ToTicks(), DateTimeKind.Utc );
        }

        /// <summary>
        /// Returns a DateTime as the local datetime
        /// </summary>
        public DateTime ToDateTimeLocal()
        {
            return new DateTime( (long)ToTicks(), DateTimeKind.Local );
        }

        /// <summary>
        /// Returns a unix timestamp
        /// </summary>
        public uint ToUnixTime() 
        {
            return MicroTime.GetUnixTime( this.ToDateTimeUTC() );
        }


        /// <summary>
        /// Returns the MicroTime formatted as a string
        /// </summary>
        public override string ToString()
        {
            return ToDateTimeUTC().ToString();
        }

        /// <summary>
        /// Compares MicroTime for equality
        /// </summary>
        public int CompareTo( ulong other )
        {
            return microSeconds.CompareTo( other );
        }

        /// <summary>
        /// Compares MicroTime for equality
        /// </summary>
        public bool Equals(ulong other)
        {
            return microSeconds.Equals(other);
        }

        static uint GetUnixTime( DateTime dt )
        {
            TimeSpan ts = ( dt - new DateTime( 1970, 1, 1, 0, 0, 0 ) );
            return ( uint )ts.TotalSeconds;
        }

        /// <summary>
        /// Deserialize a MicroTime from an input byte array
        /// </summary>
        public static MicroTime Deserialize( byte[] data )
        {
            DataStream ds = new DataStream( data );

            return new MicroTime(ds.ReadUInt64());
        }

        /// <summary>
        /// Serialize a MicroTime to a byte array
        /// </summary>
        public byte[] Serialize()
        {
            using ( BinaryWriterEx bw = new BinaryWriterEx() )
            {
                bw.Write( this.microSeconds );

                return bw.ToArray();
            }
        }
    }
}
