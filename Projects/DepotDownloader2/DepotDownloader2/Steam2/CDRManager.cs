/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using SteamKit2;

namespace DepotDownloader2
{
    class CDR
    {
        [BlobField( FieldKey = CDRFields.eFieldApplicationsRecord, Depth = 1, Complex = true )]
        public List<App> Apps { get; set; }

        [BlobField( FieldKey = CDRFields.eFieldSubscriptionsRecord, Depth = 1, Complex = true )]
        public List<Sub> Subs { get; set; }
    }

    class App
    {
        [BlobField( FieldKey = CDRAppRecordFields.eFieldName, Depth = 1 )]
        public string Name { get; set; }

        [BlobField( FieldKey = CDRAppRecordFields.eFieldAppId, Depth = 1 )]
        public int AppID { get; set; }

        [BlobField( FieldKey = CDRAppRecordFields.eFieldCurrentVersionId, Depth = 1 )]
        public int CurrentVersion { get; set; }

        [BlobField( FieldKey = CDRAppRecordFields.eFieldVersionsRecord, Complex = true, Depth = 1 )]
        public List<AppVersion> Versions { get; private set; }

        [BlobField( FieldKey = CDRAppRecordFields.eFieldFilesystemsRecord, Complex = true, Depth = 1 )]
        public List<FileSystem> FileSystems { get; private set; }

        [BlobField( FieldKey = CDRAppRecordFields.eFieldUserDefinedRecord, Depth = 1 )]
        public Dictionary<string, string> UserDefined { get; private set; }


        public string GetServerFolder()
        {
            if ( UserDefined == null )
                return null;

            string folder = null;
            UserDefined.TryGetValue( "dedicatedserverfolder", out folder, StringComparer.OrdinalIgnoreCase );

            return folder;
        }
    }

    class Sub
    {
        [BlobField( FieldKey = CDRSubRecordFields.eFieldSubId, Depth = 1 )]
        public int SubID { get; set; }

        [BlobField( FieldKey = CDRSubRecordFields.eFieldAppIdsRecord, Depth = 1 )]
        public List<int> AppIDs { get; private set; }

        public bool OwnsApp( int appId )
        {
            return AppIDs.Contains( appId );
        }
    }

    class AppVersion
    {
        [BlobField( FieldKey = CDRAppVersionFields.eFieldVersionId )]
        public uint VersionID { get; set; }

        [BlobField( FieldKey = CDRAppVersionFields.eFieldDepotEncryptionKey )]
        public string DepotEncryptionKey { get; set; }

        [BlobField( FieldKey = CDRAppVersionFields.eFieldIsEncryptionKeyAvailable )]
        public bool IsEncryptionKeyAvailable { get; set; }
    }

    class FileSystem
    {
        [BlobField( FieldKey = CDRAppFilesystemFields.eFieldAppId )]
        public int AppID { get; set; }

        [BlobField( FieldKey = CDRAppFilesystemFields.eFieldMountName )]
        public string Name { get; set; }

        [BlobField( FieldKey = CDRAppFilesystemFields.eFieldPlatform )]
        public string Platform { get; set; }
    }

    class CDRManager
    {
        const string FILE = "cdr.blob";

        static string cdrFile;

        static CDR cdrObj;


        static CDRManager()
        {
            cdrFile = Path.Combine( Application.StartupPath, FILE );
        }


        public static void Update()
        {
            Log.WriteLine( "Updating CDR..." );

            byte[] cdr = null;
            byte[] cdrHash = null;

            if ( File.Exists( cdrFile ) )
            {
                cdr = File.ReadAllBytes( cdrFile );
                cdrHash = CryptoHelper.SHAHash( cdr );
            }

            if ( ServerCache.ConfigServers.Count == 0 )
            {
                Log.WriteLine( "Error: No config servers.\n" );
                return;
            }

            foreach ( var configServer in ServerCache.ConfigServers )
            {
                try
                {
                    var csClient = new ConfigServerClient();
                    csClient.Connect( configServer );

                    byte[] tempCdr = csClient.GetContentDescriptionRecord( cdrHash );

                    if ( tempCdr == null )
                        continue; // couldn't get cdr from config server, try next server

                    if ( tempCdr.Length == 0 )
                    {
                        Log.WriteLine( "Done! Local CDR is up to date!\n" );
                        break;
                    }

                    cdr = tempCdr;
                    File.WriteAllBytes( cdrFile, tempCdr );

                    Log.WriteLine( "Done!\n" );
                    break;
                }
                catch ( Exception ex )
                {
                    Log.WriteLine( "Warning: Unable to download CDR: {0}", ex.Message );
                }
            }

            if ( cdr == null )
            {
                Log.WriteLine( "Error: Unable to download CDR." );
                return;
            }

            Log.WriteLine( "Processing CDR..." );

            using ( var reader = BlobTypedReader<CDR>.Create( new MemoryStream( cdr ) ) )
            {
                reader.Process();

                cdrObj = reader.Target;
            }

            Log.WriteLine( "Done!\n" );
        }


        public static App GetApp( int appId )
        {
            return cdrObj.Apps.Find( app => app.AppID == appId );
        }
        public static Sub GetSub( int subId )
        {
            return cdrObj.Subs.Find( sub => sub.SubID == subId );
        }

        public static IEnumerable<string> GetGamesInRange( int minRange, int maxRange )
        {
            App serverInfo = CDRManager.GetApp( 4 );

            List<string> games = new List<string>();

            foreach ( var fileSystem in serverInfo.FileSystems )
            {
                int appId = fileSystem.AppID;
                string name = fileSystem.Name;

                int suffixPos = name.LastIndexOf( "-win32" );

                if ( suffixPos == -1 )
                    suffixPos = name.LastIndexOf( "-linux" );

                if ( suffixPos > 0 )
                    name = name.Remove( suffixPos );

                if ( appId >= minRange && appId <= maxRange )
                    games.Add( name );

            }

            return games.Distinct();
        }

        public static List<int> GetDepotsForGame( string gameName )
        {
            List<int> appIds = new List<int>();

            App serverInfo = GetApp( 4 );

            PlatformID platform = Environment.OSVersion.Platform;

            string platformSuffix = "";

            if ( platform == PlatformID.Win32NT )
                platformSuffix = "-win32";
            else if ( platform == PlatformID.Unix )
                platformSuffix = "-linux";

            foreach ( var fileSystem in serverInfo.FileSystems )
            {
                string name = fileSystem.Name;

                if ( String.Compare( name, 0, gameName, 0, gameName.Length, true ) == 0 )
                {
                    string suffix = name.Substring( gameName.Length );

                    if ( suffix == "" || suffix == platformSuffix )
                    {
                        appIds.Add( fileSystem.AppID );
                    }
                }
            }

            return appIds;
        }
    }
}
