using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BlobLib
{
    public class BlobField : BlobComponent
    {
        public static readonly int HeaderLength = 6;

        private byte[] descriptor;

        private Blob childBlob;
        private byte[] data;

        public BlobField(byte[] Descriptor, byte[] Data)
        {
            descriptor = Descriptor;
            data = Data;
        }

        public BlobField(byte[] Descriptor, Blob child)
        {
            descriptor = Descriptor;
            childBlob = child;
        }


        public bool HasBlobChild()
        {
            return (childBlob != null);
        }

        private bool IsStringDescriptor()
        {
            return descriptor.Length != 4;
        }

        public string GetStringDescriptor()
        {
            if (!IsStringDescriptor())
                return null;

            return Encoding.ASCII.GetString(descriptor);
        }

        public UInt32 GetIntDescriptor()
        {
            return BitConverter.ToUInt32(descriptor, 0);
        }


        private bool IsStringData()
        {
            bool nonprint = (data.Length == 1);

            for (int i = 0; i < data.Length - 1; i++)
            {
                if ((data[i] < 32 || data[i] > 126))
                {
                    nonprint = true;
                    break;
                }
            }

            return !nonprint;
        }

        public string GetStringData()
        {
            if (!IsStringData())
                return null;

            return Encoding.UTF8.GetString(data, 0, Math.Max(0, data.Length - 1));
        }

        public byte[] GetByteData()
        {
            return data;
        }

        public Blob GetChildBlob()
        {
            return childBlob;
        }


        public override void Dump(StringBuilder sb,int level)
        {
            string space = new String('\t', level);
            sb.AppendLine(space + ToString());
            sb.Append(space + "Descriptor ");

            if (IsStringDescriptor())
            {
                sb.AppendLine(GetStringDescriptor());
            }
            else
            {
                sb.AppendLine(GetIntDescriptor().ToString());
            }

            if (HasBlobChild())
            {
                childBlob.Dump(sb, level + 1);
            }
            else
            {
                if (IsStringData())
                {
                    sb.AppendLine(space + "[" + data.Length + "s] " + GetStringData());
                }
                else
                {
                    if (data.Length < 64)
                    {
                        sb.AppendLine(space + "[" + data.Length + "h] " + BitConverter.ToString(data));
                    }
                    else
                    {
                        sb.AppendLine(space + "[" + data.Length + "h] (too long)");
                    }
                }
            }

        }


        public static BlobField ReadField(BinaryReader reader, out Int32 size)
        {
            long position = reader.BaseStream.Position;

            if ((position + HeaderLength) <= reader.BaseStream.Length)
            {
                try
                {
                    Int16 descriptorLength = reader.ReadInt16();
                    Int32 dataLength = reader.ReadInt32();

                    byte[] descriptor = reader.ReadBytes(descriptorLength);
                    byte[] data = reader.ReadBytes((int)dataLength);

                    size = HeaderLength + descriptorLength + dataLength;

                    Blob innerBlob = Blob.Parse(data);

                    if (innerBlob == null)
                    {
                        return new BlobField(descriptor, data);
                    }
                    else

                    {
                        return new BlobField(descriptor, innerBlob);
                    }
                }
                catch (Exception)
                {
                }
            }

            reader.BaseStream.Position = position;
            size = 0;

            return null;
        }

    }

}
