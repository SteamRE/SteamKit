/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace SteamKit2
{
    public class CDNClient
    {
        public class ClientEndPoint
        {
            public string Host;
            public int Port;

            public ClientEndPoint(string host, int port)
            {
                Host = host;
                Port = port;
            }
        }

        private WebClient webClient;
        private byte[] sessionKey;

        private ulong sessionID;
        private long reqcounter;

        private ClientEndPoint endPoint;
        private byte[] appTicket;

        public CDNClient(ClientEndPoint cdnServer, byte[] appticket)
        {
            sessionKey = CryptoHelper.GenerateRandomBlock(32);

            webClient = new WebClient();
            ServicePointManager.Expect100Continue = false;

            endPoint = cdnServer;
            appTicket = appticket;
        }

        ~CDNClient()
        {
            webClient.Dispose();
        }

        public bool Connect()
        {
            byte[] encryptedKey = CryptoHelper.RSAEncrypt(sessionKey);
            byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);

            string payload = String.Format("sessionkey={0}&appticket={1}", EncodeBuffer(encryptedKey), EncodeBuffer(encryptedTicket));

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

        public byte[] DownloadDepotManifest(int depotid, ulong manifestid)
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

            return manifest;
        }

        public byte[] DownloadDepotChunk(int depotid, ulong chunkid)
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

        private void AuthDepot()
        {
            Uri authURI = BuildCommand(endPoint, "authdepot");

            PrepareAuthHeader(ref webClient, authURI);
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);
            string payload = String.Format("appticket={0}", EncodeBuffer(encryptedTicket));

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

            string hex_hash = sha_hash.Aggregate(new StringBuilder(),
                       (sb, v) => sb.Append(v.ToString("x2"))
                      ).ToString();

            string authheader = String.Format("sessionid={0};req-counter={1};hash={2};", sessionID, reqcounter, hex_hash);

            webClient.Headers.Clear();
            webClient.Headers.Add("x-steam-auth", authheader);
        }

        private static Uri BuildCommand(ClientEndPoint csServer, string command)
        {
            return new Uri(String.Format("http://{0}:{1}/{2}/", csServer.Host, csServer.Port.ToString(), command));
        }

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
                catch (WebException)
                {
                    return null;
                }

                List<ClientEndPoint> endpoints = new List<ClientEndPoint>();

                KeyValue serverkv = KeyValue.LoadFromString(serverList);

                foreach (var child in serverkv.Children)
                {
                    var node = child.Children.Where(x => x.Name == "host").First();
                    var typeNode = child.Children.Where(x => x.Name == "type").First();

                    if (typeNode.Value == "CDN") // not sure what to do with these
                        continue;

                    var endpoint_string = node.Value.Split(':');

                    int port = 80;
                    
                    if(endpoint_string.Length > 1)
                        port = int.Parse(endpoint_string[1]);

                    endpoints.Add(new ClientEndPoint(endpoint_string[0], port));
                }

                return endpoints;
            }
        }

        internal static bool IsUrlSafeChar(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }

            switch (ch)
            {
                case '-':
                case '.':
                case '_':
                    return true;
            }

            return false;
        }

        internal static string EncodeBuffer(byte[] input)
        {
            StringBuilder encoded = new StringBuilder(input.Length * 2);

            for (int i = 0; i < input.Length; i++)
            {
                char inch = (char)input[i];

                if (IsUrlSafeChar(inch))
                {
                    encoded.Append(inch);
                }
                else if (inch == ' ')
                {
                    encoded.Append('+');
                }
                else
                {
                    encoded.AppendFormat("%{0:X2}", input[i]);
                }
            }

            return encoded.ToString();
        }
    }
}
