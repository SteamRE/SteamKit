using System;
using System.Collections.Generic;

namespace SteamKit2.Blob
{
    public class BlobUnhandledTypeException : Exception
    {
        public BlobUnhandledTypeException(string msg) : base(msg) { }
    }

    public class BlobTypedReader
    {
        public static object DeserializeSlow(BlobReader reader, Type type)
        {
            TypeSerializer serializer = new TypeSerializer(type);

            using (reader)
            {
                return serializer.Read(null, reader);
            }
        }

        public static object Deserialize(BlobReader reader, Type type)
        {
            TypeSerializer serializer = new TypeSerializer(type);
            JITContext.BlobDeserializer deserialize = JITContext.BuildDeserializer(serializer);

            using (reader)
            {
                return deserialize(reader, JITContext.ByteKeys.ToArray());
            }
        }

        internal static IBlobSerializer GetSerializerForType(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return new ListSerializer(type, type.GetGenericArguments()[0]);
                }
                else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type[] generics = type.GetGenericArguments();
                    return new DictionarySerializer(type, generics[0], generics[1]);
                }

                throw new BlobUnhandledTypeException("Generic type that we couldn't parse: " + type);
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                    return new TypeSerializer(type);
                case TypeCode.String:
                    return new StringSerializer();
                case TypeCode.Int32:
                    return new Int32Serializer();
                case TypeCode.UInt32:
                    return new UInt32Serializer();
                case TypeCode.Boolean:
                    return new BooleanSerializer();
                default:
                    throw new BlobUnhandledTypeException("Unhandled type: " + type);
            }

            return null;
        }
    }
}
