using System;
using System.IO;

namespace SteamKit2
{
    public class FramingStream : Stream
    {
        private Stream input;

        private int availableLength;

        public FramingStream(Stream input, int length)
        {
            this.input = input;
            this.availableLength = length;
        }

        public override bool CanRead
        {
            get { return input.CanRead; }
        }

        public override bool CanSeek
        {
            get { return input.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return input.Length; }
        }

        public override int ReadByte()
        {
            if (availableLength <= 0)
                return 0;

            availableLength--;
            return base.ReadByte();
        }

        public override long Position
        {
            get
            {
                return input.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (availableLength < count)
            {
                count = availableLength;
            }

            availableLength -= count;

            return input.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
