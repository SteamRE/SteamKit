using System;
using System.Collections.Generic;
using System.Text;
using SteamKit2;
using System.IO;
using System.Reflection;
using System.Linq;
using SqlNet;
using System.Diagnostics;

namespace CDRUpdater
{
    class Program
    {
        const string HASHFILE = "cdr_hash.bin";

        static void Main( string[] args )
        {
            DebugLog.Write( "CDR updater running!\n" );

            DebugLog.Write( "Loading config...\n" );
            Config cfg = null;
            try
            {
                cfg = Config.Load( "config.xml" );
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to load config file: {0}\n", ex.ToString() );
                return;
            }


            DebugLog.Write( "Downloading CDR...\n" );
            byte[] cdr = null;
            try
            {
                byte[] hash = null;
                try
                {
                    hash = File.ReadAllBytes( HASHFILE );
                }
                catch ( Exception ex2 )
                {
                    DebugLog.Write( "Warning: Unable to read cdr hashfile: {0}\n", ex2.ToString() );
                }

                cdr = Downloader.DownloadCDR( hash );

                if ( cdr == null || cdr.Length == 0 )
                {
                    DebugLog.Write( "No new CDR. All done!\n\n" );
                    return;
                }
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to download CDR: {0}\n", ex.ToString() );
                return;
            }


            DebugLog.Write( "Parsing blob...\n" );
            Blob blob = null;
            try
            {
                blob = new Blob( cdr );
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to parse blob: {0}\n", ex.ToString() );
                return;
            }


            DebugLog.Write( "Parsing CDR...\n" );
            CDR newCdr = null;
            try
            {
                newCdr = BlobReader.ReadFromBlob<CDR>( blob );

                byte[] hash = CryptoHelper.SHAHash( cdr );
                newCdr.Hash = Utils.HexEncode( hash );

                try
                {
                    File.WriteAllBytes( HASHFILE, hash );
                }
                catch ( Exception ex2 )
                {
                    DebugLog.Write( "Warning: Unable to write to hashfile: {0}\n", ex2.ToString() );
                }
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to parse CDR: {0}\n", ex.ToString() );
                return;
            }


            DebugLog.Write( "Connecting to SQL...\n" );
            DatabaseInfo dbInfo = cfg.DatabaseInfo;
            Sql sql = new Sql();
            try
            {
                sql.Connect( dbInfo.Host, dbInfo.Username, dbInfo.Password, dbInfo.Database );
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to connect to the DB: {0}\n", ex.ToString() );
                return;
            }

            DebugLog.Write( "Running insertions...\n" );
            try
            {
                sql.CreateTable( typeof( CDR ), true );

                var reader = sql.Select( string.Format( "SELECT `Hash` FROM `CDRList` WHERE `Hash` = '{0}'", newCdr.Hash ) );
                if ( reader != null )
                {
                    reader.Close();
                    DebugLog.Write( "CDR already in DB. All done!\n\n" );
                    return;
                }
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to create CDR table: {0}\n", ex.ToString() );
                return;
            }

            try
            {
                sql.Insert( newCdr );
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to create insertion commands: {0}\n", ex.ToString() );
                return;
            }

            DebugLog.Write( "Flushing to database...\n" );
            try
            {
                sql.FlushInsertions();
            }
            catch ( Exception ex )
            {
                DebugLog.Write( "Unable to flush to database: {0}\n", ex.ToString() );
                return;
            }

            DebugLog.Write( "All done!\n\n" );

        }
    }
}
