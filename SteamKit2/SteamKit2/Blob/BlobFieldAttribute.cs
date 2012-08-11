using System;
using System.Text;

namespace SteamKit2.Blob
{
    /// <summary>
    /// Attribute that holds the key for a Blob field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false, Inherited = true)]
    public class BlobFieldAttribute : Attribute
    {
        /// <summary>
        /// Constructs a field key with a string
        /// </summary>
        /// <param name="key">Key string</param>
        public BlobFieldAttribute(string key)
            : this(Encoding.ASCII.GetBytes(key))
        {
            intKey = -1;
        }

        /// <summary>
        /// Constructs a field key with an integer
        /// </summary>
        /// <param name="key">Integer key</param>
        public BlobFieldAttribute(int key)
            : this(BitConverter.GetBytes(key))
        {
            intKey = key;
        }

        private BlobFieldAttribute(byte[] key)
        {
            byteKey = key;
        }

        /// <summary>
        /// Byte Key represented by this field
        /// </summary>
        public byte[] ByteKey { get { return byteKey; } set { byteKey = value; } }
        /// <summary>
        /// Peekable int key for this field
        /// </summary>
        public int PeekIntKey { get { return intKey; } }
        private byte[] byteKey;
        private int intKey;

        internal IBlobSerializer Serializer;
    }
}
