/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SteamKit2
{
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

            byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);

            string payload = String.Format("sessionkey={0}&appticket={1}", WebHelpers.UrlEncode(encryptedKey), WebHelpers.UrlEncode(encryptedTicket));

            webClient.Headers.Clear();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            string response;
            try
            {
                response = webClient.UploadString(BuildCommand(endPoint, "initsession"), payload);
            }
            catch (WebException)
            {
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
            catch (WebException)
            {
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
            catch (WebException)
            {
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
            catch (WebException)
            {
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

            byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);
            string payload = String.Format("appticket={0}", WebHelpers.UrlEncode(encryptedTicket));

            string response = webClient.UploadString(authURI, payload);
        }

        private void PrepareAuthHeader(ref WebClient client, Uri uri)
        {
            reqcounter++;

            byte[] sha_hash;

            BinaryWriterEx bb = new BinaryWriterEx();

            bb.Write( sessionID );
            bb.Write( reqcounter );
            bb.Write( sessionKey );
            bb.Write( Encoding.ASCII.GetBytes( uri.AbsolutePath ) );

            sha_hash = CryptoHelper.SHAHash(bb.ToArray());

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
                    Console.WriteLine("FetchServerList returned: {0}", e.Message);
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
    }
}
