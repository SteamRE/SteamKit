using System;

namespace SteamKit2.Blob
{
    class Int32Serializer : IBlobSerializer
    {
        public Type ExpectedType
        {
            get
            {
                return typeof(Int32);
            }
        }

        public object Read(Object target, BlobReader reader)
        {
            return reader.ReadFieldStream().ReadInt32();
        }

        public void EmitRead(JITContext context)
        {
            context.ReadFieldStream();
            context.EmitStreamCall("ReadInt32");
        }
    }

    class UInt32Serializer : IBlobSerializer
    {
        public Type ExpectedType
        {
            get
            {
                return typeof(UInt32);
            }
        }

        public object Read(Object target, BlobReader reader)
        {
            return reader.ReadFieldStream().ReadUInt32();
        }

        public void EmitRead(JITContext context)
        {
            context.ReadFieldStream();
            context.EmitStreamCall("ReadUInt32");
        }
    }
}
