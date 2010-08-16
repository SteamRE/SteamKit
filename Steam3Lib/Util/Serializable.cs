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
            int structSize = Marshal.SizeOf( this );
            IntPtr ptrMem = Marshal.AllocHGlobal( structSize );

            Marshal.StructureToPtr( this, ptrMem, true );

            byte[] structData = new byte[ structSize ];

            Marshal.Copy( ptrMem, structData, 0, structData.Length );

            Marshal.DestroyStructure( ptrMem, typeof( T ) );
            Marshal.FreeHGlobal( ptrMem );

            return structData;
        }

        public static T Deserialize( byte[] data )
        {
            return Deserialize( data, 0 );
        }
        public static T Deserialize( byte[] data, int offset )
        {
            int structSize = Marshal.SizeOf( typeof( T ) );

            if ( data.Length < structSize )
                return null;

            IntPtr ptrMem = Marshal.AllocHGlobal( structSize );
            Marshal.Copy( data, offset, ptrMem, structSize );

            T structObj = ( T )Marshal.PtrToStructure( ptrMem, typeof( T ) );

            Marshal.FreeHGlobal( ptrMem );

            return structObj;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Type thisType = this.GetType();

            sb.Append( thisType.Name + " [ " );

            foreach ( var pi in thisType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                object value = pi.GetValue( this, null );
                sb.AppendFormat( "{0} = {1}, ", pi.Name, value );
            }

            foreach ( var fi in thisType.GetFields( BindingFlags.Public | BindingFlags.Instance ) )
            {
                object val = fi.GetValue( this );
                sb.AppendFormat( "{0} = {1}, ", fi.Name, val );
            }

            sb.Append( " ]" );

            return sb.ToString();
        }
    }
}
