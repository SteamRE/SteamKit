/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;

namespace SteamKit2
{
    public class CDNClient
    {
        private WebClient webClient;
        private byte[] sessionKey;

        private ulong sessionID;
        private long reqcounter;

        private IPEndPoint endPoint;
        private byte[] appTicket;

        public CDNClient(IPEndPoint cdnServer, byte[] appticket)
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

            string payload = String.Format("sessionkey={0}&appticket={1}", EncodeFull(encryptedKey), EncodeFull(encryptedTicket));

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

            sessionID = (ulong)(sessionidn.AsInteger(0));
            reqcounter = reqcountern.AsInteger(0);

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

        private void AuthDepot()
        {
            Uri authURI = BuildCommand(endPoint, "authdepot");

            PrepareAuthHeader(ref webClient, authURI);
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);
            string payload = String.Format("appticket={0}", EncodeFull(encryptedTicket));

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

        private static string EncodeFull(byte[] input)
        {
            return Regex.Replace(
              HttpUtility.UrlEncode(input),
              @"[()\.\-*]",
              m => "%" + Convert.ToString((int)m.Captures[0].Value[0], 16).ToUpperInvariant()
            );
        }

        private static Uri BuildCommand(IPEndPoint csServer, string command)
        {
            return new Uri(String.Format("http://{0}:{1}/{2}/", csServer.Address.ToString(), csServer.Port.ToString(), command));
        }

        public static List<IPEndPoint> FetchServerList(IPEndPoint csServer, int cellID)
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

                List<IPEndPoint> endpoints = new List<IPEndPoint>();

                KeyValue serverkv = KeyValue.LoadFromString(serverList);

                foreach (var child in serverkv.Children)
                {
                    var node = child.Children.Where(x => x.Name == "host").First();
                    var endpoint_string = node.AsString("").Split(':');

                    IPAddress ipaddr;
                    int port = 80;
                    
                    if(endpoint_string.Length > 1)
                        port = int.Parse(endpoint_string[1]);

                    if (IPAddress.TryParse(endpoint_string[0], out ipaddr))
                    {
                        endpoints.Add(new IPEndPoint(ipaddr, port));
                    }
                    else
                    {
                        foreach (IPAddress addr in Dns.GetHostEntry(endpoint_string[0]).AddressList)
                        {
                            endpoints.Add(new IPEndPoint(addr, port));
                        }
                    }
                }

                return endpoints;
            }
        }
    }
}
