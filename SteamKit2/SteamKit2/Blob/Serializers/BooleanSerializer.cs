using System;

namespace SteamKit2.Blob
{
    class BooleanSerializer : IBlobSerializer
    {
        public Type ExpectedType
        {
            get
            {
                return typeof(bool);
            }
        }

        public object Read(Object target, BlobReader reader)
        {
            return reader.ReadFieldStream().ReadByte() > 0;
        }

        public void EmitRead(JITContext context)
        {
            context.ReadFieldStream();
            context.EmitStreamCall("ReadByte");
            context.CompareGreaterThanZero();
        }
    }
}
