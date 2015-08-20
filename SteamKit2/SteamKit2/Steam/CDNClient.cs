/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SteamKit2
{
    /// <summary>
    /// The CDNClient class is used for downloading game content from the Steam servers.
    /// </summary>
    public sealed class CDNClient
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
                    Host = endPoint.Address.ToString(),
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
                    Host = endPoint.Host,
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

                if ( processedData[0] == 'V' && processedData[1] == 'Z' )
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


        SteamClient steamClient;

        byte[] appTicket;
        ConcurrentDictionary<uint, bool> depotIds;
        ConcurrentDictionary<uint, byte[]> depotKeys;
        ConcurrentDictionary<uint, string> depotCdnAuthKeys;

        byte[] sessionKey;

        Server connectedServer;

        ulong sessionId;
        long reqCounter;

        /// <summary>
        /// Default timeout to use when making requests
        /// </summary>
        public static TimeSpan RequestTimeout = TimeSpan.FromSeconds( 10 );

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
        /// <param name="appTicket">
        /// The optional appticket for the depot that will be downloaded.
        /// This must be present when connected to steam non-anonymously.
        /// </param>
        public CDNClient( SteamClient steamClient, byte[] appTicket = null )
        {
            this.steamClient = steamClient;

            this.depotIds = new ConcurrentDictionary<uint, bool>();
            this.appTicket = appTicket;

            this.depotKeys = new ConcurrentDictionary<uint, byte[]>();
            this.depotCdnAuthKeys = new ConcurrentDictionary<uint, string>();
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
        /// <param name="maxServers">The maximum amount of servers to request.</param>
        /// <returns>A list of servers.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// No Steam CS servers available, or the suggested CellID is unavailable.
        /// Check that the <see cref="SteamClient"/> associated with this <see cref="CDNClient"/> instance is logged onto Steam.
        /// </exception>
        public List<Server> FetchServerList( IPEndPoint csServer = null, uint? cellId = null, int maxServers = 20 )
        {
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

            KeyValue serverKv = DoCommand( csServer, "serverlist", args: string.Format( "{0}/{1}/", cellId, maxServers ) );

            var serverList = new List<Server>( maxServers );

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

                uint serverCell = ( uint )server[ "cell" ].AsInteger();
                int load = server[ "load" ].AsInteger();
                int weightedLoad = server[ "weightedload" ].AsInteger();
                int entries = server[ "NumEntriesInClientList" ].AsInteger( 1 );

                serverList.Add( new Server
                {
                    Host = host,
                    Port = port,

                    Type = type,

                    CellID = serverCell,

                    Load = load,
                    WeightedLoad = weightedLoad,
                    NumEntries = entries
                } );
                
            }

            return serverList;
        }


        /// <summary>
        /// Connects and initializes a session to the specified content server.
        /// </summary>
        /// <param name="csServer">The content server to connect to.</param>
        /// <exception cref="System.ArgumentNullException">csServer was null.</exception>
        public void Connect( Server csServer )
        {
            DebugLog.Assert( steamClient.IsConnected, "CDNClient", "CMClient is not connected!" );

            if ( csServer == null )
                throw new ArgumentNullException( "csServer" );

            // Nothing needs to be done to initialize a session to a CDN server
            if ( csServer.Type == "CDN" )
            {
                connectedServer = csServer;
                return;
            }

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

            ulong.TryParse( initKv["sessionid"].AsString(), out sessionId );
            reqCounter = initKv[ "req-counter" ].AsLong();
            connectedServer = csServer;
        }

        /// <summary>
        /// Authenticate a CDNClient to a depot in the connected session
        /// </summary>
        /// <param name="depotid">The id of the depot being accessed.</param>
        /// <param name="depotKey">
        /// The optional depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        public void AuthenticateDepot( uint depotid, byte[] depotKey = null, string cdnAuthToken = null )
        {
            if ( depotIds.ContainsKey( depotid ) )
                return;

            string data;

            if ( connectedServer.Type != "CDN" || cdnAuthToken == null )
            {
                if ( appTicket == null )
                {
                    data = string.Format( "depotid={0}", depotid );
                }
                else
                {
                    byte[] encryptedAppTicket = CryptoHelper.SymmetricEncrypt( appTicket, sessionKey );
                    data = string.Format( "appticket={0}", WebHelpers.UrlEncode( encryptedAppTicket ) );
                }

                DoCommand( connectedServer, "authdepot", data, WebRequestMethods.Http.Post, true );
            }

            depotIds[depotid] = true;
            depotKeys[depotid] = depotKey;
            depotCdnAuthKeys[depotid] = cdnAuthToken;
        }

        /// <summary>
        /// Downloads the depot manifest specified by the given manifest ID, and optionally decrypts the manifest's filenames if the depot decryption key has been provided.
        /// </summary>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="manifestId">The unique identifier of the manifest to be downloaded.</param>
        /// <returns>A <see cref="DepotManifest"/> instance that contains information about the files present within a depot.</returns>
        public DepotManifest DownloadManifest( uint depotId, ulong manifestId )
        {
            string cdnToken = null;
            depotCdnAuthKeys.TryGetValue( depotId, out cdnToken );

            byte[] depotKey = null;
            depotKeys.TryGetValue( depotId, out depotKey );

            return DownloadManifestCore( depotId, manifestId, connectedServer, cdnToken, depotKey );
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
        public DepotManifest DownloadManifest( uint depotId, ulong manifestId, string host, string cdnAuthToken, byte[] depotKey = null )
        {
            var server = new Server
            {
                Host = host,
                Port = 80
            };

            return DownloadManifestCore( depotId, manifestId, server, cdnAuthToken, depotKey );
        }

        // Ambiguous reference in cref attribute: 'CDNClient.DownloadManifest'. Assuming 'SteamKit2.CDNClient.DownloadManifest(uint, ulong)',
        // but could have also matched other overloads including 'SteamKit2.CDNClient.DownloadManifest(uint, ulong, string, string, byte[])'.
#pragma warning disable 0419

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
        /// This value should come from a manifest downloaded with <see cref="CDNClient.DownloadManifest"/>.
        /// </param>
        /// <returns>A <see cref="DepotChunk"/> instance that contains the data for the given chunk.</returns>
        /// <exception cref="System.ArgumentNullException">chunk's <see cref="DepotManifest.ChunkData.ChunkID"/> was null.</exception>
        public DepotChunk DownloadDepotChunk( uint depotId, DepotManifest.ChunkData chunk )
#pragma warning restore 0419
        {
            if ( chunk.ChunkID == null )
                throw new ArgumentNullException( "chunk.ChunkID" );

            string cdnToken = null;
            depotCdnAuthKeys.TryGetValue( depotId, out cdnToken );

            byte[] depotKey = null;
            depotKeys.TryGetValue( depotId, out depotKey );

            return DownloadDepotChunkCore( depotId, chunk, connectedServer, cdnToken, depotKey );
        }

        // Ambiguous reference in cref attribute: 'CDNClient.DownloadManifest'. Assuming 'SteamKit2.CDNClient.DownloadManifest(uint, ulong)',
        // but could have also matched other overloads including 'SteamKit2.CDNClient.DownloadManifest(uint, ulong, string, string, byte[])'.
#pragma warning disable 0419

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
        /// This value should come from a manifest downloaded with <see cref="CDNClient.DownloadManifest"/>.
        /// </param>
        /// <returns>A <see cref="DepotChunk"/> instance that contains the data for the given chunk.</returns>
        /// <param name="host">CDN hostname.</param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        /// <param name="depotKey">
        /// The depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <exception cref="System.ArgumentNullException">chunk's <see cref="DepotManifest.ChunkData.ChunkID"/> was null.</exception>
        public DepotChunk DownloadDepotChunk( uint depotId, DepotManifest.ChunkData chunk, string host, string cdnAuthToken, byte[] depotKey = null )
#pragma warning restore 0419
        {
            if (chunk.ChunkID == null)
                throw new ArgumentNullException( "chunk.ChunkID" );

            var server = new Server
            {
                Host = host,
                Port = 80
            };

            return DownloadDepotChunkCore( depotId, chunk, server, cdnAuthToken, depotKey );
        }

        string BuildCommand( Server server, string command, string args, string authtoken = null )
        {
            return string.Format( "http://{0}:{1}/{2}/{3}{4}", server.Host, server.Port, command, args, authtoken ?? "" );
        }

        byte[] DoRawCommand( Server server, string command, string data = null, string method = WebRequestMethods.Http.Get, bool doAuth = false, string args = "", string authtoken = null )
        {
            string url = BuildCommand( server, command, args, authtoken );
            var webReq = HttpWebRequest.Create( url ) as HttpWebRequest;
            webReq.Method = method;
            webReq.Pipelined = true;
            webReq.KeepAlive = true;

            if ( doAuth && server.Type == "CS" )
            {
                var req = Interlocked.Increment( ref reqCounter );

                byte[] shaHash;

                using ( var ms = new MemoryStream() )
                using ( var bw = new BinaryWriter( ms ) )
                {
                    var uri = new Uri( url );

                    bw.Write( sessionId );
                    bw.Write( req );
                    bw.Write( sessionKey );
                    bw.Write( Encoding.UTF8.GetBytes( uri.AbsolutePath ) );

                    shaHash = CryptoHelper.SHAHash( ms.ToArray() );
                }

                string hexHash = Utils.EncodeHexString( shaHash );
                string authHeader = string.Format( "sessionid={0};req-counter={1};hash={2};", sessionId, req, hexHash );

                webReq.Headers[ "x-steam-auth" ] = authHeader;
            }

            if ( method == WebRequestMethods.Http.Post )
            {
                byte[] payload = Encoding.UTF8.GetBytes( data );
                webReq.ContentType = "application/x-www-form-urlencoded";
                webReq.ContentLength = payload.Length;

                using ( var reqStream = webReq.GetRequestStream() )
                {
                    reqStream.Write( payload, 0, payload.Length );
                }
            }


            var result = webReq.BeginGetResponse( null, null );

            if ( !result.AsyncWaitHandle.WaitOne( RequestTimeout ) )
            {
                webReq.Abort();
            }

            try
            {
                var response = webReq.EndGetResponse( result );

                using ( var ms = new MemoryStream( ( int ) response.ContentLength ) )
                {
                    response.GetResponseStream().CopyTo( ms );
                    response.Close();

                    return ms.ToArray();
                }
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "CDNClient", "Failed to complete web request to {0}: {1}", url, ex.Message );
                throw;
            }
        }

        KeyValue DoCommand( Server server, string command, string data = null, string method = WebRequestMethods.Http.Get, bool doAuth = false, string args = "", string authtoken = null )
        {
            byte[] resultData = DoRawCommand( server, command, data, method, doAuth, args, authtoken );

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

        DepotManifest DownloadManifestCore( uint depotId, ulong manifestId, Server server, string cdnAuthToken, byte[] depotKey )
        {

            byte[] manifestData = DoRawCommand( server, "depot", doAuth: true, args: string.Format( "{0}/manifest/{1}/5", depotId, manifestId ), authtoken: cdnAuthToken );

            manifestData = ZipUtil.Decompress( manifestData );

            var depotManifest = new DepotManifest( manifestData );

            if ( depotKey != null )
            {
                // if we have the depot key, decrypt the manifest filenames
                depotManifest.DecryptFilenames( depotKey );
            }

            return depotManifest;
        }

        DepotChunk DownloadDepotChunkCore( uint depotId, DepotManifest.ChunkData chunk, Server server, string cdnAuthToken, byte[] depotKey )
        {
            var chunkID = Utils.EncodeHexString( chunk.ChunkID );

            byte[] chunkData = DoRawCommand( server, "depot", doAuth: true, args: string.Format( "{0}/chunk/{1}", depotId, chunkID ), authtoken: cdnAuthToken );

            if ( chunk.CompressedLength != default( uint ) )
            {
                // assert that lengths match only if the chunk has a length assigned.
                DebugLog.Assert( chunkData.Length == chunk.CompressedLength, "CDNClient", "Length mismatch after downloading depot chunk!" );
            }

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
    }
}
