using System;

namespace SteamKit2.Blob
{
    interface IBlobSerializer
    {
        Type ExpectedType { get; }
        object Read(Object target, BlobReader source);
        void EmitRead(JITContext context);
    }
}
