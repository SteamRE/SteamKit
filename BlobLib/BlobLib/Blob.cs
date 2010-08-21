using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace BlobLib
{
    public class Blob : BlobComponent
    {
        public static readonly int HeaderLength = 10;

        private List<BlobField> children;
        private ECacheState cachestate;
        private EAutoPreprocessCode processcode;

        private int compressionlevel;
        private byte[] iv;

        public Blob(ECacheState state, EAutoPreprocessCode code)
        {
            children = new List<BlobField>();

            cachestate = state;
            processcode = code;
        }

        public Blob(ECacheState state, EAutoPreprocessCode code, int level)
            : this(state, code)
        {
            compressionlevel = level;
        }

        public List<BlobField> GetFields()
        {
            return children;
        }


        private BlobField LookupField(string descriptor)
        {
            foreach (BlobField field in children)
            {
                string fdesc = field.GetStringDescriptor();
                if (fdesc != null && fdesc.Equals(descriptor, StringComparison.InvariantCultureIgnoreCase))
                {
                    return field;
                }
            }

            return null;
        }

        private BlobField LookupField(int descriptor)
        {
            foreach (BlobField field in children)
            {
                if (field.GetIntDescriptor() == descriptor)
                {
                    return field;
                }
            }

            return null;
        }


        public byte[] GetDescriptor(string descriptor)
        {
            BlobField field = LookupField(descriptor);

            if (field == null || field.HasBlobChild())
                return null;

            return field.GetByteData();
        }

        public byte[] GetDescriptor(int descriptor)
        {
            BlobField field = LookupField(descriptor);

            if (field == null || field.HasBlobChild())
                return null;

            return field.GetByteData();
        }


        public string GetStringDescriptor(string descriptor)
        {
            BlobField field = LookupField(descriptor);

            if (field == null || field.HasBlobChild())
                return null;

            return field.GetStringData();
        }

        public string GetStringDescriptor(int descriptor)
        {
            BlobField field = LookupField(descriptor);

            if (field == null || field.HasBlobChild())
                return null;

            return field.GetStringData();
        }


        public Blob GetBlobDescriptor(string descriptor)
        {
            BlobField field = LookupField(descriptor);

            if (field == null || !field.HasBlobChild())
                return null;

            return field.GetChildBlob();
        }

        public Blob GetBlobDescriptor(int descriptor)
        {
            BlobField field = LookupField(descriptor);

            if (field == null || !field.HasBlobChild())
                return null;

            return field.GetChildBlob();
        }


        public void AddChild(BlobField child)
        {
            children.Add(child);
            child.parent = this;
        }

        public override void Dump(StringBuilder sb, int level)
        {
            string space = new String('\t', level);

            sb.AppendLine(space + ToString() + " " + processcode );
            sb.AppendLine(space + " " + children.Count + " children");

            foreach (BlobField field in children)
            {
                field.Dump(sb, level + 1);
            }
        }


        private static Blob ReadBlobHeader(BinaryReader reader, out Int32 size)
        {
            long position = reader.BaseStream.Position;

            if ((position + HeaderLength) < reader.BaseStream.Length)
            {
                try
                {
                    byte state = reader.ReadByte();
                    byte code = reader.ReadByte();

                    Int32 serializedSize = reader.ReadInt32();
                    Int32 spareSize = reader.ReadInt32();

                    if ((position + serializedSize) <= reader.BaseStream.Length)
                    {
                        EAutoPreprocessCode processcode = (EAutoPreprocessCode)code;
                        ECacheState cachestate = (ECacheState)state;

                        size = serializedSize;

                        if (processcode == EAutoPreprocessCode.eAutoPreprocessCodePlaintext)
                        {
                            return new Blob(cachestate, processcode);
                        }
                        else if (processcode == EAutoPreprocessCode.eAutoPreprocessCodeCompressed)
                        {
                            Int32 decompressedSize = reader.ReadInt32();
                            UInt32 unknown = reader.ReadUInt32();
                            UInt16 level = reader.ReadUInt16();

                            // note: http://www.chiramattel.com/george/blog/2007/09/09/deflatestream-block-length-does-not-match.html
                            // need to skip two bytes for implementation mismatch
                            reader.ReadBytes(2);

                            byte[] compressed = reader.ReadBytes(serializedSize - 2);

                            using (MemoryStream ms = new MemoryStream(compressed))
                            using (DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Decompress))
                            {
                                byte[] inflated = new byte[decompressedSize];
                                deflateStream.Read(inflated, 0, inflated.Length);

                                Blob decompressed = Parse(inflated);
                                size = 0;
                                return decompressed;
                            }

                        }
                        else if (processcode == EAutoPreprocessCode.eAutoPreprocessCodeEncrypted)
                        {
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            reader.BaseStream.Position = position;
            size = 0;

            return null;
        }


        public static Blob Parse(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(ms);

            return Parse(reader);
        }

        public static Blob Parse(BinaryReader reader)
        {
            Int32 serializedSize;
            Blob root = ReadBlobHeader(reader, out serializedSize);

            if (serializedSize == 0)
            {
                return root;
            }

            serializedSize = serializedSize - Blob.HeaderLength;

            if (root == null)
            {
                return null;
            }

            while(serializedSize >= BlobField.HeaderLength)
            {
                Int32 fieldSize;
                BlobField field = BlobField.ReadField(reader, out fieldSize);

                if (field == null)
                {
                    break;
                }

                serializedSize -= fieldSize;
                root.AddChild(field);
            }

            return root;
        }

    }

}
