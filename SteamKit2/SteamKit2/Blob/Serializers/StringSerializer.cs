using System;
using System.Text;

namespace SteamKit2.Blob
{
    class StringSerializer : IBlobSerializer
    {
        public Type ExpectedType
        {
            get
            {
                return typeof(string);
            }
        }

        public object Read(Object target, BlobReader reader)
        {
            return Encoding.UTF8.GetString(reader.ReadFieldStream().ReadBytesCached(reader.FieldDataBytes), 0, reader.FieldDataBytes - 1);
        }

        public void EmitRead(JITContext context)
        {
            context.PushUTF8Encoding();
            context.LoadBlobReader();
            context.ReadFieldStream();
            context.GetFieldDataBytes();
            context.EmitStreamCall("ReadBytesCached");
            context.LoadIntConstant(0);
            context.GetFieldDataBytes();
            context.LoadIntConstant(1);
            context.Subtract();
            context.ByteConvertToString();
        }
    }
}
