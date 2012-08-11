using System;
using System.Collections.Generic;

namespace SteamKit2.Blob
{
    /// <summary>
    /// Exception class for a type that can't be parsed
    /// </summary>
    public class BlobUnhandledTypeException : Exception
    {
        internal BlobUnhandledTypeException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Blob reader that builds a type model
    /// </summary>
    public class BlobTypedReader
    {
        /// <summary>
        /// Deserialize a blob into a type. Does not JIT
        /// </summary>
        /// <param name="reader">Blob reader</param>
        /// <param name="type">Target type</param>
        /// <returns>Type model</returns>
        public static object DeserializeSlow(BlobReader reader, Type type)
        {
            TypeSerializer serializer = new TypeSerializer(type);

            using (reader)
            {
                return serializer.Read(null, reader);
            }
        }

        /// <summary>
        /// Deserialize a blob into a type
        /// </summary>
        /// <param name="reader">Blob reader</param>
        /// <param name="type">Target type</param>
        /// <returns>Type model</returns>
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
            else if (type == typeof(MicroTime))
            {
                return new MicroTimeSerializer();
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
                case TypeCode.Int16:
                    return new Int16Serializer();
                case TypeCode.UInt16:
                    return new UInt16Serializer();
                case TypeCode.Boolean:
                    return new BooleanSerializer();
                default:
                    throw new BlobUnhandledTypeException("Unhandled type: " + type);
            }
        }
    }
}
