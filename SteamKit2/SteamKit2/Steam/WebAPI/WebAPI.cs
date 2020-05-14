using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SteamKit2
{
    /// <summary>
    /// Utility class for interacting with the Steam Web API.
    /// </summary>
    public sealed class WebAPI
    {
        /// <summary>
        /// The default base address used for the Steam Web API.
        /// A different base address can be specified in a <see cref="SteamConfiguration"/> object, or
        /// as a function argument where overloads are available.
        /// </summary>
        public static Uri DefaultBaseAddress { get; } = new Uri("https://api.steampowered.com/", UriKind.Absolute);

        internal static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// Represents a single interface that exists within the Web API.
        /// This is a dynamic object that allows function calls to interfaces with minimal code.
        /// </summary>
        public sealed class Interface : DynamicObject, IDisposable
        {
            readonly AsyncInterface asyncInterface;


            /// <summary>
            /// Gets or sets the timeout value in milliseconds for any web requests made to the WebAPI.
            /// </summary>
            /// <value>
            /// The timeout value in milliseconds. The default value is 100 seconds.
            /// </value>
            public TimeSpan Timeout
            {
                get => asyncInterface.Timeout;
                set => asyncInterface.Timeout = value;
            }


            internal Interface( HttpClient httpClient, string iface, string apiKey )
            {
                asyncInterface = new AsyncInterface( httpClient, iface, apiKey );
            }


            /// <summary>
            /// Manually calls the specified Web API function with the provided details.
            /// </summary>
            /// <param name="func">The function name to call.</param>
            /// <param name="version">The version of the function to call.</param>
            /// <param name="args">A dictionary of string key value pairs representing arguments to be passed to the API.</param>
            /// <returns>A <see cref="KeyValue"/> object representing the results of the Web API call.</returns>
            /// <exception cref="ArgumentNullException">The function name or request method provided were <c>null</c>.</exception>
            /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
            /// <exception cref="WebAPIRequestException">A network error occurred when performing the request.</exception>
            /// <exception cref="InvalidDataException">An error occured when parsing the response from the WebAPI.</exception>
            public KeyValue Call( string func, int version = 1, Dictionary<string, object>? args = null )
                => Call( HttpMethod.Get, func, version, args );


            /// <summary>
            /// Manually calls the specified Web API function with the provided details.
            /// </summary>
            /// <param name="func">The function name to call.</param>
            /// <param name="version">The version of the function to call.</param>
            /// <param name="args">A dictionary of string key value pairs representing arguments to be passed to the API.</param>
            /// <param name="method">The http request method. Either "POST" or "GET".</param>
            /// <returns>A <see cref="KeyValue"/> object representing the results of the Web API call.</returns>
            /// <exception cref="ArgumentNullException">The function name or request method provided were <c>null</c>.</exception>
            /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
            /// <exception cref="WebAPIRequestException">A network error occurred when performing the request.</exception>
            /// <exception cref="InvalidDataException">An error occured when parsing the response from the WebAPI.</exception>
            public KeyValue Call( HttpMethod method, string func, int version = 1, Dictionary<string, object>? args = null )
            {
                var callTask = asyncInterface.CallAsync( method, func, version, args );

                try
                {
                    bool completed = callTask.Wait( Timeout );

                    if ( !completed )
                        throw new TimeoutException( "The WebAPI call timed out" );
                }
                catch ( AggregateException ex ) when ( ex.InnerException != null )
                {
                    // because we're internally using the async interface, any WebExceptions thrown will
                    // be wrapped inside an AggregateException.
                    // since callers don't expect this, we need to unwrap and rethrow the inner exception
                    ExceptionDispatchInfo.Capture( ex.InnerException ).Throw();
                }

                return callTask.Result;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                asyncInterface.Dispose();
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
            /// <exception cref="ArgumentOutOfRangeException">
            /// The function version number specified was out of range.
            /// </exception>
            public override bool TryInvokeMember( InvokeMemberBinder binder, object[] args, out object result )
            {
                bool success = asyncInterface.TryInvokeMember( binder, args, out result );

                if ( success )
                {
                    var resultTask = ( Task<KeyValue> )result;
                    result = resultTask.GetAwaiter().GetResult();
                }

                return success;
            }
        }

        /// <summary>
        /// Represents a single interface that exists within the Web API.
        /// This is a dynamic object that allows function calls to interfaces with minimal code.
        /// This version of the <see cref="Interface"/> class makes use of TPL Tasks to provide an asynchronous API.
        /// </summary>
        public sealed class AsyncInterface : DynamicObject, IDisposable
        {
            internal readonly HttpClient httpClient;

            internal readonly string iface;
            internal readonly string apiKey;

            /// <summary>
            /// Gets or sets the timeout value in milliseconds for any web requests made to the WebAPI.
            /// </summary>
            /// <value>
            /// The timeout value in milliseconds. The default value is 100 seconds.
            /// </value>
            public TimeSpan Timeout
            {
                    get => httpClient.Timeout;
                    set => httpClient.Timeout = value;
            }

            static Regex funcNameRegex = new Regex(
                @"(?<name>[a-zA-Z]+)(?<version>\d*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );

            internal AsyncInterface( HttpClient httpClient, string iface, string apiKey )
            {
                this.httpClient = httpClient;
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
            /// <returns>A <see cref="Task{T}"/> that contains a <see cref="KeyValue"/> object representing the results of the Web API call.</returns>
            /// <exception cref="ArgumentNullException">The function name or request method provided were <c>null</c>.</exception>
            /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
            /// <exception cref="WebAPIRequestException">A network error occurred when performing the request.</exception>
            /// <exception cref="InvalidDataException">An error occured when parsing the response from the WebAPI.</exception>
            public async Task<KeyValue> CallAsync( HttpMethod method, string func, int version = 1, Dictionary<string, object>? args = null )
            {
                if ( method == null )
                {
                    throw new ArgumentNullException( nameof(method) );
                }

                if ( func == null )
                {
                    throw new ArgumentNullException( nameof(func) );
                }

                if ( args == null )
                {
                    args = new Dictionary<string, object>();
                }


                var urlBuilder = new StringBuilder();
                var paramBuilder = new StringBuilder();

                urlBuilder.AppendFormat( "{0}/{1}/v{2}", iface, func, version );

                var isGet = HttpMethod.Get.Equals( method );

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
                    string key = HttpUtility.UrlEncode( kvp.Key );
                    string value;

                    if ( kvp.Value == null )
                    {
                        value = string.Empty;
                    }
                    else if ( kvp.Value is byte[] buffer )
                    {
                        value = HttpUtility.UrlEncode( buffer );
                    }
                    else
                    {
                        value = HttpUtility.UrlEncode( kvp.Value.ToString() );
                    }

                    return string.Format( "{0}={1}", key, value );
                } ) ) );
                
                var request = new HttpRequestMessage( method, urlBuilder.ToString() );

                if ( !isGet )
                {
                    request.Content = new StringContent( paramBuilder.ToString() );
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/x-www-form-urlencoded" );
                }

                var response = await httpClient.SendAsync( request ).ConfigureAwait( false );

                if ( !response.IsSuccessStatusCode )
                {
                    throw new WebAPIRequestException( $"Response status code does not indicate success: {response.StatusCode:D} ({response.ReasonPhrase}).", response );
                }

                var kv = new KeyValue();

                using ( var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait( false ) )
                {
                    try
                    {
                        kv.ReadAsText( stream );
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
                httpClient.Dispose();
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
            /// <exception cref="ArgumentOutOfRangeException">
            /// The function version number specified was out of range.
            /// </exception>
            public override bool TryInvokeMember( InvokeMemberBinder binder, object[] args, out object result )
            {
                IDictionary<string, object> methodArgs;

                if ( args.Length == 1 && binder.CallInfo.ArgumentNames.Count == 0 && args[ 0 ] is IDictionary<string, object> explicitArgs )
                {
                    methodArgs = explicitArgs;
                }
                else if ( binder.CallInfo.ArgumentNames.Count != args.Length )
                {
                    throw new InvalidOperationException( "Argument mismatch in API call. All parameters must be passed as named arguments, or as a single un-named dictionary argument." );
                }
                else
                {
                    methodArgs = Enumerable.Range( 0, args.Length )
                        .ToDictionary( 
                            x => binder.CallInfo.ArgumentNames[ x ],
                            x => args[ x ] );
                }

                var apiArgs = new Dictionary<string, object>();
                var requestMethod = HttpMethod.Get;

                foreach ( var ( argName, argValue ) in methodArgs )
                {
                    // method is a reserved param for selecting the http request method
                    if ( argName.Equals( "method", StringComparison.OrdinalIgnoreCase ) )
                    {
                        requestMethod = new HttpMethod( argValue.ToString() );
                        continue;
                    }
                    // flatten lists
                    else if ( argValue is IEnumerable enumerable && !( argValue is string || argValue is byte[] ) )
                    {
                        int index = 0;

                        foreach ( object value in enumerable )
                        {
                            apiArgs.Add( string.Format( "{0}[{1}]", argName, index++ ), value );
                        }

                        continue;
                    }


                    apiArgs.Add( argName, argValue );
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

                result = CallAsync( requestMethod, functionName, version, apiArgs );

                return true;
            }
        }

        /// <summary>
        /// Retrieves a dynamic handler capable of interacting with the specified interface on the Web API.
        /// </summary>
        /// <param name="baseAddress">The base <see cref="Uri"/> of the Steam Web API.</param>
        /// <param name="iface">The interface to retrieve a handler for.</param>
        /// <param name="apiKey">An optional API key to be used for authorized requests.</param>
        /// <returns>A dynamic <see cref="Interface"/> object to interact with the Web API.</returns>
        public static Interface GetInterface( Uri baseAddress, string iface, string apiKey = "" )
        {
            if ( baseAddress == null )
            {
                throw new ArgumentNullException( nameof(baseAddress) );
            }

            if ( iface == null )
            {
                throw new ArgumentNullException( nameof(iface) );
            }

            return new Interface( CreateDefaultHttpClient( baseAddress ), iface, apiKey );
        }

        /// <summary>
        /// Retrieves a dynamic handler capable of interacting with the specified interface on the Web API.
        /// </summary>
        /// <param name="iface">The interface to retrieve a handler for.</param>
        /// <param name="apiKey">An optional API key to be used for authorized requests.</param>
        /// <returns>A dynamic <see cref="Interface"/> object to interact with the Web API.</returns>
        public static Interface GetInterface( string iface, string apiKey = "" )
        {
            if ( iface == null )
            {
                throw new ArgumentNullException( nameof(iface) );
            }

            return new Interface( CreateDefaultHttpClient( DefaultBaseAddress ), iface, apiKey );
        }

        /// <summary>
        /// Retrieves a dynamic handler capable of interacting with the specified interface on the Web API.
        /// </summary>
        /// <param name="iface">The interface to retrieve a handler for.</param>
        /// <param name="apiKey">An optional API key to be used for authorized requests.</param>
        /// <returns>A dynamic <see cref="AsyncInterface"/> object to interact with the Web API.</returns>
        public static AsyncInterface GetAsyncInterface( string iface, string apiKey = "" )
        {
            if ( iface == null )
            {
                throw new ArgumentNullException( nameof(iface) );
            }

            return new AsyncInterface( CreateDefaultHttpClient( DefaultBaseAddress ), iface, apiKey );
        }

        /// <summary>
        /// Retrieves a dynamic handler capable of interacting with the specified interface on the Web API.
        /// </summary>
        /// <param name="baseAddress">The base <see cref="Uri"/> of the Steam Web API.</param>
        /// <param name="iface">The interface to retrieve a handler for.</param>
        /// <param name="apiKey">An optional API key to be used for authorized requests.</param>
        /// <returns>A dynamic <see cref="AsyncInterface"/> object to interact with the Web API.</returns>
        public static AsyncInterface GetAsyncInterface( Uri baseAddress, string iface, string apiKey = "" )
        {
            if ( baseAddress == null )
            {
                throw new ArgumentNullException( nameof(baseAddress) );
            }

            if ( iface == null )
            {
                throw new ArgumentNullException( nameof(iface) );
            }
            
            return new AsyncInterface( CreateDefaultHttpClient( baseAddress ), iface, apiKey );
        }

        static HttpClient CreateDefaultHttpClient( Uri baseAddress )
        {
            var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = DefaultTimeout
            };

            return client;
        }
    }
}
