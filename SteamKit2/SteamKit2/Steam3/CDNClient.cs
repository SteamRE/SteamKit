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

#if wehadakeyvalueparser
using KeyValueParser;
#endif

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

#if wehadakeyvalueparser
            KVItem responsekv = KVParser.Read(response);

            var sessionidn = responsekv.subItems.Where(c => c.key == "sessionid").First();
            var reqcountern = responsekv.subItems.Where(c => c.key == "req-counter").First();

            sessionID = (ulong)long.Parse(sessionidn.value);
            reqcounter = long.Parse(reqcountern.value);
#endif

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

        private void AuthDepot()
        {
            Uri authURI = BuildCommand(endPoint, "authdepot");

            webClient.Headers.Clear();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            PrepareAuthHeader(ref webClient, authURI);

            byte[] encryptedTicket = CryptoHelper.SymmetricEncrypt(appTicket, sessionKey);
            string payload = String.Format("appticket={0}", EncodeFull(encryptedTicket));

            string response = webClient.UploadString(authURI, payload);
        }

        private void PrepareAuthHeader(ref WebClient client, Uri uri)
        {
            reqcounter++;

            byte[] sha_hash;

            using(System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                byte[] buf_sid = Encoding.ASCII.GetBytes(sessionID.ToString());
                byte[] buf_req = Encoding.ASCII.GetBytes(reqcounter.ToString());
                byte[] buf_url = Encoding.ASCII.GetBytes(uri.AbsoluteUri);

                
                ms.Write(buf_sid, 0, buf_sid.Length);
                ms.Write(buf_req, 0, buf_req.Length);
                ms.Write(sessionKey, 0, sessionKey.Length);
                ms.Write(buf_url, 0, buf_url.Length);

                sha_hash = CryptoHelper.SHAHash(ms.ToArray());
            }

            string hex_hash = sha_hash.Aggregate(new StringBuilder(),
                       (sb, v) => sb.Append(v.ToString("x2"))
                      ).ToString();

            string authheader = String.Format("sessionid={0};req-counter={1};hash={2};", sessionID, reqcounter, hex_hash);

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
#if wehadakeyvalueparser
                KVItem serverkv = KVParser.Read(serverList);

                foreach (var entry in serverkv.subItems)
                {
                    var node = entry.subItems.Where(x => x.key == "host").First();
                    var endpoint_string = node.value.Split(':');

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
#endif
                return endpoints;
            }
        }
    }
}
