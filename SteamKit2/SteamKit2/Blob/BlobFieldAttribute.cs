using System;
using System.Text;

namespace SteamKit2.Blob
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false, Inherited = true)]
    public class BlobFieldAttribute : Attribute
    {
        public BlobFieldAttribute(string key)
            : this(Encoding.ASCII.GetBytes(key))
        { }

        public BlobFieldAttribute(int key)
            : this(BitConverter.GetBytes(key))
        {
        }

        private BlobFieldAttribute(byte[] key)
        {
            byteKey = key;
        }

        public byte[] ByteKey { get { return byteKey; } set { byteKey = value; } }
        private byte[] byteKey;

        internal IBlobSerializer Serializer;
    }
}
