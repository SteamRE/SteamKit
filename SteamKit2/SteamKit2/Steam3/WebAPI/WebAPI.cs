using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Collections;

namespace SteamKit2
{
    public sealed class WebAPI
    {

        public sealed class Interface : IDisposable
        {
            WebClient webClient;

            string iface;
            string apiKey;

            const string API_ROOT = "http://api.steampowered.com/";


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
        }

        public static Interface GetInterface( string iface, string apiKey = "" )
        {
            return new Interface( iface, apiKey );
        }
    }

}
