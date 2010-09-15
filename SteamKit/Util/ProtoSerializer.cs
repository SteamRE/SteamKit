using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ProtoBuf;

namespace SteamKit
{
    static class ProtoSerializer
    {
        public static byte[] Serialize<T>( T instance )
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Serializer.Serialize( ms, instance );
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                return Serializer.Deserialize<T>( ms );
            }
        }
    }
}
