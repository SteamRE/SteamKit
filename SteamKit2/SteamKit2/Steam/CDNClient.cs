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
    public sealed class CDNClient : IDisposable
    {
        public sealed class Server
        {
            public string Host { get; internal set; }
            public int Port { get; internal set; }

            public string Type { get; internal set; }


            /// <summary>
            /// Performs an implicit conversion from <see cref="System.NetIPEndPoint"/> to <see cref="SteamKit2.CDNClient.Server"/>.
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


        SteamClient steamClient;

        WebClient webClient;

        byte[] appTicket;
        uint depotId;

        byte[] sessionKey;

        Server connectedServer;

        ulong sessionId;
        long reqCounter;


        static CDNClient()
        {
            ServicePointManager.Expect100Continue = false;
        }


        public CDNClient( SteamClient steamClient, byte[] appTicket = null )
        {
            this.steamClient = steamClient;
            this.appTicket = appTicket;

            webClient = new WebClient();
        }
        public CDNClient( SteamClient steamClient, uint depotId )
        {
            this.steamClient = steamClient;
            this.depotId = depotId;

            webClient = new WebClient();
        }


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

        public DepotManifest DownloadManifest( uint depotId, ulong manifestId )
        {
            byte[] compressedManifest = DoRawCommand( connectedServer, "depot", doAuth: true, args: string.Format( "{0}/manifest/{1}", depotId, manifestId ) );

            byte[] depotManifest = ZipUtil.Decompress( compressedManifest );

            return new DepotManifest( depotManifest );
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


#if false
    /// <summary>
    /// Represents a client able to connect to the Steam3 CDN and download games on the new content system.
    /// </summary>
    public sealed class CDNClient
    {
        /// <summary>
        /// Represents the endpoint of a Steam3 content server.
        /// </summary>
        public sealed class ClientEndPoint
        {
            /// <summary>
            /// Gets the server host.
            /// </summary>
            public string Host { get; private set; }
            /// <summary>
            /// Gets the server port.
            /// </summary>
            public int Port { get; private set; }
            /// <summary>
            /// Gets the server type.
            /// </summary>
            public string Type { get; private set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="ClientEndPoint"/> class.
            /// </summary>
            /// <param name="host">The server host.</param>
            /// <param name="port">The server port.</param>
            /// <param name="type">The server type.</param>
            public ClientEndPoint( string host, int port, string type = null )
            {
                Host = host;
                Port = port;
                Type = type;
            }

        }

        private WebClient webClient;
        private byte[] sessionKey;

        private ulong sessionID;
        private long reqcounter;

        private ClientEndPoint endPoint;
        private byte[] appTicket;

        private SteamID steamID;
        private uint depotID;

        static CDNClient()
        {
            ServicePointManager.Expect100Continue = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CDNClient"/> class.
        /// </summary>
        /// <param name="cdnServer">The CDN server to connect to.</param>
        /// <param name="appticket">The appticket of the app this instance is for.</param>
        public CDNClient(ClientEndPoint cdnServer, byte[] appticket)
        {
            sessionKey = CryptoHelper.GenerateRandomBlock(32);

            webClient = new WebClient();

            endPoint = cdnServer;
            appTicket = appticket;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CDNClient"/> class without an application ticket.
        /// </summary>
        /// <param name="cdnServer">The CDN server to connect to.</param>
        /// <param name="steamID">The SteamID of the current user.</param>
        /// <param name="depotID">Depot ID being requested.</param>
        public CDNClient(ClientEndPoint cdnServer, uint depotID, SteamID steamID)
        {
            sessionKey = CryptoHelper.GenerateRandomBlock(32);

            webClient = new WebClient();

            endPoint = cdnServer;
            appTicket = null;
            this.steamID = steamID;
            this.depotID = depotID;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="CDNClient"/> is reclaimed by garbage collection.
        /// </summary>
        ~CDNClient()
        {
            webClient.Dispose();
        }

        /// <summary>
        /// Points this <see cref="CDNClient"/> instance to another server.
        /// </summary>
        /// <param name="ep">The endpoint.</param>
        public void PointTo(ClientEndPoint ep)
        {
            endPoint = ep;
        }

        /// <summary>
        /// Connects this instance to the server.
        /// </summary>
        /// <returns><c>true</c> if the connection was a success; otherwise, <c>false</c>.</returns>
        public bool Connect()
        {

            byte[] encryptedKey = null;

            // TODO: handle other universes?
            byte[] universeKey = KeyDictionary.GetPublicKey( EUniverse.Public );
            using ( var rsa = new RSACrypto( universeKey ) )
            {
                encryptedKey = rsa.Encrypt( sessionKey );
            }

            string payload;

            if (appTicket == null)
            {
                payload = String.Format("sessionkey={0}&anonymoususer=1&steamid={1}", WebHelpers.UrlEncode(encryptedKey), steamID.ConvertToUInt64());
            }
            else
            {
                byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);
                payload = String.Format("sessionkey={0}&appticket={1}", WebHelpers.UrlEncode(encryptedKey), WebHelpers.UrlEncode(encryptedTicket));
            }

            webClient.Headers.Clear();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            string response;
            try
            {
                response = webClient.UploadString(BuildCommand(endPoint, "initsession"), payload);
            }
            catch (WebException e)
            {
                LogWebException("Connect", e);
                return false;
            }

            var responsekv = KeyValue.LoadFromString(response);
            var sessionidn = responsekv.Children.Where(c => c.Name == "sessionid").First();
            var reqcountern = responsekv.Children.Where(c => c.Name == "req-counter").First();

            sessionID = (ulong)(sessionidn.AsLong(0));
            reqcounter = reqcountern.AsLong(0);

            try
            {
                AuthDepot();
            }
            catch (WebException e)
            {
                LogWebException("AuthDepot", e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Downloads the depot manifest for the given depot and manifest.
        /// </summary>
        /// <param name="depotid">The depotid.</param>
        /// <param name="manifestid">The manifestid.</param>
        /// <returns>A <see cref="DepotManifest"/> instance on success; otherwise, <c>null</c>.</returns>
        public DepotManifest DownloadDepotManifest(int depotid, ulong manifestid)
        {
            Uri manifestURI = new Uri(BuildCommand(endPoint, "depot"), String.Format("{0}/manifest/{1}", depotid, manifestid));

            PrepareAuthHeader(ref webClient, manifestURI);

            byte[] compressedManifest;
            byte[] manifest;
            try
            {
                compressedManifest = webClient.DownloadData(manifestURI);
            }
            catch (WebException e)
            {
                LogWebException("DownloadDepotManifest", e);
                return null;
            }

            try
            {
                manifest = ZipUtil.Decompress(compressedManifest);
            }
            catch (Exception)
            {
                return null;
            }

            return new DepotManifest( manifest );
        }

        /// <summary>
        /// Downloads the specified depot chunk from the content server.
        /// </summary>
        /// <param name="depotid">The DepotID of the chunk to download.</param>
        /// <param name="chunkid">The the ID of the chunk to download.</param>
        /// <returns></returns>
        public byte[] DownloadDepotChunk(int depotid, string chunkid)
        {
            Uri chunkURI = new Uri(BuildCommand(endPoint, "depot"), String.Format("{0}/chunk/{1}", depotid, chunkid));

            PrepareAuthHeader(ref webClient, chunkURI);

            byte[] chunk;
            try
            {
                chunk = webClient.DownloadData(chunkURI);
            }
            catch (WebException e)
            {
                LogWebException("DownloadDepotChunk", e);
                return null;
            }

            return chunk;
        }

        /// <summary>
        /// Processes a chunk by decrypting and decompressing it.
        /// </summary>
        /// <param name="chunk">The chunk to process.</param>
        /// <param name="depotkey">The AES encryption key to use when decrypting the chunk.</param>
        /// <returns>The processed chunk.</returns>
        public static byte[] ProcessChunk(byte[] chunk, byte[] depotkey)
        {
            byte[] decrypted_chunk = CryptoHelper.SymmetricDecrypt(chunk, depotkey);
            byte[] decompressed_chunk = ZipUtil.Decompress(decrypted_chunk);

            return decompressed_chunk;
        }

        private void AuthDepot()
        {
            Uri authURI = BuildCommand(endPoint, "authdepot");

            PrepareAuthHeader(ref webClient, authURI);
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            string payload;
            if (appTicket == null)
            {
                payload = String.Format("depotid={0}", depotID);
            }
            else
            {
                byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);
                payload = String.Format("appticket={0}", WebHelpers.UrlEncode(encryptedTicket));
            }

            webClient.UploadString(authURI, payload);
        }

        private void PrepareAuthHeader(ref WebClient client, Uri uri)
        {
            reqcounter++;

            byte[] sha_hash;

            using ( var ms = new MemoryStream() )
            using ( var bw = new BinaryWriter( ms ) )
            {
                bw.Write( sessionID );
                bw.Write( reqcounter );
                bw.Write( sessionKey );
                bw.Write( Encoding.ASCII.GetBytes( uri.AbsolutePath ) );

                sha_hash = CryptoHelper.SHAHash( ms.ToArray() );
            }

            string hex_hash = Utils.EncodeHexString(sha_hash);

            string authheader = String.Format("sessionid={0};req-counter={1};hash={2};", sessionID, reqcounter, hex_hash);

            webClient.Headers.Clear();
            webClient.Headers.Add("x-steam-auth", authheader);
        }

        private static Uri BuildCommand(ClientEndPoint csServer, string command)
        {
            return new Uri(String.Format("http://{0}:{1}/{2}/", csServer.Host, csServer.Port.ToString(), command));
        }

        /// <summary>
        /// Fetches a server list from the given content server for the provided CellID.
        /// </summary>
        /// <param name="csServer">The server to request a server list from.</param>
        /// <param name="cellID">The CellID.</param>
        /// <returns>A list of content servers.</returns>
        public static List<ClientEndPoint> FetchServerList(ClientEndPoint csServer, int cellID)
        {
            int serversToRequest = 20;

            using(WebClient webClient = new WebClient())
            {
                Uri request = new Uri(BuildCommand(csServer, "serverlist"), String.Format("{0}/{1}/", cellID, serversToRequest));

                string serverList;
                try
                {
                    serverList = webClient.DownloadString(request);
                }
                catch (WebException e)
                {
                    LogWebException("FetchServerList", e);
                    return null;
                }

                KeyValue serverkv = KeyValue.LoadFromString(serverList);

                if (serverkv["deferred"].AsString() == "1")
                    return null;

                List<ClientEndPoint> endpoints = new List<ClientEndPoint>();

                foreach (var child in serverkv.Children)
                {
                    var node = child.Children.Where(x => x.Name == "host" || x.Name == "Host").First();
                    var typeNode = child.Children.Where(x => x.Name == "type").First();

                    var endpoint_string = node.Value.Split(':');

                    int port = 80;
                    
                    if(endpoint_string.Length > 1)
                        port = int.Parse(endpoint_string[1]);

                    endpoints.Add(new ClientEndPoint(endpoint_string[0], port, typeNode.AsString()));
                }

                return endpoints;
            }
        }

        private static void LogWebException(string function, WebException e)
        {
            HttpWebResponse response;
            if (e.Status == WebExceptionStatus.ProtocolError && (response = e.Response as HttpWebResponse) != null)
            {
                DebugLog.WriteLine("CDNClient", "{0} received HTTP error: {1} - {2}", function, (int)response.StatusCode, response.StatusCode);
            }
            else
            {
                DebugLog.WriteLine("CDNClient", "{0} returned: {1}", function, e.Status);
            }
        }
    }
#endif
}
