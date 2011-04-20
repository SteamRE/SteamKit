using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// Represents an exception that can occur when doing Steam2 actions.
    /// </summary>
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
            public byte[] ServerTGT { get; set; }

            // steam3 details
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
        /// <returns>A new StorageSession object for the session.</returns>
        public StorageSession OpenStorage( uint depotId, uint depotVersion, uint cellId, Credentials credentials )
        {
            bool bRet = this.HandshakeServer( ( EServerType )7 );

            if ( !bRet )
                throw new Steam2Exception( "Storage handshake with content server failed" );

            bRet = this.SendCommand(
                0, // open storage
                cellId
            );

            byte success = this.Socket.Reader.ReadByte();

            if ( success == 0 )
                throw new Steam2Exception( string.Format( "Unable to open storage depot for cellid {0}", cellId ) );

            ushort bannerLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt16() );
            byte[] bannerData = this.Socket.Reader.ReadBytes( bannerLen );

            return new StorageSession( this, depotId, depotVersion, credentials );
        }
        /// <summary>
        /// Opens a storage session with the storage server.
        /// </summary>
        /// <param name="depotId">The depot id.</param>
        /// <param name="depotVersion">The depot version.</param>
        /// <param name="cellId">The cell id.</param>
        /// <returns>A new StorageSession object for the session.</returns>
        public StorageSession OpenStorage( uint depotId, uint depotVersion, uint cellId )
        {
            return OpenStorage( depotId, depotVersion, cellId, null );
        }
        /// <summary>
        /// Opens a storage session with the storage server.
        /// </summary>
        /// <param name="depotId">The depot id.</param>
        /// <param name="depotVersion">The depot version.</param>
        /// <returns>A new StorageSession object for the session.</returns>
        public StorageSession OpenStorage( uint depotId, uint depotVersion )
        {
            return OpenStorage( depotId, depotVersion, 0 );
        }

        /// <summary>
        /// Opens a package session with the package server.
        /// </summary>
        /// <param name="cellId">The cell id.</param>
        /// <returns>A new PackageSession object for the session.</returns>
        public PackageSession OpenPackage( uint cellId )
        {
            bool bRet = this.HandshakeServer( ( EServerType )3 );

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
            if ( !this.HandshakeServer( ( EServerType )3 ) )
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
            public enum DownloadPriority : byte
            {
                Low = 0,
                Medium = 1,
                High = 2,
            }

            public sealed class File
            {
                public enum FileMode
                {
                    Compressed = 1,
                    EncryptedAndCompressed = 2,
                    Encrypted = 3,
                }

                public byte[] Data { get; set; }
                public FileMode Mode { get; set; }
            }


            public uint DepotID { get; private set; }
            public uint DepotVersion { get; private set; }

            public uint ConnectionID { get; private set; }
            public uint MessageID { get; private set; }

            public uint StorageID { get; private set; }


            ContentServerClient client;
            TcpSocket Socket { get { return client.Socket; } }


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
                    bRet = this.SendCommand(
                        10, // open storage with login
                        ConnectionID,
                        MessageID,
                        depotId,
                        depotVersion,
                        ( ushort )credentials.ServerTGT.Length,
                        credentials.ServerTGT,
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

            public void Dispose()
            {
                this.SendCommand( 3,
                    this.StorageID,
                    this.MessageID
                );
            }


            public Manifest DownloadManifest()
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

                uint manifestLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                uint storId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint manifestLen2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                byte[] manifest = this.Socket.Reader.ReadBytes( ( int )manifestLen );

                this.MessageID++;

                return new Manifest( manifest );
            }
            public byte[] DownloadChecksums()
            {
                bool bRet = this.SendCommand(
                    6, // download checksums
                    this.StorageID,
                    this.MessageID
                );

                uint storId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                // name is a guess
                byte hasChecksums = this.Socket.Reader.ReadByte();

                uint checksumsLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );


                uint storId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint msgId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
                uint checksumsLen2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                byte[] checksums = this.Socket.Reader.ReadBytes( ( int )checksumsLen );

                this.MessageID++;

                return checksums;
            }
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

            public File DownloadFileParts( Manifest.Node file, uint filePart, uint numParts, DownloadPriority priority )
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
                uint fileMode = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

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

                    byte[] chunk = this.Socket.Reader.ReadBytes( ( int )chunkLen );
                    ms.Write( chunk, 0, chunk.Length );
                }

                byte[] data = ms.ToArray();

                this.MessageID++;

                File outputFile = new File()
                {
                    Mode = ( File.FileMode )fileMode,
                    Data = data,
                };

                return outputFile;
            }
            public File DownloadFileParts( Manifest.Node file, uint filePart, uint numParts )
            {
                return DownloadFileParts( file, filePart, numParts, DownloadPriority.Low );
            }

            public File DownloadFile( Manifest.Node file, DownloadPriority priority )
            {
                const uint MaxParts = 16;

                uint numFileparts = ( uint )Math.Ceiling( ( float )file.SizeOrCount / ( float )file.Parent.BlockSize );
                uint numChunks = ( uint )Math.Ceiling( ( float )numFileparts / ( float )MaxParts );

                MemoryStream ms = new MemoryStream();
                File.FileMode mode = File.FileMode.Compressed;

                for ( uint x = 0 ; x < numChunks ; ++x )
                {
                    File filePart = DownloadFileParts( file, x * MaxParts, MaxParts, priority );
                    mode = filePart.Mode;

                    ms.Write( filePart.Data, 0, filePart.Data.Length );
                }

                return new File()
                {
                    Data = ms.ToArray(),
                    Mode = mode,
                };
            }
            public File DownloadFile( Manifest.Node file )
            {
                return DownloadFile( file, DownloadPriority.Low );
            }


            bool SendCommand( byte cmd, params object[] args )
            {
                return this.client.SendCommand( cmd, args );
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

            public void Dispose()
            {
                TcpPacket packet = new TcpPacket();
                packet.Write( ( uint )3 );

                // exit package mode
                this.Socket.Send( packet );
            }


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
