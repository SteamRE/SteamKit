using System;

namespace SteamKit2.Blob
{
    class MicroTimeSerializer : IBlobSerializer
    {
        public Type ExpectedType
        {
            get
            {
                return typeof(MicroTime);
            }
        }

        public object Read(Object target, BlobReader reader)
        {
            return new MicroTime(reader.ReadFieldStream().ReadUInt64());
        }

        static Type[] CtorParams = new Type[] { typeof(ulong) };

        public void EmitRead(JITContext context)
        {
            context.ReadFieldStream();
            context.EmitStreamCall("ReadUInt64");
            context.CreateType(typeof(MicroTime), CtorParams);
        }
    }
}
