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
        static WebAPI()
        {
            ServicePointManager.Expect100Continue = false;
        }

        public sealed class Interface : DynamicObject, IDisposable
        {
            WebClient webClient;

            string iface;
            string apiKey;

            const string API_ROOT = "api.steampowered.com";

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


            public KeyValue Call( string func, int version = 1, Dictionary<string, string> args = null, string method = WebRequestMethods.Http.Get, bool secure = false )
            {
                if ( args == null )
                    args = new Dictionary<string, string>();

                StringBuilder urlBuilder = new StringBuilder();
                StringBuilder paramBuilder = new StringBuilder();

                urlBuilder.Append( secure ? "https://" : "http://" );
                urlBuilder.Append( API_ROOT );
                urlBuilder.AppendFormat( "/{0}/{1}/v{2}", iface, func, version );

                bool isGet = method.Equals( WebRequestMethods.Http.Get, StringComparison.OrdinalIgnoreCase );

                if ( isGet )
                {
                    // if we're doing a GET request, we'll build the params onto the url
                    paramBuilder = urlBuilder;
                    paramBuilder.Append( "/?" ); // start our GET params
                }

                args.Add( "format", "vdf" );

                if ( !string.IsNullOrEmpty( apiKey ) )
                {
                    args.Add( "key", apiKey );
                }

                // append any args
                paramBuilder.Append( string.Join( "&", args.Select( kvp =>
                {
                    string key = WebHelpers.UrlEncode( kvp.Key );
                    string value = kvp.Value; // WebHelpers.UrlEncode( kvp.Value );

                    return string.Format( "{0}={1}", key, value );
                } ) ) );


                byte[] data = null;

                if ( isGet )
                {
                    data = webClient.DownloadData( urlBuilder.ToString() );
                }
                else
                {
                    byte[] postData = Encoding.Default.GetBytes( paramBuilder.ToString() );

                    webClient.Headers.Add( HttpRequestHeader.ContentType, "application/x-www-form-urlencoded" );
                    data = webClient.UploadData( urlBuilder.ToString(), postData );
                }
                
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

                string requestMethod = WebRequestMethods.Http.Get;
                bool secure = false;

                // convert named arguments into key value pairs
                for ( int x = 0 ; x < args.Length ; x++ )
                {
                    string argName = binder.CallInfo.ArgumentNames[ x ];
                    object argValue = args[ x ];

                    if ( argName.Equals( "method", StringComparison.OrdinalIgnoreCase ) )
                    {
                        requestMethod = argValue.ToString();
                        continue;
                    }

                    if ( argName.Equals( "secure", StringComparison.OrdinalIgnoreCase ) )
                    {
                        try
                        {
                            secure = ( bool )argValue;
                        }
                        catch ( InvalidCastException )
                        {
                            throw new ArgumentException( "The parameter 'secure' is a reserved parameter that must be of type bool." );
                        }

                        continue;
                    }

                    apiArgs.Add( argName, argValue.ToString() );
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


                result = Call( functionName, version, apiArgs, requestMethod, secure );

                return true;
            }
        }

        public static Interface GetInterface( string iface, string apiKey = "" )
        {
            return new Interface( iface, apiKey );
        }
    }

}
