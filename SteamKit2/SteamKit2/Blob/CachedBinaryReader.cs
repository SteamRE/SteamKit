using System;
using System.IO;
using System.Diagnostics;

namespace SteamKit2
{
    class CachedBinaryReader : BinaryReader
    {
        private MemoryStream ms;

        private long length;
        private long position;
        private long rollbackpos;

        // we can only seek with MemoryStream
        public CachedBinaryReader(MemoryStream ms)
            : this( ms, ms.Position, ms.Length )
        {
        }

        protected CachedBinaryReader(MemoryStream ms, long position, long length )
            : base(ms)
        {
            this.ms = ms;

            this.length = length;
            this.position = position;

            this.rollbackpos = position;
        }

        public long Position
        {
            get
            {
                return position;
            }
        }

        public bool CanRead( long nbytes )
        {
            Debug.Assert( ms.Position == position );

            if ( nbytes < 0 )
                return false;

            return ( this.length - this.position ) >= nbytes;
        }

        public void StartTransaction()
        {
            Debug.Assert( ms.Position == position );

            rollbackpos = position;
        }

        // after a successful chain of reads, we can commit the internal position of the BinaryReader to our cache
        public void Commit()
        {
            SetCachedPostion( ms.Position );
        }

        // after trying to read and getting an exception, we roll back the internal stream to our cached position
        public void Rollback()
        {
            SetCachedPostion(rollbackpos);
            SetStreamPosition(rollbackpos);
        }

        protected void SetCachedPostion( long position )
        {
            this.position = position;
        }

        protected void SetStreamPosition( long position )
        {
            ms.Position = position;
        }

    }
}
