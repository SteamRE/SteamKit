using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Collections;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace SteamKit2
{
    public sealed class WebAPI
    {

        public sealed class Interface : DynamicObject, IDisposable
        {
            WebClient webClient;

            string iface;
            string apiKey;

            const string API_ROOT = "http://api.steampowered.com/";

            static Regex funcNameRegex = new Regex(
                @"(?<name>[a-zA-Z]+)(?<version>\d*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );


            internal Interface( string iface, string apiKey )
            {
                webClient = new WebClient();

                this.iface = iface;
                this.apiKey = apiKey;
            }


            public KeyValue Call( string func, int version = 1, Dictionary<string, string> args = null )
            {
                StringBuilder urlBuilder = new StringBuilder();

                urlBuilder.Append( API_ROOT );
                urlBuilder.AppendFormat( "{0}/{1}/v{2}/?format=vdf", iface, func, version );

                if ( !string.IsNullOrEmpty( apiKey ) )
                {
                    urlBuilder.AppendFormat( "&key={0}", apiKey );
                }

                if ( args != null )
                {
                    foreach ( var kvp in args )
                    {
                        string key = WebHelpers.UrlEncode( kvp.Key );
                        string value = WebHelpers.UrlEncode( kvp.Value );

                        urlBuilder.AppendFormat( "&{0}={1}", key, value );
                    }
                }

                byte[] data = webClient.DownloadData( urlBuilder.ToString() );
                
                KeyValue kv = new KeyValue();

                using ( var ms = new MemoryStream( data ) )
                    kv.ReadAsText( ms );

                return kv;
            }

            public void Dispose()
            {
                webClient.Dispose();
            }

            public override bool TryInvokeMember( InvokeMemberBinder binder, object[] args, out object result )
            {
                if ( binder.CallInfo.ArgumentNames.Count != args.Length )
                {
                    throw new InvalidOperationException( "Argument mismatch in API call. All parameters must be passed as named arguments." );
                }

                var apiArgs = new Dictionary<string, string>();

                // convert named arguments into key value pairs
                for ( int x = 0 ; x < args.Length ; x++ )
                {
                    apiArgs.Add( binder.CallInfo.ArgumentNames[ x ], args[ x ].ToString() );
                }

                Match match = funcNameRegex.Match( binder.Name );

                if ( !match.Success )
                {
                    throw new InvalidOperationException(
                        "The called API function was invalid. It must be in the form of 'FunctionName###' where the optional ### represent the function version."
                    );
                }

                string functionName = match.Groups[ "name" ].Value;

                int version = 1; // assume version 1 unless specified
                string versionString = match.Groups[ "version" ].Value;

                if ( !string.IsNullOrEmpty( versionString ) )
                {
                    version = int.Parse( versionString );
                }


                result = Call( functionName, version, apiArgs );

                return true;
            }
        }

        public static Interface GetInterface( string iface, string apiKey = "" )
        {
            return new Interface( iface, apiKey );
        }
    }

}
