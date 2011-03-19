/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// Represents a client that is capable of connecting to a Steam2 content server.
    /// </summary>
    public sealed class ContentServerClient : ServerClient
    {
        public sealed class Credentials
        {
            // steam2 details
            public byte[] ServerTGT { get; set; }

            // steam3 details
            public ulong SessionToken { get; set; }
            public byte[] AppTicket { get; set; }
        }

        public sealed class File
        {
            public uint FileMode { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentServerClient"/> class.
        /// </summary>
        public ContentServerClient()
        {
        }

        #region Package Server
        /// <summary>
        /// Requests the cell ID of the currently connected content server.
        /// </summary>
        /// <returns>A valid cellid on success, or 0 on failure.</returns>
        public uint GetCellID()
        {
            if ( !this.HandshakeServer( ( EServerType )3 ) )
                return 0; // 0 is the global or error cellid

            TcpPacket packet = new TcpPacket();
            packet.Write( ( uint )2 );

            try
            {
                this.Socket.Send( packet );

                uint cellID = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                return cellID;
            }
            catch
            {
                return 0;
            }
        }

        public bool EnterPackageMode()
        {
            return this.HandshakeServer( ( EServerType )3 );
        }

        public byte[] DownloadPackage( string fileName, uint cellId )
        {
            TcpPacket packet = new TcpPacket();
            packet.Write( ( uint )0 ); // unknown, always 0?
            packet.Write( ( uint )0 ); // unknown, always 0?
            packet.Write( ( uint )fileName.Length );
            packet.Write( fileName );
            packet.Write( cellId );

            this.Socket.Send( packet );

            // length is sent twice, as two uints
            uint len1 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
            uint len2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

            byte[] packageData = this.Socket.Reader.ReadBytes( ( int )len1 );

            return packageData;
        }

        public void ExitPackageMode()
        {
            TcpPacket packet = new TcpPacket();
            packet.Write( ( uint )3 );

            // exit package mode
            this.Socket.Send( packet );
        }
        #endregion

        #region Storage Server

        uint connectionId;
        uint messageId;

        public bool EnterStorageMode( uint cellId )
        {
            bool bRet = this.HandshakeServer( ( EServerType )7 );

            if ( !bRet )
                return false;

            this.connectionId = 0;
            this.messageId = 0;

            bRet = this.SendCommand(
                0,
                cellId
            );

            // can this ever fail?
            byte success = this.Socket.Reader.ReadByte();

            ushort bannerLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt16() );
            byte[] bannerData = this.Socket.Reader.ReadBytes( bannerLen );

            return ( success == 1 );
        }

        public uint OpenStorage( uint depotId, uint depotVersion )
        {
            return this.OpenStorage( depotId, depotVersion, null );
        }
        public uint OpenStorage( uint depotId, uint depotVersion, Credentials credentials )
        {
            bool bRet = false;

            if ( credentials == null )
            {
                bRet = this.SendCommand(
                   9, // open storage
                   connectionId,
                   messageId,
                   depotId,
                   depotVersion
                );
            }
            else
            {
                bRet = this.SendCommand(
                    10, // open storage with login
                    connectionId,
                    messageId,
                    depotId,
                    depotVersion,
                    ( ushort )credentials.ServerTGT.Length,
                    credentials.ServerTGT,
                    credentials.SessionToken,
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
                return UInt32.MaxValue;

            uint storageId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
            uint storageChecksum = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

            this.connectionId++;

            return storageId;
        }
        public void CloseStorage( uint storageId )
        {
            this.SendCommand( 3,
                storageId,
                this.messageId
            );

            this.messageId++;
        }

        public byte[] DownloadManifest( uint storageId )
        {
            bool bRet = this.SendCommand(
                4, // download manifest
                storageId,
                this.messageId
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

            this.messageId++;

            return manifest;
        }
        public byte[] DownloadChecksums( uint storageId )
        {
            bool bRet = this.SendCommand(
                6, // download checksums
                storageId,
                this.messageId
            );

            uint storId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
            uint msgId = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

            // name is a guess
            byte hasChecksums = this.Socket.Reader.ReadByte();

            uint checksumsLen = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );


            uint storId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
            uint msgId2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );
            uint checksumsLen2 = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

            byte[] checksums = this.Socket.Reader.ReadBytes( (int)checksumsLen );

            this.messageId++;

            return checksums;
        }
        public uint[] DownloadDepotUpdate( uint storageId, uint oldVersion )
        {
            bool bRet = this.SendCommand(
                5, // download updates
                storageId,
                this.messageId,
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
            DataStream ds = new DataStream( packet.GetPayload(), true );

            uint[] fileIdList = new uint[ numUpdates ];
            for ( int x = 0 ; x < numUpdates ; ++x )
            {
                fileIdList[ x ] = ds.ReadUInt32();
            }

            this.messageId++;

            return fileIdList;
            
        }
        public File DownloadFile( uint storageId, int fileId )
        {
            this.SendCommand(
                7, // download file
                storageId,
                this.messageId,
                fileId,
                0, // file part, always 0?
                1, // num parts, always 1?
                ( byte )0 // unknown
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

            this.messageId++;

            File cf = new File()
            {
                FileMode = fileMode,
                Data = data,
            };

            return cf;
        }

        public void ExitStorageMode()
        {
            TcpPacket packet = new TcpPacket();
            packet.Write( ( byte )1 );

            this.Socket.Send( packet );
        }

        #endregion
    }
}
