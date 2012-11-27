using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace SteamKit2
{
    /// <summary>
    /// Represents an exception that can occur when doing Steam2 actions.
    /// </summary>
    [Serializable]
    public class Steam2Exception : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Steam2Exception"/> class.
        /// </summary>
        public Steam2Exception()
            : base()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Steam2Exception"/> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        public Steam2Exception( string msg )
            : base( msg )
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Steam2Exception"/> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public Steam2Exception( string msg, Exception innerException )
            : base( msg, innerException )
        {
        }

    }

    /// <summary>
    /// Represents a client that is capable of connecting to a Steam2 content server.
    /// </summary>
    public sealed class ContentServerClient : ServerClient
    {

        /// <summary>
        /// These credentials must be supplied when attempting to open a storage session for a depot which requires it.
        /// </summary>
        public sealed class Credentials
        {
            /// <summary>
            /// Gets or sets the Steam2 ServerTGT.
            /// </summary>
            /// <value>The ServerTGT.</value>
            public Steam2Ticket Steam2Ticket { get; set; }

            /// <summary>
            /// Gets or sets the Steam3 session token.
            /// </summary>
            /// <value>The session token.</value>
            public ulong SessionToken { get; set; }
            /// <summary>
            /// Gets or sets the Steam3 app ticket for the app being requested.
            /// </summary>
            /// <value>The app ticket.</value>
            public byte[] AppTicket { get; set; }
        }


        /// <summary>
        /// Opens a storage session with the storage server.
        /// </summary>
        /// <param name="depotId">The depot id.</param>
        /// <param name="depotVersion">The depot version.</param>
        /// <param name="cellId">The cell id.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="doHandshake">Whether or not to send the handshake and reopen cell</param>
        /// <returns>A new StorageSession object for the session.</returns>
        public StorageSession OpenStorage( uint depotId, uint depotVersion, uint cellId, Credentials credentials, bool doHandshake = true )
        {
            if (doHandshake)
            {
                bool bRet = this.HandshakeServer((ESteam2ServerType)7);

                if (!bRet)
                    throw new Steam2Exception("Storage handshake with content server failed");

                bRet = this.SendCommand(
                    0, // open storage
                    cellId
                );

                byte success = this.Socket.Reader.ReadByte();

                if (success == 0)
                    throw new Steam2Exception(string.Format("Unable to open storage depot for cellid {0}", cellId));


                ushort bannerLen = NetHelpers.EndianSwap(this.Socket.Reader.ReadUInt16());
                byte[] bannerData = this.Socket.Reader.ReadBytes(bannerLen);
            }
			
            return new StorageSession( this, depotId, depotVersion, credentials );
        }
        /// <summary>
        /// Opens a storage session with the storage server.
        /// </summary>
        /// <param name="depotId">The depot id.</param>
        /// <param name="depotVersion">The depot version.</param>
        /// <param name="cellId">The cell id.</param>
		/// <param name="doHandshake">Whether or not to send the handshake and reopen cell</param>
        /// <returns>A new StorageSession object for the session.</returns>
        public StorageSession OpenStorage( uint depotId, uint depotVersion, uint cellId, bool doHandshake = true )
        {
            return OpenStorage( depotId, depotVersion, cellId, null, doHandshake );
        }
        /// <summary>
        /// Opens a storage session with the storage server.
        /// </summary>
        /// <param name="depotId">The depot id.</param>
        /// <param name="depotVersion">The depot version.</param>
        /// <param name="doHandshake">Whether or not to send the handshake and reopen cell</param>
        /// <returns>A new StorageSession object for the session.</returns>
        public StorageSession OpenStorage( uint depotId, uint depotVersion, bool doHandshake = true )
        {
            return OpenStorage( depotId, depotVersion, 0, doHandshake );
        }

        /// <summary>
        /// Opens a package session with the package server.
        /// </summary>
        /// <param name="cellId">The cell id.</param>
        /// <returns>A new PackageSession object for the session.</returns>
        public PackageSession OpenPackage( uint cellId )
        {
            bool bRet = this.HandshakeServer( ( ESteam2ServerType )3 );

            if ( !bRet )
                throw new Steam2Exception( "Package handshake with content server failed" );

            return new PackageSession( this, cellId );
        }

        /// <summary>
        /// Requests the cell ID of the currently connected content server.
        /// </summary>
        /// <returns>The cell ID of the server.</returns>
        public uint GetCellID()
        {
            if ( !this.HandshakeServer( ( ESteam2ServerType )3 ) )
                throw new Steam2Exception( "Package handshake with content server failed" );

            TcpPacket packet = new TcpPacket();
            packet.Write( ( uint )2 );


            try
            {
                this.Socket.Send( packet );

                uint cellId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                return cellId;
            }
            catch ( Exception ex )
            {
                throw new Steam2Exception( "Unable to request cell id", ex );
            }
        }


        /// <summary>
        /// This represents a storage session with a storage server, used to download game content.
        /// </summary>
        public sealed class StorageSession : IDisposable
        {
            /// <summary>
            /// The priority setting for a file download.
            /// </summary>
            public enum DownloadPriority : byte
            {
                /// <summary>
                /// Low priority.
                /// </summary>
                Low = 0,
                /// <summary>
                /// Medium priority.
                /// </summary>
                Medium = 1,
                /// <summary>
                /// High priority.
                /// </summary>
                High = 2,
            }

            /// <summary>
            /// Represents the state of a file within a depot.
            /// </summary>
            public enum FileMode
            {
                /// <summary>
                /// No special handling is required.
                /// </summary>
                None = 0,
                /// <summary>
                /// This file is compressed.
                /// </summary>
                Compressed = 1,
                /// <summary>
                /// This file is compressed and encrypted.
                /// </summary>
                EncryptedAndCompressed = 2,
                /// <summary>
                /// This file is encrypted.
                /// </summary>
                Encrypted = 3,
            }


            /// <summary>
            /// Gets the depot ID this session instance is for.
            /// </summary>
            public uint DepotID { get; private set; }
            /// <summary>
            /// Gets the depot version this session instance is for.
            /// </summary>
            public uint DepotVersion { get; private set; }

            uint ConnectionID;
            uint MessageID;

            uint StorageID;


            ContentServerClient client;
            TcpSocket Socket { get { return client.Socket; } }

            static readonly byte[] aesIV = new byte[ 16 ];

            internal StorageSession( ContentServerClient cli, uint depotId, uint depotVersion, Credentials credentials )
            {
                this.DepotID = depotId;
                this.DepotVersion = depotVersion;

                this.client = cli;


                bool bRet = false;

                if ( credentials == null )
                {
                    bRet = this.SendCommand(
                        9, // open storage
                        ConnectionID,
                        MessageID,
                        depotId,
                        depotVersion
                    );
                }
                else
                {
                    byte[] serverTgt = credentials.Steam2Ticket.Entries[ 14 ].Data; // god help this never change

                    bRet = this.SendCommand(
                        10, // open storage with login
                        ConnectionID,
                        MessageID,
                        depotId,
                        depotVersion,
                        ( ushort )serverTgt.Length,
                        serverTgt,
                        NetHelpers.EndianSwap( credentials.SessionToken ),
                        ( byte )credentials.AppTicket.Length,
                        credentials.AppTicket
                    );
                }

                // the server sends us back the connection and message ids
                // the client probably performs a sanity check?
                uint connId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                byte hasDepot = this.Socket.Reader.ReadByte();

                // the server gives us 0x1 if the depot doesn't exist or requires authentication
                if ( hasDepot != 0 )
                    throw new Steam2Exception( "Content server does not have depot, or valid credentials were not given" );

                StorageID = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint storageChecksum = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                this.ConnectionID++;

            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.SendCommand( 3,
                    this.StorageID,
                    this.MessageID
                );
            }


            /// <summary>
            /// Downloads the <see cref="Steam2Manifest"/> which contains metadata representing the files within the depot.
            /// </summary>
            /// <returns></returns>
            public Steam2Manifest DownloadManifest()
            {
                bool bRet = this.SendCommand(
                    4, // download manifest
                    this.StorageID,
                    this.MessageID
                );


                uint storId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                // name is a guess
                byte hasManifest = this.Socket.Reader.ReadByte();

                uint manifestLength = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                byte[] manifest = new byte[ manifestLength ];

                uint manifestChunksToRead = manifestLength;
                do
                {
                    uint chunkStorID = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint chunkMsgID = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint chunkLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                    chunkLen = Math.Min( chunkLen, manifestChunksToRead );
                    uint toRead = chunkLen;

                    while ( toRead > 0 )
                    {
                        uint socketRead = ( uint )this.Socket.Reader.Read( manifest, ( int )( ( manifestLength - manifestChunksToRead ) + ( chunkLen - toRead ) ), ( int )toRead );
                        toRead = toRead - socketRead;
                    }

                    manifestChunksToRead = manifestChunksToRead - chunkLen;
                } while ( manifestChunksToRead > 0 );

                this.MessageID++;

                return new Steam2Manifest( manifest );
            }
            /// <summary>
            /// Downloads the <see cref="Steam2ChecksumData"/> for this depot.
            /// </summary>
            /// <returns></returns>
            public Steam2ChecksumData DownloadChecksums()
            {
                bool bRet = this.SendCommand(
                    6, // download checksums
                    this.StorageID,
                    this.MessageID
                );

                uint storId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                // name is a(n incorrect) guess
                byte hasChecksums = this.Socket.Reader.ReadByte();

                uint checksumsLength = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                byte[] checksumData = new byte[ checksumsLength ];

                uint checksumChunksToRead = checksumsLength;
                do
                {
                    uint chunkStorID = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint chunkMsgID = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint chunkLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                    chunkLen = Math.Min( chunkLen, checksumChunksToRead );
                    uint toRead = chunkLen;

                    while ( toRead > 0 )
                    {
                        uint socketRead = ( uint )this.Socket.Reader.Read( checksumData, ( int )( ( checksumsLength - checksumChunksToRead ) + ( chunkLen - toRead ) ), ( int )toRead );
                        toRead = toRead - socketRead;
                    }

                    checksumChunksToRead = checksumChunksToRead - chunkLen;
                } while ( checksumChunksToRead > 0 );

                this.MessageID++;

                return new Steam2ChecksumData( checksumData );
            }
            /// <summary>
            /// Downloads a list of updated FileIDs since the given version.
            /// </summary>
            /// <param name="oldVersion">The old version to compare to.</param>
            /// <returns>A list of FileIDs that have been updated.</returns>
            public uint[] DownloadUpdates( uint oldVersion )
            {
                bool bRet = this.SendCommand(
                    5, // download updates
                    this.StorageID,
                    this.MessageID,
                    oldVersion
                );

                uint storId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                byte updateState = this.Socket.Reader.ReadByte();

                uint numUpdates = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                if ( numUpdates == 0 )
                    return null;

                uint storId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                TcpPacket packet = this.Socket.ReceivePacket();
                DataStream ds = new DataStream( packet.GetPayload() );

                uint[] fileIdList = new uint[ numUpdates ];
                for ( int x = 0 ; x < numUpdates ; ++x )
                {
                    fileIdList[ x ] = ds.ReadUInt32();
                }

                this.MessageID++;

                return fileIdList;
            }

            /// <summary>
            /// Downloads a specific file from the Steam servers.
            /// </summary>
            /// <param name="file">The file to download, given from the manifest.</param>
            /// <param name="priority">The download priority.</param>
            /// <param name="cryptKey">The AES encryption key used for any encrypted files.</param>
            /// <returns>A byte array representing the file.</returns>
            public byte[] DownloadFile( Steam2Manifest.Node file, DownloadPriority priority = DownloadPriority.Low, byte[] cryptKey = null )
            {
                if ( ( file.Attributes & Steam2Manifest.Node.Attribs.EncryptedFile ) != 0 && cryptKey == null )
                {
                    throw new Steam2Exception( string.Format( "AES encryption key required for file: {0}", file.FullName ) );
                }

                const uint MaxParts = 16;

                uint numFileparts = ( uint )Math.Ceiling( ( float )file.SizeOrCount / ( float )file.Parent.BlockSize );
                uint numChunks = ( uint )Math.Ceiling( ( float )numFileparts / ( float )MaxParts );

                MemoryStream ms = new MemoryStream();

                for ( uint x = 0 ; x < numChunks ; ++x )
                {
                    byte[] filePart = DownloadFileParts( file, x * MaxParts, MaxParts, priority, cryptKey );

                    ms.Write( filePart, 0, filePart.Length );
                }

                return ms.ToArray();
            }

            byte[] DownloadFileParts( Steam2Manifest.Node file, uint filePart, uint numParts, DownloadPriority priority = DownloadPriority.Low, byte[] cryptKey = null )
            {
                this.SendCommand(
                    7, // download file
                    this.StorageID,
                    this.MessageID,
                    file.FileID,
                    filePart,
                    numParts,
                    ( byte )priority
                );

                uint storId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                byte hasFile = this.Socket.Reader.ReadByte();

                uint numChunks = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint fileModeValue = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                FileMode fileMode = ( FileMode )fileModeValue;

                MemoryStream ms = new MemoryStream();
                for ( int x = 0 ; x < numChunks ; ++x )
                {
                    uint storId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint msgId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint chunkLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                    uint storId3 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint msgId3 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                    uint chunkLen2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                    if ( chunkLen == 0 )
                        continue;

                    byte[] chunk = null;
                    int len = 0;

                    if ( fileMode == FileMode.Compressed )
                    {
                        chunk = this.Socket.Reader.ReadBytes( ( int )chunkLen );
                        len = DecompressFileChunk( ref chunk, ( int )file.Parent.BlockSize );
                    }
                    else if ( fileMode == FileMode.Encrypted )
                    {
                        len = DecryptFileChunk( out chunk, ( int )chunkLen, cryptKey );
                    }
                    else if ( fileMode == FileMode.EncryptedAndCompressed )
                    {
                        // Account for 2 integers before the encrypted data
                        chunkLen -= 8;

                        // Skip first integer (length of the encrypted data)
                        this.Socket.Reader.ReadInt32();
                        // Length of decrypted and decompressed data
                        int plainLen = this.Socket.Reader.ReadInt32();

                        DecryptFileChunk( out chunk, ( int )chunkLen, cryptKey );
                        len = DecompressFileChunk( ref chunk, plainLen );
                    }
                    else if ( fileMode == FileMode.None )
                    {
                        chunk = this.Socket.Reader.ReadBytes( ( int )chunkLen );
                        len = chunk.Length;
                    }

                    ms.Write( chunk, 0, len );
                }

                byte[] data = ms.ToArray();

                this.MessageID++;

                return data;
            }


            bool SendCommand( byte cmd, params object[] args )
            {
                return this.client.SendCommand( cmd, args );
            }

            static int DecompressFileChunk( ref byte[] chunk, int blockSize )
            {
                using ( MemoryStream chunkStream = new MemoryStream( chunk ) )
                using ( DeflateStream ds = new DeflateStream( chunkStream, CompressionMode.Decompress ) )
                {
                    // skip zlib header
                    chunkStream.Seek( 2, SeekOrigin.Begin );

                    byte[] decomp = new byte[ blockSize ];
                    int len = ds.Read( decomp, 0, decomp.Length );

                    chunk = decomp;
                    return len;
                }
            }
            int DecryptFileChunk( out byte[] chunk, int chunkLen, byte[] cryptKey )
            {
                // Round up to nearest AES block size (16 bytes)
                int decryptLen = ( chunkLen + 15 ) & ~15;
                chunk = new byte[ decryptLen ];

                int toRead = chunkLen;
                while ( toRead > 0 )
                {
                    toRead -= this.Socket.Reader.Read( chunk, chunkLen - toRead, toRead );
                }

                using ( var chunkStream = new MemoryStream( chunk ) )
                using ( var aes = new RijndaelManaged() )
                {
                    aes.Mode = CipherMode.CFB;
                    aes.BlockSize = aes.KeySize = 128;
                    aes.Padding = PaddingMode.None;

                    using ( var aesTransform = aes.CreateDecryptor( cryptKey, aesIV ) )
                    using ( var ds = new CryptoStream( chunkStream, aesTransform, CryptoStreamMode.Read ) )
                    {
                        byte[] decrypt = new byte[ chunkLen ];
                        int len = ds.Read( decrypt, 0, decrypt.Length );

                        chunk = decrypt;
                        return len;
                    }
                }
            }
        }

        /// <summary>
        /// This represents a storage session with a package server, used to download client updates.
        /// </summary>
        public sealed class PackageSession : IDisposable
        {
            /// <summary>
            /// Gets or sets the cell ID.
            /// </summary>
            /// <value>The cell ID.</value>
            public uint CellID { get; private set; }

            ContentServerClient client;
            TcpSocket Socket { get { return client.Socket; } }


            internal PackageSession( ContentServerClient cli, uint cellId )
            {
                this.client = cli;
                this.CellID = cellId;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                TcpPacket packet = new TcpPacket();
                packet.Write( ( uint )3 );

                // exit package mode
                this.Socket.Send( packet );
            }


            /// <summary>
            /// Downloads the specified package file.
            /// </summary>
            /// <param name="fileName">Name of the file.</param>
            /// <returns>A byte array representing the file.</returns>
            public byte[] DownloadPackage( string fileName )
            {
                TcpPacket packet = new TcpPacket();
                packet.Write( ( uint )0 ); // unknown, always 0?
                packet.Write( ( uint )0 ); // unknown, always 0?
                packet.Write( ( uint )fileName.Length );
                packet.Write( fileName );
                packet.Write( this.CellID );

                this.Socket.Send( packet );

                // length is sent twice, as two uints
                uint len1 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint len2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                byte[] packageData = this.Socket.Reader.ReadBytes( ( int )len1 );

                return packageData;
            }
        }

    }
}