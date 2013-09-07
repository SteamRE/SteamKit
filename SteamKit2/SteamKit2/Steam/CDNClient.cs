/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// The CDNClient class is used for downloading game content from the Steam servers.
    /// </summary>
    public sealed class CDNClient : IDisposable
    {
        /// <summary>
        /// Represents a single Steam3 'Steampipe' content server.
        /// </summary>
        public sealed class Server
        {
            /// <summary>
            /// Gets the hostname of the server.
            /// </summary>
            public string Host { get; internal set; }
            /// <summary>
            /// Gets the port of the server.
            /// </summary>
            public int Port { get; internal set; }

            /// <summary>
            /// Gets the type of the server.
            /// </summary>
            public string Type { get; internal set; }


            /// <summary>
            /// Performs an implicit conversion from <see cref="System.Net.IPEndPoint"/> to <see cref="SteamKit2.CDNClient.Server"/>.
            /// </summary>
            /// <param name="endPoint">A IPEndPoint to convert into a <see cref="SteamKit2.CDNClient.Server"/>.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator Server( IPEndPoint endPoint )
            {
                return new Server
                {
                    Host = endPoint.Address.ToString(),
                    Port = endPoint.Port,
                };
            }
        }

        /// <summary>
        /// Represents a single downloaded chunk from a file in a depot.
        /// </summary>
        public sealed class DepotChunk
        {
            /// <summary>
            /// Gets the depot manifest chunk information associated with this chunk.
            /// </summary>
            public DepotManifest.ChunkData ChunkInfo { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether this chunk has been processed. A chunk is processed when the data has been decrypted and decompressed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this chunk has been processed; otherwise, <c>false</c>.
            /// </value>
            public bool IsProcessed { get; internal set; }

            /// <summary>
            /// Gets the underlying data for this chunk.
            /// </summary>
            public byte[] Data { get; internal set; }


            /// <summary>
            /// Processes the specified depot key by decrypting the data with the given depot encryption key, and then by decompressing the data.
            /// If the chunk has already been processed, this function does nothing.
            /// </summary>
            /// <param name="depotKey">The depot decryption key.</param>
            /// <exception cref="System.IO.InvalidDataException">Thrown if the processed data does not match the expected checksum given in it's chunk information.</exception>
            public void Process( byte[] depotKey )
            {
                if ( IsProcessed )
                    return;

                byte[] processedData = CryptoHelper.SymmetricDecrypt( Data, depotKey );
                processedData = ZipUtil.Decompress( processedData );

                byte[] dataCrc = CryptoHelper.AdlerHash( processedData );

                if ( !dataCrc.SequenceEqual( ChunkInfo.Checksum ) )
                    throw new InvalidDataException( "Processed data checksum is incorrect! Downloaded depot chunk is corrupt or invalid/wrong depot key?" );

                Data = processedData;
                IsProcessed = true;
            }
        }


        SteamClient steamClient;

        WebClient webClient;

        byte[] appTicket;
        uint depotId;
        byte[] depotKey;

        byte[] sessionKey;

        Server connectedServer;

        ulong sessionId;
        long reqCounter;


        static CDNClient()
        {
            ServicePointManager.Expect100Continue = false;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CDNClient"/> class.
        /// </summary>
        /// <param name="steamClient">
        /// The <see cref="SteamClient"/> this instance will be associated with.
        /// The SteamClient instance must be connected and logged onto Steam.</param>
        /// <param name="depotId">The DepotID of the depot that will be downloaded.</param>
        /// <param name="appTicket">
        /// The optional appticket for the depot that will be downloaded.
        /// This must be present when connected to steam non-anonymously.
        /// </param>
        /// <param name="depotKey">
        /// The optional depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if neeed) in depot manifests, and processing depot chunks.
        /// </param>
        public CDNClient( SteamClient steamClient, uint depotId, byte[] appTicket = null, byte[] depotKey = null )
        {
            this.steamClient = steamClient;

            this.depotId = depotId;
            this.appTicket = appTicket;
            this.depotKey = depotKey;

            webClient = new WebClient();
        }


        /// <summary>
        /// Fetches a list of content servers.
        /// </summary>
        /// <param name="csServer">
        /// The optional Steam3 content server to fetch the list from.
        /// If this parameter is not specified, a random CS server will be selected.
        /// </param>
        /// <param name="cellId">
        /// The optional CellID used to specify which regional servers should be returned in the list.
        /// If this parameter is not specified, Steam's GeoIP suggested CellID will be used instead.
        /// </param>
        /// <returns>A list of servers.</returns>
        /// <exception cref="System.InvalidOperationException">No Steam CS servers available, or the suggested CellID is unavailable.
        /// Check that the <see cref="SteamClient"/> associated with this <see cref="CDNClient"/> instance is logged onto Steam.
        /// </exception>
        public List<Server> FetchServerList( IPEndPoint csServer = null, uint? cellId = null )
        {
            const int SERVERS_TO_REQUEST = 20;

            DebugLog.Assert( steamClient.IsConnected, "CDNClient", "CMClient is not connected!" );
            DebugLog.Assert( steamClient.CellID != null, "CDNClient", "CMClient is not logged on!" );

            if ( csServer == null )
            {
                // if we're not specifying what CS server we want to fetch a server list from, randomly select a cached CS server
                var csServers = steamClient.GetServersOfType( EServerType.CS );

                if ( csServers.Count == 0 )
                {
                    // steamclient doesn't know about any CS servers yet
                    throw new InvalidOperationException( "No CS servers available!" );
                }

                Random random = new Random();
                csServer = csServers[ random.Next( csServers.Count ) ];
            }

            if ( cellId == null )
            {
                if ( steamClient.CellID == null )
                    throw new InvalidOperationException( "Recommended CellID is not available. CMClient not logged on?" );

                // fallback to recommended cellid
                cellId = steamClient.CellID.Value;
            }

            KeyValue serverKv = DoCommand( csServer, "serverlist", args: string.Format( "{0}/{1}/", cellId, SERVERS_TO_REQUEST ) );

            var serverList = new List<Server>( SERVERS_TO_REQUEST );

            if ( serverKv[ "deferred" ].AsBoolean() )
            {
                return serverList;
            }

            foreach ( var server in serverKv.Children )
            {
                string type = server[ "type" ].AsString();
                string host = server[ "host" ].AsString();

                string[] hostSplits = host.Split( ':' );

                int port = 80;
                if ( hostSplits.Length > 1 )
                {
                    int parsedPort;
                    if ( int.TryParse( hostSplits[ 1 ], out parsedPort ) )
                    {
                        port = parsedPort;
                    }
                }

                serverList.Add( new Server
                {
                    Host = host,
                    Port = port,

                    Type = type,
                } );
            }

            return serverList;
        }


        /// <summary>
        /// Connects and authenticates to the specified content server.
        /// </summary>
        /// <param name="csServer">The content server to connect to.</param>
        /// <exception cref="System.ArgumentNullException">csServer was null.</exception>
        public void Connect( Server csServer )
        {
            DebugLog.Assert( steamClient.IsConnected, "CDNClient", "CMClient is not connected!" );

            if ( csServer == null )
                throw new ArgumentNullException( "csServer" );

            byte[] pubKey = KeyDictionary.GetPublicKey( steamClient.ConnectedUniverse );

            sessionKey = CryptoHelper.GenerateRandomBlock( 32 );

            byte[] cryptedSessKey = null;
            using ( var rsa = new RSACrypto( pubKey ) )
            {
                cryptedSessKey = rsa.Encrypt( sessionKey );
            }

            string data;

            if ( appTicket == null )
            {
                // no appticket, doing anonymous connection
                data = string.Format( "sessionkey={0}&anonymoususer=1&steamid={1}", WebHelpers.UrlEncode( cryptedSessKey ), steamClient.SteamID.ConvertToUInt64() );
            }
            else
            {
                byte[] encryptedAppTicket = CryptoHelper.SymmetricEncrypt( appTicket, sessionKey );
                data = string.Format( "sessionkey={0}&appticket={1}", WebHelpers.UrlEncode( cryptedSessKey ), WebHelpers.UrlEncode( encryptedAppTicket ) );
            }

            KeyValue initKv = DoCommand( csServer, "initsession", data, WebRequestMethods.Http.Post );

            sessionId = ( ulong )initKv[ "sessionid" ].AsLong();
            reqCounter = initKv[ "req-counter" ].AsLong();

            if ( appTicket == null )
            {
                data = string.Format( "depotid={0}", depotId );
            }
            else
            {
                byte[] encryptedAppTicket = CryptoHelper.SymmetricEncrypt( appTicket, sessionKey );
                data = string.Format( "appticket={0}", WebHelpers.UrlEncode( encryptedAppTicket ) );
            }

            DoCommand( csServer, "authdepot", data, WebRequestMethods.Http.Post, true );

            connectedServer = csServer;
        }

        /// <summary>
        /// Downloads the depot manifest specified by the given manifest ID, and optionally decrypts the manifest's filenames if the depot decryption key has been provided.
        /// </summary>
        /// <param name="manifestId">The unique identifier of the manifest to be downloaded.</param>
        /// <returns>A <see cref="DepotManifest"/> instance that contains information about the files present within a depot.</returns>
        public DepotManifest DownloadManifest( ulong manifestId )
        {
            byte[] compressedManifest = DoRawCommand( connectedServer, "depot", doAuth: true, args: string.Format( "{0}/manifest/{1}", depotId, manifestId ) );

            byte[] manifestData = ZipUtil.Decompress( compressedManifest );

            var depotManifest = new DepotManifest( manifestData );

            if ( depotKey != null )
            {
                // if we have the depot key, decrypt the manifest filenames
                depotManifest.DecryptFilenames( depotKey );
            }

            return depotManifest;
        }

        /// <summary>
        /// Downloads the specified depot chunk, and optionally processes the chunk and verifies the checksum if the depot decryption key has been provided.
        /// </summary>
        /// <param name="chunk">
        /// A <see cref="DepotManifest.ChunkData"/> instance that represents the chunk to download.
        /// This value should come from a manifest downloaded with <see cref="CDNClient.DownloadManifest"/>.
        /// </param>
        /// <returns>A <see cref="DepotChunk"/> instance that contains the data for the given chunk.</returns>
        public DepotChunk DownloadDepotChunk( DepotManifest.ChunkData chunk )
        {
            string chunkId = Utils.EncodeHexString( chunk.ChunkID );

            byte[] chunkData = DoRawCommand( connectedServer, "depot", doAuth: true, args: string.Format( "{0}/chunk/{1}", depotId, chunkId ) );

            DebugLog.Assert( chunkData.Length == chunk.CompressedLength, "CDNClient", "Length mismatch after downloading depot chunk!" );

            var depotChunk = new DepotChunk
            {
                ChunkInfo = chunk,
                Data = chunkData,
            };

            if ( depotKey != null )
            {
                // if we have the depot key, we can process the chunk immediately
                depotChunk.Process( depotKey );
            }

            return depotChunk;
        }



        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            webClient.Dispose();
        }


        string BuildCommand( Server server, string command, string args )
        {
            return string.Format( "http://{0}:{1}/{2}/{3}", server.Host, server.Port, command, args );
        }

        byte[] DoRawCommand( Server server, string command, string data = null, string method = WebRequestMethods.Http.Get, bool doAuth = false, string args = "" )
        {
            string url = BuildCommand( server, command, args );

            webClient.Headers.Clear();

            if ( doAuth )
            {
                reqCounter++;

                byte[] shaHash;

                using ( var ms = new MemoryStream() )
                using ( var bw = new BinaryWriter( ms ) )
                {
                    var uri = new Uri( url );

                    bw.Write( sessionId );
                    bw.Write( reqCounter );
                    bw.Write( sessionKey );
                    bw.Write( Encoding.ASCII.GetBytes( uri.AbsolutePath ) );

                    shaHash = CryptoHelper.SHAHash( ms.ToArray() );
                }

                string hexHash = Utils.EncodeHexString( shaHash );
                string authHeader = string.Format( "sessionid={0};req-counter={1};hash={2};", sessionId, reqCounter, hexHash );

                webClient.Headers[ "x-steam-auth" ] = authHeader;
            }

            byte[] resultData = null;

            if ( method == WebRequestMethods.Http.Get )
            {
                resultData = webClient.DownloadData( url );
            }
            else if ( method == WebRequestMethods.Http.Post )
            {
                webClient.Headers[ HttpRequestHeader.ContentType ] = "application/x-www-form-urlencoded";

                resultData = webClient.UploadData( url, Encoding.ASCII.GetBytes( data ) );
            }

            return resultData;
        }
        KeyValue DoCommand( Server server, string command, string data = null, string method = WebRequestMethods.Http.Get, bool doAuth = false, string args = "" )
        {
            byte[] resultData = DoRawCommand( server, command, data, method, doAuth, args );

            var dataKv = new KeyValue();

            using ( MemoryStream ms = new MemoryStream( resultData ) )
            {
                try
                {
                    dataKv.ReadAsText( ms );
                }
                catch ( Exception ex )
                {
                    throw new InvalidDataException( "An internal error occurred while attempting to parse the response from the CS server.", ex );
                }
            }

            return dataKv;
        }

    }
}
