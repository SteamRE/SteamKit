using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;
using SteamKit2;

namespace Vapor
{
    struct AvatarDownloadDetails
    {
        public bool Success;
        public string Filename;
        public Exception Exception;
    }

    class AvatarData
    {
        public SteamID steamID;
        public string avatarFile;
        public string fullURI;
        public WebClient client;
        public Action<AvatarDownloadDetails> callback;
    }

    static class CDNCache
    {
        const string AvatarRoot = "http://media.steampowered.com/steamcommunity/public/images/avatars/";

        const string AvatarFull = "{0}/{1}_full.jpg";
        const string AvatarMedium = "{0}/{1}_medium.jpg";
        const string AvatarSmall = "{0}/{1}.jpg";

        private static string CacheDirectory;
        private static Dictionary<SteamID, AvatarData> QueuedDownloads;

        public static void Initialize()
        {
            QueuedDownloads = new Dictionary<SteamID, AvatarData>();
            SetupCache();
        }

        public static void Shutdown()
        {
            lock (QueuedDownloads)
            {
                foreach (AvatarData ad in QueuedDownloads.Values)
                {
                    ad.client.CancelAsync();
                }

                QueuedDownloads.Clear();
            }
        }

        private static void SetupCache()
        {
            CacheDirectory = Path.Combine(Application.StartupPath, "cache");

            if (!Directory.Exists(CacheDirectory))
            {
                try
                {
                    Directory.CreateDirectory(CacheDirectory); // try making the cache directory
                    DebugLog.WriteLine("CDNCache", "Creating cache directory for avatars.");
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("CDNCache", "Unable to create cache directory.\n{0}", ex.ToString());
                    return;
                }
            }
        }

        private static WebClient SpawnWebClient()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);

            return webClient;
        }

        private static void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            AvatarData ad = e.UserState as AvatarData;

            if (e.Error != null)
            {
                DebugLog.WriteLine("CDNCache", "Unable to download avatar {0}.\n{1}", ad.fullURI, e.Error.ToString());
                ad.callback( new AvatarDownloadDetails() { Success = false, Exception = e.Error } );
            }
            else
            {
                ad.callback( new AvatarDownloadDetails() { Success = true, Filename = ad.avatarFile } );
            }

            lock (QueuedDownloads)
            {
                QueuedDownloads.Remove(ad.steamID);
            }

            ad.client.Dispose();
        }

        public static void DownloadAvatar( SteamID steamId, byte[] avatarHash, Action<AvatarDownloadDetails> completionHandler )
        {
            string hashStr = BitConverter.ToString( avatarHash ).Replace( "-", "" ).ToLower();
            string hashPrefix = hashStr.Substring( 0, 2 );

            // if an existing request exists, return false
            lock (QueuedDownloads)
            {
                if (QueuedDownloads.ContainsKey(steamId))
                {
                    completionHandler(new AvatarDownloadDetails() { Success = false });
                    return;
                }
            }

            string localPath = Path.Combine( Application.StartupPath, "cache" );
            string localFile = Path.Combine( localPath, hashStr + ".jpg" );

            if ( File.Exists( localFile ) )
            {
                FileInfo fi = new FileInfo( localFile );

                if ( fi.Length == 0 )
                {
                    DebugLog.WriteLine( "CDNCache", "Avatar {0} was truncated, redownloading.", hashStr );
                    File.Delete( localFile );
                }
                else
                {
                    DebugLog.WriteLine( "CDNCache", "Avatar {0} found in the cache.", hashStr );

                    completionHandler(new AvatarDownloadDetails() { Success = true, Filename = localFile });
                    return;
                }
            }

            DebugLog.WriteLine( "CDNCache", "Downloading avatar {0}", hashStr );
            

            string downloadUri = string.Format(AvatarRoot + AvatarSmall, hashPrefix, hashStr);

            AvatarData ad = new AvatarData
            {
                avatarFile = localFile,
                callback = completionHandler,
                fullURI = downloadUri,
                steamID = steamId,
                client = SpawnWebClient()
            };

            lock (QueuedDownloads)
            {
                QueuedDownloads.Add(steamId, ad);
            }

            ad.client.DownloadFileAsync(new Uri(downloadUri), localFile, ad);
        }

    }
}
