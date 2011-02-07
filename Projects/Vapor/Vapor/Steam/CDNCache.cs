using System;
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
    }

    static class CDNCache
    {
        const string AvatarRoot = "http://media.steampowered.com/steamcommunity/public/images/avatars/";

        const string AvatarFull = "{0}/{1}_full.jpg";
        const string AvatarMedium = "{0}/{1}_medium.jpg";
        const string AvatarSmall = "{0}/{1}.jpg";

        static Semaphore dlPool;

        static object mapLock = new object();
        static Dictionary<SteamID, byte[]> avatarMap;


        static CDNCache()
        {
            dlPool = new Semaphore( 0, 3 );
            dlPool.Release( 3 );

            avatarMap = new Dictionary<SteamID, byte[]>();
        }

        struct AvatarData
        {
            public SteamID friend;
            public byte[] hash;
            public Action<AvatarDownloadDetails> callback;
        }

        public static byte[] GetAvatarHash( SteamID steamId )
        {
            if ( avatarMap.ContainsKey( steamId ) )
                return avatarMap[ steamId ];

            return null;
        }

        public static void DownloadAvatar( SteamID steamId, byte[] avatarHash, Action<AvatarDownloadDetails> callBack )
        {
            ThreadPool.QueueUserWorkItem( DoDownload, new AvatarData() { friend = steamId, hash = avatarHash, callback = callBack } );
        }

        static void DoDownload( object state )
        {
            AvatarData data = ( AvatarData )state;

            SteamID steamId = data.friend;
            Action<AvatarDownloadDetails> callBack = data.callback;
            byte[] avatarHash = data.hash;

            lock ( mapLock )
                avatarMap[ steamId ] = avatarHash;

            string hashStr = BitConverter.ToString( avatarHash ).Replace( "-", "" ).ToLower();
            string hashPrefix = hashStr.Substring( 0, 2 );

            string localPath = Path.Combine( Application.StartupPath, "cache" );

            if ( !Directory.Exists( localPath ) )
            {
                try
                {
                    Directory.CreateDirectory( localPath ); // try making the cache directory
                }
                catch
                {
                    callBack( new AvatarDownloadDetails() { Success = false, } );
                    return;
                }
            }

            string localFile = Path.Combine( localPath, hashStr + ".jpg" );
            if ( File.Exists( localFile ) )
            {
                callBack( new AvatarDownloadDetails() { Success = true, Filename = localFile } );
                return;
            }

            dlPool.WaitOne();
            
            string downloadUri = string.Format( AvatarRoot + AvatarSmall, hashPrefix, hashStr );

            using ( WebClient client = new WebClient() )
            {
                try
                {
                    client.DownloadFile( downloadUri, localFile );
                }
                catch
                {
                    dlPool.Release();
                    callBack( new AvatarDownloadDetails() { Success = false } );
                    return;
                }
            }

            dlPool.Release();
            callBack( new AvatarDownloadDetails() { Success = true, Filename = localFile } );
        }
    }
}
