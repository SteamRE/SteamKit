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


        public bool HasBlobChild()
        {
            return ( childBlob != null );
        }

        private bool IsStringDescriptor()
        {
            return Descriptor.Length != 4;
        }

        public string GetStringDescriptor()
        {
            if ( !IsStringDescriptor() )
                return null;

            return Encoding.ASCII.GetString( Descriptor );
        }

        public UInt32 GetIntDescriptor()
        {
            return BitConverter.ToUInt32( Descriptor, 0 );
        }

        public T GetDescriptor<T>()
        {
            GCHandle handle = GCHandle.Alloc( Descriptor, GCHandleType.Pinned );
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                T obj = ( T )Marshal.PtrToStructure( ptr, typeof( T ) );

                return obj;
            }
            finally
            {
                handle.Free();
            }
        }


        public bool IsStringData()
        {
            bool nonprint = ( Data.Length == 1 );

            for ( int i = 0 ; i < Data.Length - 1 ; i++ )
            {
                if ( ( Data[ i ] < 32 || Data[ i ] > 126 ) )
                {
                    nonprint = true;
                    break;
                }
            }

            return !nonprint;
        }

        public string GetStringData()
        {
            if ( Data == null )
                return null;

            return Encoding.UTF8.GetString( Data, 0, Math.Max( 0, Data.Length - 1 ) );
        }


        public T GetData<T>()
            where T : struct
        {
            GCHandle handle = GCHandle.Alloc( Data, GCHandleType.Pinned );
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                T obj = ( T )Marshal.PtrToStructure( ptr, typeof( T ) );

                return obj;
            }
            finally
            {
                handle.Free();
            }
        }

        public Blob GetChildBlob()
        {
            return childBlob;
        }

    }
}