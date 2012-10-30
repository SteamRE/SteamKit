using System;

namespace SteamKit2.Blob
{
    class Int16Serializer : IBlobSerializer
    {
        public Type ExpectedType
        {
            get
            {
                return typeof(Int16);
            }
        }

        public object Read(Object target, BlobReader reader)
        {
            return reader.ReadFieldStream().ReadInt16();
        }

        public void EmitRead(JITContext context)
        {
            context.LoadBlobReader();
            context.ReadFieldStream();
            context.StreamRead(2, "Int16");
        }
    }

    class UInt16Serializer : IBlobSerializer
    {
        public Type ExpectedType
        {
            get
            {
                return typeof(UInt16);
            }
        }

        public object Read(Object target, BlobReader reader)
        {
            return reader.ReadFieldStream().ReadUInt16();
        }

        public void EmitRead(JITContext context)
        {
            context.LoadBlobReader();
            context.ReadFieldStream();
            context.StreamRead(2, "UInt16");
        }
    }
}
