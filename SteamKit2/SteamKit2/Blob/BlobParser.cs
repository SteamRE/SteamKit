/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SteamKit2
{
    public class BlobParser
    {
        private const int BlobHeaderLength = 10;
        private const int FieldHeaderLength = 6;
        private const int CompressedHeaderLength = 10;
        private const int EncryptedHeaderLength = 20;

        private static byte[] Key;

        public static void SetKey( byte[] key )
        {
            Key = key;
        }

        public static Blob ParseBlob( byte[] buffer )
        {
            if ( buffer.Length < BlobHeaderLength )
                return null;

            using ( MemoryStream ms = new MemoryStream( buffer ) )
            using ( CachedBinaryReader reader = new CachedBinaryReader( ms ) )
            {
                return ParseBlob( reader );
            }
        }

        private static Blob ParseBlob( CachedBinaryReader reader )
        {
            if ( !reader.CanRead( BlobHeaderLength ) )
                return null;

            EAutoPreprocessCode process;
            ECacheState cachestate;

            Int32 serialized, spare;

            reader.StartTransaction();

            try
            {
                cachestate = ( ECacheState )reader.ReadByte();
                process = ( EAutoPreprocessCode )reader.ReadByte();

                serialized = reader.ReadInt32();
                spare = reader.ReadInt32();

                reader.Commit();
            }
            catch ( Exception e )
            {
                Debug.Write( e );

                reader.Rollback();
                return null;
            }

            serialized -= BlobHeaderLength;

            if ( process == EAutoPreprocessCode.eAutoPreprocessCodeEncrypted )
            {
                serialized -= EncryptedHeaderLength;
            }

            if ( !IsValidCacheState( cachestate ) || !IsValidProcess( process ) ||
                    serialized < 0 || spare < 0 ||
                    !reader.CanRead( serialized + spare ) )
            {
                reader.Rollback();
                return null;
            }

            long curpos = reader.Position;
            Blob blob = null;

            switch ( process )
            {
                case EAutoPreprocessCode.eAutoPreprocessCodePlaintext:
                    {
                        blob = new Blob( cachestate, process );
                        ParseBlobData( reader, blob, serialized, spare );
                    }
                    break;
                case EAutoPreprocessCode.eAutoPreprocessCodeCompressed:
                    {
                        blob = ParseCompressedBlob( reader, serialized );
                    }
                    break;
                case EAutoPreprocessCode.eAutoPreprocessCodeEncrypted:
                    {
                        blob = ParseEncryptedBlob( reader, serialized );
                    }
                    break;
            }

            Debug.Assert( reader.Position == ( curpos + serialized + spare ) );

            return blob;
        }

        private static Blob ParseEncryptedBlob( CachedBinaryReader reader, long serializedSize )
        {
            Int32 decryptedSize;
            byte[] IV, HMAC;

            reader.StartTransaction();

            try
            {
                decryptedSize = reader.ReadInt32();
                IV = reader.ReadBytes( 16 );

                reader.Commit();
            }
            catch ( Exception e )
            {
                Debug.Write( e );

                reader.Rollback();
                return null;
            }

            serializedSize -= EncryptedHeaderLength;
            reader.StartTransaction();

            byte[] ciphertext = reader.ReadBytes( ( int )serializedSize );

            reader.Commit();

            byte[] plaintext = CryptoHelper.AESDecrypt( ciphertext, Key, IV );

            Blob encryptedblob = ParseBlob( plaintext );
            encryptedblob.IV = IV;

            return encryptedblob;
        }

        private static Blob ParseCompressedBlob( CachedBinaryReader reader, long serializedSize )
        {
            Int32 decompressedSize, unknown;
            UInt16 level;

            reader.StartTransaction();

            try
            {
                decompressedSize = reader.ReadInt32();
                unknown = reader.ReadInt32();
                level = reader.ReadUInt16();

                // note: http://www.chiramattel.com/george/blog/2007/09/09/deflatestream-block-length-does-not-match.html
                // need to skip two bytes for implementation mismatch
                reader.ReadBytes( 2 );

                reader.Commit();
            }
            catch ( Exception e )
            {
                Debug.Write( e );

                reader.Rollback();
                return null;
            }

            serializedSize -= CompressedHeaderLength + 2;
            reader.StartTransaction();

            // the DeflateStream reads too much, so we pull out a buffer
            byte[] compressed = reader.ReadBytes( ( int )serializedSize );

            reader.Commit();

            using ( MemoryStream ms = new MemoryStream( compressed ) )
            using ( DeflateStream deflateStream = new DeflateStream( ms, CompressionMode.Decompress ) )
            {
                byte[] inflated = new byte[ decompressedSize ];
                deflateStream.Read( inflated, 0, inflated.Length );

                Blob compressedblob = ParseBlob( inflated );
                compressedblob.CompressionLevel = level;

                return compressedblob;
            }
        }

        private static void ParseBlobData( CachedBinaryReader reader, Blob blob, long serializedSize, int spare )
        {
            bool corruptedCompletion = false;

            while ( serializedSize > FieldHeaderLength && reader.CanRead( FieldHeaderLength ) )
            {
                long curpos = reader.Position;

                Int16 descriptorLength;
                Int32 dataLength;

                byte[] descriptor, data;

                reader.StartTransaction();

                try
                {
                    descriptorLength = reader.ReadInt16();
                    dataLength = reader.ReadInt32();

                    reader.Commit();

                    //Debug.Assert(descriptorLength > 0);
                    //Debug.Assert(dataLength > 0);

                    // when the descriptorLength is 0 assume it's corrupted (the CDR is always corrupted....?)
                    if ( descriptorLength <= 0 /*|| dataLength <= 0*/ || !reader.CanRead( descriptorLength + dataLength ) )
                    {
                        reader.Rollback();

                        corruptedCompletion = true;
                        break;
                    }

                    descriptor = reader.ReadBytes( descriptorLength );

                    reader.Commit();

                    Debug.Assert( descriptor.Length == descriptorLength );
                }
                catch ( Exception e )
                {
                    Debug.Write( e );

                    reader.Rollback();
                    return;
                }

                Blob innerBlob = BlobParser.ParseBlob( reader );
                if ( innerBlob == null )
                {
                    reader.StartTransaction();

                    data = reader.ReadBytes( dataLength );

                    reader.Commit();

                    blob.AddField( new BlobField( descriptor, data ) );
                }
                else
                {
                    blob.AddField( new BlobField( descriptor, innerBlob ) );
                }

                serializedSize -= ( reader.Position - curpos );
            }

            reader.StartTransaction();

            blob.Spare = reader.ReadBytes( spare );

            reader.Commit();

            if ( corruptedCompletion )
            {
                Debug.WriteLine( "Had to complete corrupted blob at " + reader.Position + " (" + serializedSize + ") remaining" );
                reader.StartTransaction();
                reader.ReadBytes( ( int )serializedSize );
                reader.Commit();
                serializedSize = 0;
            }

            Debug.Assert( serializedSize == 0 );
        }


        static bool IsValidCacheState( ECacheState cachestate )
        {
            return ( cachestate >= ECacheState.eCacheEmpty && cachestate <= ECacheState.eCachePtrIsCopyOnWritePlaintextVersion );
        }
        static bool IsValidProcess( EAutoPreprocessCode process )
        {
            return ( process == EAutoPreprocessCode.eAutoPreprocessCodePlaintext ||
                    process == EAutoPreprocessCode.eAutoPreprocessCodeCompressed ||
                    process == EAutoPreprocessCode.eAutoPreprocessCodeEncrypted );
        }

    }
}