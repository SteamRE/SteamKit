using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SteamKit2
{
    static class Extensions
    {
        public static void CopyTo( this Stream memStream, Stream dest )
        {
            byte[] tempBuff = new byte[ memStream.Length - memStream.Position ];
            memStream.Read( tempBuff, 0, tempBuff.Length );
            dest.Write( tempBuff, 0, tempBuff.Length );
        }

        public static uint PeekUInt32( this MemoryStream memStream )
        {
            byte[] peek = new byte[ 4 ];

            memStream.Read( peek, 0, peek.Length );
            memStream.Seek( -peek.Length, SeekOrigin.Current );

            return BitConverter.ToUInt32( peek, 0 );
        }
    }
}
