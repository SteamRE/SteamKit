using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BlobLib
{
    public class BlobField
    {
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


        public bool IsStringData()
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

    }

}
