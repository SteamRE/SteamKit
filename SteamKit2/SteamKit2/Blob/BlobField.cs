/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    public class BlobField
    {
        public byte[] Descriptor { get; private set; }
        public byte[] Data { get; private set; }

        private Blob childBlob;


        public BlobField( byte[] Descriptor, byte[] Data )
        {
            this.Descriptor = Descriptor;
            this.Data = Data;
        }
        public BlobField( byte[] Descriptor, Blob child )
        {
            this.Descriptor = Descriptor;
            this.childBlob = child;
        }


        // descriptor
        public bool IsStringDescriptor()
        {
            return IsString( Descriptor );
        }

        public string GetStringDescriptor()
        {
            if ( !IsStringDescriptor() )
                return null;

            return Encoding.ASCII.GetString( Descriptor );
        }
        public Int32 GetInt32Descriptor()
        {
            return BitConverter.ToInt32( Descriptor, 0 );
        }
        public UInt32 GetUInt32Descriptor()
        {
            return BitConverter.ToUInt32( Descriptor, 0 );
        }


        // data
        public bool IsStringData()
        {
            return IsString( Data );
        }
        public bool HasChildBlob()
        {
            return ( childBlob != null );
        }

        public string GetStringData()
        {
            if ( Data == null )
                return null;

            return Encoding.UTF8.GetString( Data, 0, Math.Max( 0, Data.Length - 1 ) );
        }

        public long GetInt64Data()
        {
            return BitConverter.ToInt64( Data, 0 );
        }
        public ulong GetUInt64Data()
        {
            return BitConverter.ToUInt64( Data, 0 );
        }

        public int GetInt32Data()
        {
            return BitConverter.ToInt32( Data, 0 );
        }
        public uint GetUInt32Data()
        {
            return BitConverter.ToUInt32( Data, 0 );
        }

        public short GetInt16Data()
        {
            return BitConverter.ToInt16( Data, 0 );
        }
        public ushort GetUInt16Data()
        {
            return BitConverter.ToUInt16( Data, 0 );
        }

        public bool GetBoolData()
        {
            return BitConverter.ToBoolean( Data, 0 );
        }

        public Blob GetChildBlob()
        {
            return childBlob;
        }


        bool IsString( byte[] data )
        {
            if ( data == null )
                return false;

            bool nonprint = ( data.Length == 1 );

            for ( int i = 0 ; i < data.Length - 1 ; i++ )
            {
                if ( ( data[ i ] < 32 || data[ i ] > 126 ) )
                {
                    nonprint = true;
                    break;
                }
            }

            return !nonprint;
        }
    }
}