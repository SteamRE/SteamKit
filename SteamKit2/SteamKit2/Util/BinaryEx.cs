/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamKit2
{
    public class BinaryReaderEx : BinaryReader
    {
        public BinaryReaderEx(Stream stream)
            : base(stream)
        {
        }

        public static implicit operator Stream(BinaryReaderEx br)
        {
            return br.BaseStream;
        }

        protected override void Dispose(bool disposing)
        {
            //Stop BinaryReader from closing the stream
            base.Dispose(false);
        }
    }
    public class BinaryWriterEx : BinaryWriter
    {
        public bool SwapEndianness { get; set; }
        public long Length { get { return BaseStream.Length; } }
        public long Position { get { return BaseStream.Position; } }

        public BinaryWriterEx()
            : this(false)
        {
        }
        public BinaryWriterEx(bool swapEndianness)
            : this(new MemoryStream(), swapEndianness)
        {
        }
        public BinaryWriterEx(int capacity)
            : this(capacity, false)
        {
        }
        public BinaryWriterEx(int capacity, bool swapEndianness)
            : this(new MemoryStream(capacity), swapEndianness)
        {
        }
        public BinaryWriterEx(Stream stream)
            : this(stream, false)
        {
        }
        public BinaryWriterEx(Stream stream, bool swapEndianness)
            : base(stream)
        {
            SwapEndianness = swapEndianness;
        }

        public static implicit operator Stream(BinaryWriterEx bw)
        {
            return bw.BaseStream;
        }

        protected override void Dispose(bool disposing)
        {
            //Stop BinaryWriter from closing the stream
            base.Dispose(false);
        }

        public void Clear()
        {
            OutStream = new MemoryStream();
        }

        public override void Write(short value)
        {
            if (SwapEndianness)
                value = IPAddress.HostToNetworkOrder(value);
            base.Write(value);
        }
        public override void Write(int value)
        {
            if (SwapEndianness)
                value = IPAddress.HostToNetworkOrder(value);
            base.Write(value);
        }
        public override void Write(long value)
        {
            if (SwapEndianness)
                value = IPAddress.HostToNetworkOrder(value);
            base.Write(value);
        }
        public override void Write(ushort value)
        {
            Write((short)value);
        }
        public override void Write(uint value)
        {
            Write((int)value);
        }
        public override void Write(ulong value)
        {
            Write((long)value);
        }


        public void Write(Type dataType, object data)
        {
            int dataLen = Marshal.SizeOf(dataType);
            IntPtr dataBlock = Marshal.AllocHGlobal(dataLen);

            Marshal.StructureToPtr(data, dataBlock, true);

            byte[] byteData = new byte[dataLen];

            Marshal.Copy(dataBlock, byteData, 0, dataLen);

            Marshal.FreeHGlobal(dataBlock);

            if (SwapEndianness)
                Array.Reverse(byteData);

            Write(byteData);
        }
        //Not named 'Write' because when you would call Write(byte[]) it would try to call Write<byte[]>(byte[])
        public void WriteType<T>(T data) where T : struct
        {
            Write(typeof(T), data);
        }

        public byte[] ToArray()
        {
            if (BaseStream is MemoryStream)
                return ((MemoryStream)BaseStream).ToArray();
            return null;
        }

        public new void Write(string data)
        {
            Write(data, Encoding.Default);
        }
        public void Write(string data, Encoding encoding)
        {
            if (data == null)
                return;
            Write(encoding.GetBytes(data));
        }

        public void WriteNullTermString(string data)
        {
            WriteNullTermString(data, Encoding.Default);
        }
        public void WriteNullTermString(string data, Encoding encoding)
        {
            Write(data, encoding);
            WriteType<Byte>(0);
        }
    }

}
