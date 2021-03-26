using System;
using System.IO;

namespace SteamKit2
{
    class VZipDeltaUtil
    {
        private static UInt16 VZipHeader = 0x5A56;
        private static UInt16 VZipFooter = 0x767A;
        private static int HeaderLength = 7;
        private static int FooterLength = 10;

        private static char Version = 'd';


        public static byte[] Decompress( byte[] buffer, byte[] sourceChunkData )
        {
            using ( MemoryStream ms = new MemoryStream( buffer ) )
            using ( BinaryReader reader = new BinaryReader( ms ) )
            {
                if ( reader.ReadUInt16() != VZipHeader )
                {
                    throw new Exception( "Expecting VZipHeader at start of stream" );
                }

                if ( reader.ReadChar() != Version )
                {
                    throw new Exception( "Expecting VZip version 'd'" );
                }

                // This is also the CRC of the chunk
                /* uint secondaryCRC = */ reader.ReadUInt32();

                byte[] properties = reader.ReadBytes( 5 );
                byte[] deltaBuffer = reader.ReadBytes( ( int )ms.Length - HeaderLength - FooterLength - 5 );

                uint outputCRC = reader.ReadUInt32();
                uint sizeDecompressed = reader.ReadUInt32();

                if ( reader.ReadUInt16() != VZipFooter )
                {
                    throw new Exception( "Expecting VZipFooter at end of stream" );
                }

                SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder( allowIllegalStreamStart: true );
                decoder.SetDecoderProperties( properties );

                using ( MemoryStream trainingStream = new MemoryStream( sourceChunkData ) )
                using ( MemoryStream inputStream = new MemoryStream( deltaBuffer ) )
                using ( MemoryStream outStream = new MemoryStream( ( int )sizeDecompressed ) )
                {
                    decoder.Train( trainingStream );
                    decoder.Code( inputStream, outStream, deltaBuffer.Length, sizeDecompressed, null );

                    var outData = outStream.ToArray();
                    if ( Crc32.Compute( outData ) != outputCRC )
                    {
                        throw new InvalidDataException( "CRC does not match decompressed data. VZip data may be corrupted." );
                    }

                    return outData;
                }
            }
        }
    }
}
