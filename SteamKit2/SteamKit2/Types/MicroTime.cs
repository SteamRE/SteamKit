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
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class MicroTime : /*Serializable<MicroTime>,*/ IEquatable<ulong>, IComparable<ulong>
    {
        public ulong Time;

        public static MicroTime Now
        {
            get
            {
                return new MicroTime( MicroTime.GetMicroSecNow() );
            }
        }


        public MicroTime()
            : this( 0 )
        {
        }
        public MicroTime( ulong time )
        {
            this.Time = time;
        }


        public static implicit operator ulong( MicroTime microTime )
        {
            return microTime.Time;
        }
        public static implicit operator MicroTime( ulong microTime )
        {
            return new MicroTime( microTime );
        }


        public DateTime ToDateTimeUTC()
        {
            return new DateTime( (long)Time * 10, DateTimeKind.Utc );
        }

        public DateTime ToDateTimeLocal()
        {
            return new DateTime((long)Time * 10, DateTimeKind.Local);
        }

        public uint ToUnixTime() 
        {
            return MicroTime.GetUnixTime( this.ToDateTimeUTC() );
        }


        public override string ToString()
        {
            return ToDateTimeUTC().ToString();
        }


        public bool Equals( ulong other )
        {
            return Time.Equals( other );
        }

        public int CompareTo( ulong other )
        {
            return Time.CompareTo( other );
        }


        static ulong GetMicroSecNow()
        {
            return 0xDCBFFEFF2BC000UL + ( ( ulong )GetUnixTime( DateTime.UtcNow ) * 1000000UL );
        }
        static uint GetUnixTime( DateTime dt )
        {
            TimeSpan ts = ( dt - new DateTime( 1970, 1, 1, 0, 0, 0 ) );
            return ( uint )ts.TotalSeconds;
        }

        
        public static MicroTime Deserialize( byte[] data )
        {
            DataStream ds = new DataStream( data );

            MicroTime mt = new MicroTime();

            mt.Time = ds.ReadUInt64();

            return mt;
        }

        public byte[] Serialize()
        {
            using ( BinaryWriterEx bw = new BinaryWriterEx() )
            {
                bw.Write( this.Time );

                return bw.ToArray();
            }
        }
    }
}
