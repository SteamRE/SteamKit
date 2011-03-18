using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Reflection;

namespace CDRUpdater
{
    static class BlobReader
    {
		// may god have mercy on my soul for this code
        public static object ReadFromBlob( Blob blob, Type type )
        {
            object obj = Activator.CreateInstance( type );

            foreach ( var prop in type.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var blobField = prop.GetAttribute<BlobFieldAttribute>();

                if ( blobField == null )
                    continue; // ignore properties that aren't marked as fields

                if ( blobField.FieldType == FieldType.Type )
                {
                    BlobField bf = blob[ blobField.Field ];

                    if ( bf == null )
                        continue;

                    object data = GetData( bf, blobField.ValueType ?? prop.PropertyType );
                    prop.SetValue( obj, data, null );
                }
                else if ( blobField.FieldType == FieldType.BlobList || blobField.FieldType == FieldType.TypeList )
                {
                    var listObj = ( IBlobList )prop.GetValue( obj, null );

                    BlobField innerField = blob[ blobField.Field ];

                    if ( innerField == null )
                        continue;

                    Blob blobList = innerField.GetChildBlob();

                    foreach ( var field in blobList.Fields )
                    {
                        object subData = null;

                        if ( blobField.FieldType == FieldType.BlobList )
                        {
                            Blob subBlob = field.GetChildBlob();
                            subData = ReadFromBlob( subBlob, blobField.ValueType );
                        }
                        else if ( blobField.FieldType == FieldType.TypeList )
                        {
                            subData = GetDescriptor( field, blobField.KeyType );
                        }

                        listObj.Add( subData );
                    }
                }
                else if ( blobField.FieldType == FieldType.TypeDictionary )
                {
                    var dictObj = ( IBlobDictionary )prop.GetValue( obj, null );

                    BlobField innerField = blob[ blobField.Field ];

                    if ( innerField == null )
                        continue;

                    Blob blobList = innerField.GetChildBlob();

                    foreach ( var field in blobList.Fields )
                    {
                        object subKey = GetDescriptor( field, blobField.KeyType );
                        object subValue = GetData( field, blobField.ValueType );

                        dictObj.Add( subKey, subValue );
                    }
                }
            }

            return obj;
        }

        public static T ReadFromBlob<T>( Blob blob )
            where T : IBlob, new()
        {
            return ( T )ReadFromBlob( blob, typeof( T ) );
        }


        static object GetDescriptor( BlobField bf, Type propType )
        {
            object data = null;

            if ( propType == typeof( int ) )
            {
                data = bf.GetInt32Descriptor();
            }
            else if ( propType == typeof( uint ) )
            {
                data = bf.GetUInt32Descriptor();
            }
            else if ( propType == typeof( string ) )
            {
                data = bf.GetStringDescriptor();
            }

            return data;
        }

        static object GetData( BlobField bf, Type propType )
        {
            object data = null;

            if ( propType.IsEnum )
                propType = Enum.GetUnderlyingType( propType );

            if ( propType == typeof( uint ) )
            {
                data = bf.GetUInt32Data();
            }
            else if ( propType == typeof( int ) )
            {
                data = bf.GetInt32Data();
            }
            else if ( propType == typeof( ushort ) )
            {
                data = bf.GetUInt16Data();
            }
            else if ( propType == typeof( short ) )
            {
                data = bf.GetInt16Data();
            }
            else if ( propType == typeof( string ) )
            {
                data = bf.GetStringData();
            }
            else if ( propType == typeof( bool ) )
            {
                data = bf.GetBoolData();
            }
            else if ( propType == typeof( byte[] ) )
            {
                data = bf.Data;
            }
            else if ( propType == typeof( ulong ) )
            {
                data = bf.GetUInt64Data();
            }
            else if ( propType == typeof( long ) )
            {
                data = bf.GetInt64Data();
            }
            else if ( propType == typeof( MicroTime ) )
            {
                data = new MicroTime( bf.GetUInt64Data() );
            }
            else
            {
                throw new NotImplementedException( "Missing BlobField data handler for type!" );
            }

            return data;
        }
    }
}