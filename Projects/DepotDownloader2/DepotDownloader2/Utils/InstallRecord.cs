/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Data;

namespace DepotDownloader2
{
    class InstallRecord : IDisposable
    {
        const string FILE = "install.db";

        string directory;
        string installFile;

        SQLiteConnection connection;

        Dictionary<int, int> versionInfo;


        public InstallRecord( string directory )
        {
            this.directory = directory;
            installFile = Path.Combine( directory, FILE );

            versionInfo = new Dictionary<int, int>();
            connection = new SQLiteConnection( string.Format( "Data Source = {0}; Version = 3;", installFile ) );

            this.Open();
        }


        public void Open()
        {
            if ( connection.State != ConnectionState.Closed )
                return;

            if ( !File.Exists( installFile ) )
            {
                Log.WriteLine( "No installation record at {0}, creating a new one...", directory );
            }

            connection.Open();

            using ( var cmd = connection.CreateCommand() )
            {
                // if the file didn't exist, we'll want to create the initial table format
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS `installrecord` ( `appid` INTEGER NOT NULL, `version` INTEGER NOT NULL, PRIMARY KEY ( `appid` ) );";
                cmd.ExecuteNonQuery();
            }
            

            using ( var cmd = connection.CreateCommand() )
            {
                // populate whatever version info we can
                cmd.CommandText = "SELECT * FROM `installrecord`";

                using ( var reader = cmd.ExecuteReader() )
                {
                    while ( reader.Read() )
                    {
                        int appId = ( int )( long )reader[ "appid" ];
                        int version = ( int )( long )reader[ "version" ];

                        versionInfo[ appId ] = version;
                    }
                }
            }
        }
        public void Close()
        {
            if ( connection.State != ConnectionState.Open )
                return;

            Flush();

            connection.Close();
        }

        public void Flush()
        {
            foreach ( var kvp in versionInfo )
            {
                int appId = kvp.Key;
                int version = kvp.Value;

                // save our loaded data
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = "REPLACE INTO `installrecord` ( `appid`, `version` ) VALUES ( @appid, @version );";

                    cmd.Parameters.Add( new SQLiteParameter( "@appid" ) { Value = appId } );
                    cmd.Parameters.Add( new SQLiteParameter( "@version" ) { Value = version } );

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int? GetAppVersion( int appId )
        {
            if ( versionInfo.ContainsKey( appId ) )
            {
                return versionInfo[ appId ];
            }

            return null;
        }
        public void SetAppVersion( int appId, int version )
        {
            versionInfo[ appId ] = version;
        }

        public void Dispose()
        {
            this.Close();

            connection.Dispose();
        }

    }
}
