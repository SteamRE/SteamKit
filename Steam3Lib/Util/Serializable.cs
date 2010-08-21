using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SteamLib
{
    [StructLayout( LayoutKind.Sequential, Pack = 1)]
    public class Serializable<T> where T : Serializable<T>
    {
        public byte[] Serialize()
        {
            int dataSize = Marshal.SizeOf( this );
            byte[] data = new byte[ dataSize ];

            GCHandle dataHandle = GCHandle.Alloc( data, GCHandleType.Pinned );

            try
            {
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                Marshal.StructureToPtr( this, dataPtr, false );
            }
            finally
            {
                dataHandle.Free();
            }

            return data;
        }

        public static T Deserialize( byte[] data )
        {
            return Deserialize( data, 0 );
        }
        public static T Deserialize( byte[] data, int offset )
        {
            T result = default( T );

            GCHandle dataHandle = GCHandle.Alloc( data, GCHandleType.Pinned );

            try
            {
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                result = ( T )Marshal.PtrToStructure( dataPtr, typeof( T ) );
            }
            finally
            {
                dataHandle.Free();
            }

            return result;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Type thisType = this.GetType();

            sb.Append( thisType.Name + " [ " );

            foreach ( var fi in thisType.GetFields() )
            {
                object val = fi.GetValue( this );
                sb.AppendFormat( "{0} = {1}, ", fi.Name, val );
            }

            sb.Append( " ]" );

            return sb.ToString();
        }
    }
}
