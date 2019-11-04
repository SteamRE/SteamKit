/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            /// The protocol used to connect to this server
            /// </summary>
            public enum ConnectionProtocol
            {
                /// <summary>
                /// Server does not advertise HTTPS support, connect over HTTP
                /// </summary>
                HTTP = 0,
                /// <summary>
                /// Server advertises it supports HTTPS, connection made over HTTPS
                /// </summary>
                HTTPS = 1
            }

            /// <summary>
            /// Gets the supported connection protocol of the server.
            /// </summary>
            public ConnectionProtocol Protocol { get; internal set; }
            /// <summary>
            /// Gets the hostname of the server.
            /// </summary>
            public string? Host { get; internal set; }
            /// <summary>
            /// Gets the virtual hostname of the server.
            /// </summary>
            public string? VHost { get; internal set; }
            /// <summary>
            /// Gets the port of the server.
            /// </summary>
            public int Port { get; internal set; }

            /// <summary>
            /// Gets the type of the server.
            /// </summary>
            public string? Type { get; internal set; }

            /// <summary>
            /// Gets the SourceID this server belongs to.
            /// </summary>
            public int SourceID { get; internal set; }

            /// <summary>
            /// Gets the CellID this server belongs to.
            /// </summary>
            public uint CellID { get; internal set; }

            /// <summary>
            /// Gets the load value associated with this server.
            /// </summary>
            public int Load { get; internal set; }
            /// <summary>
            /// Gets the weighted load.
            /// </summary>
            public int WeightedLoad { get; internal set; }
            /// <summary>
            /// Gets the number of entries this server is worth.
            /// </summary>
            public int NumEntries { get; internal set; }

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
                    Protocol = endPoint.Port == 443 ? ConnectionProtocol.HTTPS : ConnectionProtocol.HTTP,
                    Host = endPoint.Address.ToString(),
                    VHost = endPoint.Address.ToString(),
                    Port = endPoint.Port,
                };
            }

            /// <summary>
            /// Performs an implicit conversion from <see cref="System.Net.DnsEndPoint"/> to <see cref="SteamKit2.CDNClient.Server"/>.
            /// </summary>
            /// <param name="endPoint">A DnsEndPoint to convert into a <see cref="SteamKit2.CDNClient.Server"/>.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator Server( DnsEndPoint endPoint )
            {
                return new Server
                {
                    Protocol = endPoint.Port == 443 ? ConnectionProtocol.HTTPS : ConnectionProtocol.HTTP,
                    Host = endPoint.Host,
                    VHost = endPoint.Host,
                    Port = endPoint.Port,
                };
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this server.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this server.
            /// </returns>
            public override string ToString()
            {
                return string.Format( "{0}:{1} ({2})", Host, Port, Type );
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
            public DepotManifest.ChunkData ChunkInfo { get; }

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
            public byte[] Data { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DepotChunk"/> class.
            /// </summary>
            /// <param name="info">The manifest chunk information associated with this chunk.</param>
            /// <param name="data">The underlying data for this chunk.</param>
            public DepotChunk(DepotManifest.ChunkData info, byte[] data)
            {
                if ( info is null )
                {
                    throw new ArgumentNullException( nameof( info ) );
                }

                if ( data is null )
                {
                    throw new ArgumentNullException( nameof( data ) );
                }

                ChunkInfo = info;
                Data = data;
            }

            /// <summary>
            /// Processes the specified depot key by decrypting the data with the given depot encryption key, and then by decompressing the data.
            /// If the chunk has already been processed, this function does nothing.
            /// </summary>
            /// <param name="depotKey">The depot decryption key.</param>
            /// <exception cref="System.IO.InvalidDataException">Thrown if the processed data does not match the expected checksum given in it's chunk information.</exception>
            public void Process( byte[] depotKey )
            {
                if ( depotKey == null )
                {
                    throw new ArgumentNullException( nameof(depotKey) );
                }

                if ( IsProcessed )
                    return;

                byte[] processedData = CryptoHelper.SymmetricDecrypt( Data, depotKey );

                if ( processedData.Length > 1 &&  processedData[0] == 'V' && processedData[1] == 'Z' )
                {
                    processedData = VZipUtil.Decompress( processedData );
                }
                else
                {
                    processedData = ZipUtil.Decompress( processedData );
                }

                byte[] dataCrc = CryptoHelper.AdlerHash( processedData );

                if ( !dataCrc.SequenceEqual( ChunkInfo.Checksum ) )
                    throw new InvalidDataException( "Processed data checksum is incorrect! Downloaded depot chunk is corrupt or invalid/wrong depot key?" );

                Data = processedData;
                IsProcessed = true;
            }
        }


        HttpClient httpClient;

        ConcurrentDictionary<uint, byte[]?> depotKeys;
        ConcurrentDictionary<uint, string?> depotCdnAuthKeys;

        /// <summary>
        /// Default timeout to use when making requests
        /// </summary>
        public static TimeSpan RequestTimeout = TimeSpan.FromSeconds( 10 );


        /// <summary>
        /// Initializes a new instance of the <see cref="CDNClient"/> class.
        /// </summary>
        /// <param name="steamClient">
        /// The <see cref="SteamClient"/> this instance will be associated with.
        /// The SteamClient instance must be connected and logged onto Steam.</param>
        public CDNClient( SteamClient steamClient )
        {
            if ( steamClient == null )
            {
                throw new ArgumentNullException( nameof(steamClient) );
            }

            this.httpClient = steamClient.Configuration.HttpClientFactory();
            this.depotKeys = new ConcurrentDictionary<uint, byte[]?>();
            this.depotCdnAuthKeys = new ConcurrentDictionary<uint, string?>();
        }


        /// <summary>
        /// Authenticate a CDNClient to a depot.
        /// </summary>
        /// <param name="depotid">The id of the depot being accessed.</param>
        /// <param name="depotKey">
        /// The optional depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        public void AuthenticateDepot( uint depotid, byte[]? depotKey = null, string? cdnAuthToken = null )
        {
            depotKeys[depotid] = depotKey;
            depotCdnAuthKeys[depotid] = cdnAuthToken;
        }

        /// <summary>
        /// Downloads the depot manifest specified by the given manifest ID, and optionally decrypts the manifest's filenames if the depot decryption key has been provided.
        /// </summary>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="manifestId">The unique identifier of the manifest to be downloaded.</param>
        /// <param name="server">CDN server to download from.</param>
        /// <returns>A <see cref="DepotManifest"/> instance that contains information about the files present within a depot.</returns>
        /// <exception cref="System.ArgumentNullException"><see ref="server"/> was null.</exception>
        /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
        /// <exception cref="SteamKitWebRequestException">A network error occurred when performing the request.</exception>
        public async Task<DepotManifest> DownloadManifestAsync( uint depotId, ulong manifestId, Server server )
        {
            if ( server is null )
            {
                throw new ArgumentNullException( nameof( server ) );
            }

            depotCdnAuthKeys.TryGetValue( depotId, out var cdnToken );
            depotKeys.TryGetValue( depotId, out var depotKey );

            return await DownloadManifestAsync( depotId, manifestId, server, cdnToken, depotKey ).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads the depot manifest specified by the given manifest ID, and optionally decrypts the manifest's filenames if the depot decryption key has been provided.
        /// </summary>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="manifestId">The unique identifier of the manifest to be downloaded.</param>
        /// <param name="host">CDN hostname.</param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        /// <param name="depotKey">
        /// The depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <returns>A <see cref="DepotManifest"/> instance that contains information about the files present within a depot.</returns>
        /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
        /// <exception cref="SteamKitWebRequestException">A network error occurred when performing the request.</exception>
        public async Task<DepotManifest> DownloadManifestAsync( uint depotId, ulong manifestId, string host, string cdnAuthToken, byte[]? depotKey = null )
        {
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = host,
                VHost = host,
                Port = 80
            };

            return await DownloadManifestAsync( depotId, manifestId, server, cdnAuthToken, depotKey ).ConfigureAwait( false );
        }

        /// <summary>
        /// Downloads the depot manifest specified by the given manifest ID, and optionally decrypts the manifest's filenames if the depot decryption key has been provided.
        /// </summary>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="manifestId">The unique identifier of the manifest to be downloaded.</param>
        /// <param name="server">The content server to connect to.</param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        /// <param name="depotKey">
        /// The depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <returns>A <see cref="DepotManifest"/> instance that contains information about the files present within a depot.</returns>
        /// <exception cref="System.ArgumentNullException"><see ref="server"/> was null.</exception>
        /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
        /// <exception cref="SteamKitWebRequestException">A network error occurred when performing the request.</exception>
        public async Task<DepotManifest> DownloadManifestAsync( uint depotId, ulong manifestId, Server server, string? cdnAuthToken, byte[]? depotKey )
        {
            if ( server == null )
            {
                throw new ArgumentNullException( nameof( server ) );
            }

            var manifestData = await DoRawCommandAsync( server, string.Format( "depot/{0}/manifest/{1}/5", depotId, manifestId ), cdnAuthToken ).ConfigureAwait( false );

            manifestData = ZipUtil.Decompress( manifestData );

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
        /// <remarks>
        /// This function will also validate the length of the downloaded chunk with the value of <see cref="DepotManifest.ChunkData.CompressedLength"/>,
        /// if it has been assigned a value.
        /// </remarks>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="chunk">
        /// A <see cref="DepotManifest.ChunkData"/> instance that represents the chunk to download.
        /// This value should come from a manifest downloaded with <see cref="o:DownloadManifestAsync"/>.
        /// </param>
        /// <param name="server">CDN server to download from.</param>
        /// <returns>A <see cref="DepotChunk"/> instance that contains the data for the given chunk.</returns>
        /// <exception cref="System.ArgumentNullException">chunk's <see cref="DepotManifest.ChunkData.ChunkID"/> or <see ref="connectedServer"/> was null.</exception>
        /// <exception cref="System.IO.InvalidDataException">Thrown if the downloaded data does not match the expected length.</exception>
        /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
        /// <exception cref="SteamKitWebRequestException">A network error occurred when performing the request.</exception>
        public async Task<DepotChunk> DownloadDepotChunkAsync( uint depotId, DepotManifest.ChunkData chunk, Server server )
        {
            if ( server is null )
            {
                throw new ArgumentNullException( nameof( server ) );
            }

            depotCdnAuthKeys.TryGetValue( depotId, out var cdnToken );
            depotKeys.TryGetValue( depotId, out var depotKey );

            return await DownloadDepotChunkAsync( depotId, chunk, server, cdnToken, depotKey ).ConfigureAwait( false );
        }

        /// <summary>
        /// Downloads the specified depot chunk, and optionally processes the chunk and verifies the checksum if the depot decryption key has been provided.
        /// </summary>
        /// <remarks>
        /// This function will also validate the length of the downloaded chunk with the value of <see cref="DepotManifest.ChunkData.CompressedLength"/>,
        /// if it has been assigned a value.
        /// </remarks>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="chunk">
        /// A <see cref="DepotManifest.ChunkData"/> instance that represents the chunk to download.
        /// This value should come from a manifest downloaded with <see cref="o:DownloadManifestAsync"/>.
        /// </param>
        /// <returns>A <see cref="DepotChunk"/> instance that contains the data for the given chunk.</returns>
        /// <param name="host">CDN hostname.</param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        /// <param name="depotKey">
        /// The depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <exception cref="System.ArgumentNullException">chunk's <see cref="DepotManifest.ChunkData.ChunkID"/> was null.</exception>
        /// <exception cref="System.IO.InvalidDataException">Thrown if the downloaded data does not match the expected length.</exception>
        /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
        /// <exception cref="SteamKitWebRequestException">A network error occurred when performing the request.</exception>
        public async Task<DepotChunk> DownloadDepotChunkAsync( uint depotId, DepotManifest.ChunkData chunk, string host, string cdnAuthToken, byte[]? depotKey = null)
        {
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = host,
                VHost = host,
                Port = 80
            };

            return await DownloadDepotChunkAsync( depotId, chunk, server, cdnAuthToken, depotKey ).ConfigureAwait( false );
        }

        /// <summary>
        /// Downloads the specified depot chunk, and optionally processes the chunk and verifies the checksum if the depot decryption key has been provided.
        /// </summary>
        /// <remarks>
        /// This function will also validate the length of the downloaded chunk with the value of <see cref="DepotManifest.ChunkData.CompressedLength"/>,
        /// if it has been assigned a value.
        /// </remarks>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="chunk">
        /// A <see cref="DepotManifest.ChunkData"/> instance that represents the chunk to download.
        /// This value should come from a manifest downloaded with <see cref="o:DownloadManifestAsync"/>.
        /// </param>
        /// <returns>A <see cref="DepotChunk"/> instance that contains the data for the given chunk.</returns>
        /// <param name="server">The content server to connect to.</param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        /// <param name="depotKey">
        /// The depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <exception cref="System.ArgumentNullException">chunk's <see cref="DepotManifest.ChunkData.ChunkID"/> was null.</exception>
        /// <exception cref="System.IO.InvalidDataException">Thrown if the downloaded data does not match the expected length.</exception>
        /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
        /// <exception cref="SteamKitWebRequestException">A network error occurred when performing the request.</exception>
        public async Task<DepotChunk> DownloadDepotChunkAsync( uint depotId, DepotManifest.ChunkData chunk, Server server, string? cdnAuthToken, byte[]? depotKey )
        {
            if ( server == null )
            {
                throw new ArgumentNullException( nameof( server ) );
            }

            if ( chunk == null )
            {
                throw new ArgumentNullException( nameof( chunk ) );
            }

            if ( chunk.ChunkID == null )
            {
                throw new ArgumentException( "Chunk must have a ChunkID.", nameof( chunk ) );
            }

            var chunkID = Utils.EncodeHexString( chunk.ChunkID );

            var chunkData = await DoRawCommandAsync( server, string.Format( "depot/{0}/chunk/{1}", depotId, chunkID ), cdnAuthToken ).ConfigureAwait( false );

            // assert that lengths match only if the chunk has a length assigned.
            if ( chunk.CompressedLength != default( uint ) && chunkData.Length != chunk.CompressedLength )
            {
                throw new InvalidDataException( $"Length mismatch after downloading depot chunk! (was {chunkData.Length}, but should be {chunk.CompressedLength})" );
            }

            var depotChunk = new DepotChunk( chunk, chunkData );

            if ( depotKey != null )
            {
                // if we have the depot key, we can process the chunk immediately
                depotChunk.Process( depotKey );
            }

            return depotChunk;
        }

        /// <summary>
        /// Disposes of this object.
        /// </summary>
        public void Dispose()
        {
            httpClient.Dispose();
        }

        async Task<byte[]> DoRawCommandAsync( Server server, string command, string? args )
        {
            var url = BuildCommand( server, command, args ?? string.Empty );
            using var request = new HttpRequestMessage( HttpMethod.Get, url );

            using ( var cts = new CancellationTokenSource() )
            {
                cts.CancelAfter( RequestTimeout );

                try
                {
                    var response = await httpClient.SendAsync( request, cts.Token ).ConfigureAwait( false );

                    if ( !response.IsSuccessStatusCode )
                    {
                        throw new SteamKitWebRequestException( $"Response status code does not indicate success: {response.StatusCode:D} ({response.ReasonPhrase}).", response );
                    }

                    var responseData = await response.Content.ReadAsByteArrayAsync().ConfigureAwait( false );
                    return responseData;
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "CDNClient", "Failed to complete web request to {0}: {1}", url, ex.Message );
                    throw;
                }
            }
        }

        static Uri BuildCommand( Server server, string command, string args )
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = server.Protocol == Server.ConnectionProtocol.HTTP ? "http" : "https",
                Host = server.VHost,
                Port = server.Port,
                Path = command,
                Query = args
            };

            return uriBuilder.Uri;
        }
    }
}
