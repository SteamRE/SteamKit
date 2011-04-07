using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    static class ClientConfig
    {
        const string CONFIGFILE = "SteamKit.config";

        const byte VERSION = 1;
        const byte VERSION_REQ_SENTRYFILES = 1;
        

        public static Dictionary<string, string> SentryFiles { get; private set; }


        static ClientConfig()
        {
            try
            {
                Load();
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "ClientConfig", "ClientConfig was unable to load client config!\n{0}", ex.ToString() );
            }
        }


        public static void AddSentryFile( string userName, string sentryFileName )
        {
            if ( SentryFiles.ContainsKey( userName ) )
            {
                SentryFiles[ userName ] = sentryFileName;
            }
            else
            {
                SentryFiles.Add( userName, sentryFileName );
            }

            try
            {
                Save();
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "ClientConfig", "AddSentryFile was unable to save client config!\n{0}", ex.ToString() );
            }
        }
        public static string GetSentryFile( string userName )
        {
            try
            {
                Load();
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "ClientConfig", "GetSentryFile was unable to load client config!\n{0}", ex.ToString() );
                return null;
            }

            if ( SentryFiles.ContainsKey( userName ) )
                return SentryFiles[ userName ];

            return null;
        }


        static void Save()
        {
            using ( FileStream fs = new FileStream( CONFIGFILE, FileMode.Create, FileAccess.Write, FileShare.None ) )
            using ( BinaryWriter bw = new BinaryWriter( fs ) )
            {
                bw.Write( VERSION );

                bw.Write( (ushort)SentryFiles.Count );

                foreach ( var kvp in SentryFiles )
                {
                    byte[] key = Encoding.ASCII.GetBytes( kvp.Key );
                    byte[] value = Encoding.ASCII.GetBytes( kvp.Value );

                    bw.Write( ( byte )key.Length );
                    bw.Write( key );

                    bw.Write( ( byte )value.Length );
                    bw.Write( value );
                }

            }
        }
        static void Load()
        {
            SentryFiles = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            using ( FileStream fs = new FileStream( CONFIGFILE, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None ) )
            using ( BinaryReader br = new BinaryReader( fs ) )
            {

                if ( fs.Length == 0 )
                    return;

                byte ver = br.ReadByte();

                if ( ver >= VERSION_REQ_SENTRYFILES )
                {
                    int count = br.ReadUInt16();

                    for ( int x = 0 ; x < count ; ++x )
                    {
                        byte keyLen = br.ReadByte();
                        byte[] key = br.ReadBytes( keyLen );

                        byte valueLen = br.ReadByte();
                        byte[] value = br.ReadBytes( valueLen );

                        string keyStr = Encoding.ASCII.GetString( key );
                        string valueStr = Encoding.ASCII.GetString( value );

                        SentryFiles.Add( keyStr, valueStr );
                    }
                }
            }
        }
    }
}
