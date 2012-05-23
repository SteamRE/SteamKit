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
    /// <summary>
    /// Utility class for interacting with the Steam Web API.
    /// </summary>
    public sealed class WebAPI
    {
        static WebAPI()
        {
            // stop WebClient from inserting this header into requests
            // the backend doesn't like it
            ServicePointManager.Expect100Continue = false;

            // todo: perhaps toggle this per request instead of globally disabling it?
        }

        /// <summary>
        /// Represents a single interface that exists within the Web API.
        /// This is a dynamic object that allows function calls to interfaces with minimal code.
        /// </summary>
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


            /// <summary>
            /// Manually calls the specified Web API function with the provided details.
            /// </summary>
            /// <param name="func">The function name to call.</param>
            /// <param name="version">The version of the function to call.</param>
            /// <param name="args">A dictionary of string key value pairs representing arguments to be passed to the API.</param>
            /// <param name="method">The http request method. Either "POST" or "GET".</param>
            /// <param name="secure">if set to <c>true</c> this method will be called through the secure API.</param>
            /// <returns>A <see cref="KeyValue"/> object representing the results of the Web API call.</returns>
            /// <exception cref="ArgumentNullException">The function name or request method provided were <c>null</c>.</exception>
            /// <exception cref="WebException">An network error occurred when performing the request.</exception>
            /// <exception cref="InvalidDataException">An error occured when parsing the response from the WebAPI.</exception>
            public KeyValue Call( string func, int version = 1, Dictionary<string, string> args = null, string method = WebRequestMethods.Http.Get, bool secure = false )
            {
                if ( func == null )
                    throw new ArgumentNullException( "func" );

                if ( args == null )
                    args = new Dictionary<string, string>();

                if ( method == null )
                    throw new ArgumentNullException( "method" );

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
                    // TODO: the WebAPI is a special snowflake that needs to appropriately handle url encoding
                    // this is in contrast to the steam3 content server APIs which use an entirely different scheme of encoding

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
                {
                    try
                    {
                        kv.ReadAsText( ms );
                    }
                    catch ( Exception ex )
                    {
                        throw new InvalidDataException(
                            "An internal error occurred when attempting to parse the response from the WebAPI server. This can indicate a change in the VDF format.",
                            ex
                        );
                    }
                }

                return kv;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                webClient.Dispose();
            }

            /// <summary>
            /// Provides the implementation for operations that invoke a member.
            /// Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can
            /// override this method to specify dynamic behavior for operations such as calling a method.
            /// This method should not be called directly, it is  called through dynamic method calls.
            /// </summary>
            /// <param name="binder">
            /// Provides information about the dynamic operation.
            /// The binder.Name property provides the name of the member on which the dynamic operation is performed.
            /// For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the
            /// class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleMethod".
            /// The binder.IgnoreCase property specifies whether the member name is case-sensitive.
            /// </param>
            /// <param name="args">
            /// The arguments that are passed to the object member during the invoke operation. For example,
            /// for the statement sampleObject.SampleMethod(100), where sampleObject is derived from the
            /// <see cref="T:System.Dynamic.DynamicObject"/> class, the first argument to <paramref name="args"/> is equal to 100.
            /// </param>
            /// <param name="result">The result of the member invocation.</param>
            /// <returns>
            /// true if the operation is successful; otherwise, false. If this method returns false, the run-time
            /// binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Dynamic method is called with non-named argument.
            /// All parameters must be passed as name arguments to API calls.
            /// - or -
            /// The dynamic method name was not in the correct format.
            /// All API function calls must be in the format 'FunctionName###' where the optional ###'s represent a version number.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// The reserved named parameter 'secure' was not a boolean value.
            /// This parameter is used when requests must go through the secure API.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// The function version number specified was out of range.
            /// </exception>
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

                    // method is a reserved param for selecting the http request method
                    if ( argName.Equals( "method", StringComparison.OrdinalIgnoreCase ) )
                    {
                        requestMethod = argValue.ToString();
                        continue;
                    }
                    // secure is another reserved param for selecting the http or https apis
                    else if ( argName.Equals( "secure", StringComparison.OrdinalIgnoreCase ) )
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
                    // flatten lists
                    else if ( argValue is IEnumerable )
                    {
                        int index = 0;
                        IEnumerable enumerable = argValue as IEnumerable;

                        foreach (object value in enumerable)
                        {
                            apiArgs.Add( String.Format( "{0}[{1}]", argName, index++ ), value.ToString() );
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
                    // the regex matches digits, but we should check for absurdly large numbers
                    if ( !int.TryParse( versionString, out version ) )
                    {
                        throw new ArgumentOutOfRangeException( "version", "The function version number supplied was invalid or out of range." );
                    }
                }

                result = Call( functionName, version, apiArgs, requestMethod, secure );

                return true;
            }
        }

        /// <summary>
        /// Retreives a dynamic handler capable of interacting with the specified interface on the Web API.
        /// </summary>
        /// <param name="iface">The interface to retrieve a handler for.</param>
        /// <param name="apiKey">An optional API key to be used for authorized requests.</param>
        /// <returns>A dynamic <see cref="Interface"/> object to interact with the Web API.</returns>
        public static Interface GetInterface( string iface, string apiKey = "" )
        {
            return new Interface( iface, apiKey );
        }
    }

}
