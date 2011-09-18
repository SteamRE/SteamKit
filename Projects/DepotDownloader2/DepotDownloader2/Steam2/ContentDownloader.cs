/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using SteamKit2;

namespace DepotDownloader2
{
    class ContentDownloader
    {
        public static void Install( int depotId )
        {
            if ( !Directory.Exists( Options.Directory ) )
            {
                Log.WriteVerbose( "Creating install directory: {0}", Options.Directory );
                Directory.CreateDirectory( Options.Directory );
            }

            using ( var installRecord = new InstallRecord( Options.Directory ) )
            {
                int installedVersion = installRecord.GetAppVersion( depotId ) ?? 0;

                App depotInfo = CDRManager.GetApp( depotId );

                int latestVersion = depotInfo.CurrentVersion;
                string name = depotInfo.Name;

                if ( installedVersion > latestVersion )
                {
                    Log.WriteLine( "{0} (installed: {1}, latest: {2}) is newer than the CDR!", name, installedVersion, latestVersion );
                    return;
                }

                IPEndPoint storageServer = GetStorageServer( depotId, latestVersion );

                if ( storageServer == null )
                {
                    Log.WriteLine( "Error: Unable to find content server for depot {0}, version {1}", depotId, latestVersion );
                    return;
                }

                var steamSub = CDRManager.GetSub( 0 );

                if ( steamSub == null || !steamSub.OwnsApp( depotId ) )
                {
                    Log.WriteLine( "Error: This game is not available on the steam subscription." );
                    Log.WriteLine( "Authenticated downloads are not supported." );

                    return;
                }

                bool bDidUpdate = Download( depotId, installedVersion, latestVersion );

                Console.WriteLine();

                if ( bDidUpdate )
                {
                    installRecord.SetAppVersion( depotId, latestVersion );
                    installRecord.Flush();
                }
            }
        }

        public static bool Download( int depotId, int fromVersion, int toVersion )
        {
            App depotInfo = CDRManager.GetApp( depotId );

            string installDir = Options.Directory;

            string serverFolder = depotInfo.GetServerFolder();
            if ( serverFolder != null )
            {
                installDir = Path.Combine( installDir, serverFolder );
            }

            bool bFullUpdate = false;

            // version 0 means we have no install record
            if ( fromVersion == 0 )
                bFullUpdate = true;

            Log.WriteLine( "Updating \"{0}\" from version {1} to {2}...", depotInfo.Name, fromVersion, toVersion );

            Log.WriteVerbose( "Downloading manifest for {0}...", fromVersion );
            var fromManifest = DownloadManifest( depotId, fromVersion );

            // if we couldn't get a manifest for the local installed version, we can't perform a diff
            if ( fromManifest == null )
                bFullUpdate = true;

            Log.WriteVerbose( "Downloading manifest for {0}...", toVersion );
            var toManifest = DownloadManifest( depotId, toVersion );

            if ( toManifest == null )
            {
                Log.WriteLine( "Unable to download. Install cancelled." );
                return false;
            }

            List<Steam2Manifest.Node> allFiles = new List<Steam2Manifest.Node>();

            if ( bFullUpdate )
            {
                Log.WriteVerbose( "Performing full update!" );

                // we want everything except the directories. those are handled later
                var newFiles = toManifest.Nodes.Where( node => node.FileID != -1 );

                // if we're doing a full update, we'll just download everything
                allFiles.AddRange( newFiles );
            }
            else
            {
                // otherwise lets perform a diff and see what we need

                Log.WriteVerbose( "Performing diff from {0} to {1}...", fromVersion, toVersion );
                uint[] updates = DownloadUpdates( depotId, fromVersion, toVersion );

                if ( updates == null )
                {
                    Log.WriteLine( "Error: Unable to perform diff. Cancelling." );
                    return false;
                    
                    // TODO: perhaps attempt a full update?
                    // this can be costly, but i've yet to see any depot that doesn't have a diff
                }

                // find the nodes that updated in the old manifest
                var oldUpdatedNodes = updates.Select( id => fromManifest.Nodes.Find( node => node.FileID == id ) );

                // get the list of new nodes the old ones correspond to
                var newUpdatedNodes = oldUpdatedNodes.Select( oldNode => toManifest.Nodes.Find( node => oldNode.FullName == node.FullName ) ).ToList();

                // find all new files between the two manifest versions, and exclude directories
                var newFileNodes = toManifest.Nodes
                    .Except( fromManifest.Nodes, ( left, right ) => StringComparer.OrdinalIgnoreCase.Equals( left.FullName, right.FullName ) )
                    .Where( node => node.FileID != -1 )
                    .ToList();

                if ( fromVersion == toVersion )
                {
                    // if we're not performing an update, clear the new/update lists, and only use the reaquire list
                    newUpdatedNodes.Clear();
                    newFileNodes.Clear();
                }

                // find missing files we need to redownload
                var requireNodes = toManifest.Nodes.Where( node =>
                {
                    if ( node.FileID == -1 )
                        return false;

                    string filePath = Path.Combine( installDir, node.FullName );

                    return !File.Exists( filePath );
                } );

                Log.WriteVerbose( "{0} updated files, {1} new files, {2} to reaquire.", newUpdatedNodes.Count, newFileNodes.Count, requireNodes.Count() );

                allFiles.AddRange( newUpdatedNodes );
                allFiles.AddRange( newFileNodes );
                allFiles.AddRange( requireNodes );
            }

            // grab all the directory nodes from the latest manifest
            var directoryNodes = toManifest.Nodes.FindAll( node => node.FileID == -1 );

            // precreate all directories we'll need
            foreach ( var dirNode in directoryNodes )
            {
                string fullPath = Path.Combine( installDir, dirNode.FullName );

                if ( !Directory.Exists( fullPath ) )
                {
                    Directory.CreateDirectory( fullPath );
                }
            }

            // null nodes can happen if the diff is missing a file because it was removed in a newer version
            // for now we remove them from the download list, and the files don't be deleted
            // TODO: change this behavior?
            allFiles = allFiles.Where( node => node != null ).ToList();

            return DownloadFiles( depotId, toVersion, allFiles, installDir );
        }

        static bool DownloadFiles( int depotId, int toVersion, List<Steam2Manifest.Node> allFiles, string installDir )
        {
            var server = GetStorageServer( depotId, toVersion );

            if ( server == null )
            {
                Log.WriteLine( "Error: Unable to find content server for depot {0}, version {1}", depotId, toVersion );
                return false;
            }

            ContentServerClient csClient = new ContentServerClient();

            try
            {
                csClient.Connect( server );

                using ( var storageSession = csClient.OpenStorage( ( uint )depotId, ( uint )toVersion ) )
                {
                    for ( int x = 0 ; x < allFiles.Count ; ++x )
                    {
                        var node = allFiles[ x ];

                        float perc = ( ( float )x / ( float )allFiles.Count ) * 100.0f;

                        DownloadFile( storageSession, node, perc, installDir );
                    }

                }
            }
            catch ( Exception ex )
            {
                Log.WriteLine( "Error: Unable to install: {0}", ex.Message );
                return false; // a failure in any step of the download process is a complete failure for the download
            }

            csClient.Disconnect();

            return true;
        }
        static void DownloadFile( ContentServerClient.StorageSession storageSession, Steam2Manifest.Node node, float perc, string installDir )
        {
            string downloadPath = Path.Combine( installDir, node.FullName );

            bool isConfigFile = ( node.Attributes & Steam2Manifest.Node.Attribs.UserConfigurationFile ) > 0;
            bool isEncrypted = ( node.Attributes & Steam2Manifest.Node.Attribs.EncryptedFile ) > 0;

            FileInfo fi = new FileInfo( downloadPath );

            // TODO: we're downloading every file right now
            // but this behavior may change in the future
            /*
            if ( fi.Exists && fi.Length == node.SizeOrCount && !Options.Verify )
            {
                // we can skip downloading if the filesize is the same
                // TODO: investigate downloading checksums from the content server
                return;
            }
            */

            // don't download config files if they already exist
            if ( fi.Exists && isConfigFile )
                return;

            if ( isEncrypted )
            {
                Log.WriteLine( "Warning: file {1}: {0} is encrypted! Not downloaded.", node.FileID, node.FullName );
                return;
            }

            Log.WriteLine( " {0:00.00}%\t{1}", perc, downloadPath );

            var file = storageSession.DownloadFile( node, ContentServerClient.StorageSession.DownloadPriority.High );
            File.WriteAllBytes( downloadPath, file );
        }

        static Steam2Manifest DownloadManifest( int depotId, int depotVersion )
        {
            var server = GetStorageServer( depotId, depotVersion );

            if ( server == null )
                return null;

            Steam2Manifest manifest = null;
            ContentServerClient csClient = new ContentServerClient();

            try
            {
                csClient.Connect( server );

                using ( var storageSession = csClient.OpenStorage( ( uint )depotId, ( uint )depotVersion, Options.CellID ) )
                {
                    manifest = storageSession.DownloadManifest();
                }
            }
            catch ( Exception ex )
            {
                Log.WriteVerbose( "Warning: Unable to download manifest: {0}", ex.Message );
            }

            csClient.Disconnect();

            return manifest;
        }
        static uint[] DownloadUpdates( int depotId, int fromVersion, int toVersion )
        {
            var server = GetStorageServer( depotId, toVersion );

            if ( server == null )
                return null;

            uint[] updates = null;
            ContentServerClient csClient = new ContentServerClient();

            try
            {
                csClient.Connect( server );


                using ( var storageSession = csClient.OpenStorage( ( uint )depotId, ( uint )toVersion, Options.CellID ) )
                {
                    updates = storageSession.DownloadUpdates( ( uint )fromVersion );
                }
            }
            catch ( Exception ex )
            {
                Log.WriteVerbose( "Warning: Unable to download updates: {0}", ex.Message );
            }

            csClient.Disconnect();

            return updates;
        }
        static byte[] DownloadChecksums( int depotId, int toVersion )
        {
            var server = GetStorageServer( depotId, toVersion );

            if ( server == null )
                return null;

            byte[] checksums = null;
            ContentServerClient csClient = new ContentServerClient();

            try
            {
                csClient.Connect( server );


                using ( var storageSession = csClient.OpenStorage( ( uint )depotId, ( uint )toVersion, Options.CellID ) )
                {
                    checksums = storageSession.DownloadChecksums();
                }
            }
            catch ( Exception ex )
            {
                Log.WriteVerbose( "Warning: Unable to download updates: {0}", ex.Message );
            }

            csClient.Disconnect();

            return checksums;
        }

        static IPEndPoint GetStorageServer( int depotId, int depotVersion )
        {
            if ( ServerCache.CSDSServers.Count == 0 )
            {
                Log.WriteLine( "Error: No CSDS servers!" );
                return null;
            }

            foreach ( var csdsServer in ServerCache.CSDSServers )
            {
                ContentServerDSClient csdsClient = new ContentServerDSClient();
                csdsClient.Connect( csdsServer );

                var contentServers = csdsClient.GetContentServerList( ( uint )depotId, ( uint )depotVersion, ( uint )Options.CellID );

                if ( contentServers == null )
                {
                    Log.WriteVerbose( "Warning: CSDS {0} rejected depot {1}, version {2}", csdsServer, depotId, depotVersion );
                    continue;
                }

                if ( contentServers.Length == 0 )
                {
                    Log.WriteVerbose( "Warning: CSDS {0} had no servers for depot {1}, version {2}", csdsServer, depotId, depotVersion );
                    continue;
                }

                return contentServers.Aggregate( ( bestMin, x ) => ( bestMin == null || ( x.Load <= bestMin.Load ) ) ? x : bestMin ).StorageServer;
            }

            return null;
        }
    }
}
