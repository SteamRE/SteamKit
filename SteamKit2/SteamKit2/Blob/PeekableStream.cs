using System;
using System.IO;

namespace SteamKit2
{
    public class PeekableStream : Stream
    {
        private Stream input;

        private int peekPos;
        private byte[] peekBuffer;

        public PeekableStream(Stream input)
        {
            this.input = input;
            this.peekBuffer = null;
            this.peekPos = 0;
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

        public void Mark(int peekSize)
        {
            // break early if we can fulfill this mark with the remaining buffer
            if (peekBuffer != null && peekBuffer.Length - peekSize >= peekPos)
            {
                return;
            }

            // compact the existing buffer
            if (peekBuffer != null && peekPos < peekBuffer.Length)
            {
                int unpeekedSize = peekBuffer.Length - peekPos;

                byte[] newBuffer = new byte[peekSize];
                peekSize = peekSize - unpeekedSize;

                Array.Copy(peekBuffer, peekPos, newBuffer, 0, unpeekedSize);
                input.Read(newBuffer, unpeekedSize, peekSize);

                peekBuffer = newBuffer;
            }
            else
            {
                byte[] inBuffer = new byte[peekSize];
                input.Read(inBuffer, 0, peekSize);

                peekBuffer = inBuffer;
            }

            Reset();
        }

        public void Reset()
        {
            if (peekBuffer == null)
            {
                throw new Exception("Peek buffer already wiped");
            }

            peekPos = 0;
        }

        private void Wipe()
        {
            peekBuffer = null;
            peekPos = 0;
        }

        public override int ReadByte()
        {
            if (peekBuffer != null && peekPos < peekBuffer.Length)
            {
                return peekBuffer[peekPos++];
            }

            Wipe();
            return base.ReadByte();
        }

        public override long Position
        {
            get
            {
                if (peekBuffer != null)
                    return input.Position - (peekBuffer.Length - peekPos);

                return input.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            int inCount = count;

            if (peekBuffer != null)
            {
                int toRead = Math.Min(peekBuffer.Length - peekPos, inCount - read);

                Array.Copy(peekBuffer, peekPos, buffer, offset, toRead);

                read += toRead;
                peekPos += toRead;
                count -= toRead;
            }

            if (count > 0)
                Wipe();

            read += input.Read(buffer, offset+read, count);
            return read;
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
