using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ProtoBuf;

namespace SteamKit2
{
    /// <summary>
    /// Utility class for interacting with the Steam Web API.
    /// </summary>
    public sealed partial class WebAPI
    {
        /// <summary>
        /// Represents a response to a WebAPI request.
        /// </summary>
        public sealed class WebAPIResponse<TResponse>
        {
            /// <summary>
            /// Gets the result of the response.
            /// </summary>
            public EResult Result { get; }

            /// <summary>
            /// Gets the body of the response.
            /// </summary>
            public TResponse Body { get; }

            /// <summary>
            /// Constructs <see cref="WebAPIResponse{TResponse}"/> object.
            /// </summary>
            /// <param name="body">The response body.</param>
            /// <param name="result">The result.</param>
            internal WebAPIResponse( EResult result, TResponse body )
            {
                Result = result;
                Body = body;
            }
        }

        /// <summary>
        /// The default base address used for the Steam Web API.
        /// A different base address can be specified in a <see cref="SteamConfiguration"/> object, or
        /// as a function argument where overloads are available.
        /// </summary>
        public static Uri DefaultBaseAddress { get; } = new Uri( "https://api.steampowered.com/", UriKind.Absolute );

        internal static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds( 100 );

        /// <summary>
        /// Represents a single interface that exists within the Web API.
        /// This is a dynamic object that allows function calls to interfaces with minimal code.
        /// </summary>
        public sealed class Interface : IDisposable
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
            public KeyValue Call( string func, int version = 1, Dictionary<string, object?>? args = null )
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
            public KeyValue Call( HttpMethod method, string func, int version = 1, Dictionary<string, object?>? args = null )
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
        }

        /// <summary>
        /// Represents a single interface that exists within the Web API.
        /// This is a dynamic object that allows function calls to interfaces with minimal code.
        /// This version of the <see cref="Interface"/> class makes use of TPL Tasks to provide an asynchronous API.
        /// </summary>
        public sealed partial class AsyncInterface : IDisposable
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

            internal AsyncInterface( HttpClient httpClient, string iface, string apiKey )
            {
                this.httpClient = httpClient;
                this.iface = iface;
                this.apiKey = apiKey;
            }

            /// <summary>
            /// Manually calls the specified Web API function with the provided details.
            /// </summary>
            /// <typeparam name="T">Protobuf type of the response message.</typeparam>
            /// <param name="func">The function name to call.</param>
            /// <param name="version">The version of the function to call.</param>
            /// <param name="args">A dictionary of string key value pairs representing arguments to be passed to the API.</param>
            /// <param name="method">The http request method. Either "POST" or "GET".</param>
            /// <returns>A <see cref="Task{T}"/> that contains object representing the results of the Web API call.</returns>
            /// <exception cref="ArgumentNullException">The function name or request method provided were <c>null</c>.</exception>
            /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
            /// <exception cref="WebAPIRequestException">A network error occurred when performing the request.</exception>
            /// <exception cref="ProtoException">An error occured when parsing the response from the WebAPI.</exception>
            public async Task<T> CallProtobufAsync<T>( HttpMethod method, string func, int version = 1, Dictionary<string, object?>? args = null )
            {
                var response = await CallAsyncInternal( method, func, version, args, "protobuf_raw" );

                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait( false );

                var body = Serializer.Deserialize<T>( stream );

                return body;
            }

            /// <summary>
            /// Manually calls the specified Web API function with the provided details.
            /// </summary>
            /// <typeparam name="TRequest">Protobuf type of the request message.</typeparam>
            /// <typeparam name="TResponse">Protobuf type of the response message.</typeparam>
            /// <param name="func">The function name to call.</param>
            /// <param name="version">The version of the function to call.</param>
            /// <param name="request">A protobuf object representing arguments to be passed to the API.</param>
            /// <param name="method">The http request method. Either "POST" or "GET".</param>
            /// <returns>A <see cref="Task{T}"/> that contains object representing the results of the Web API call.</returns>
            /// <exception cref="ArgumentNullException">The function name or request method provided were <c>null</c>.</exception>
            /// <exception cref="HttpRequestException">An network error occurred when performing the request.</exception>
            /// <exception cref="WebAPIRequestException">A network error occurred when performing the request.</exception>
            /// <exception cref="ProtoException">An error occured when parsing the response from the WebAPI.</exception>
            public async Task<WebAPIResponse<TResponse>> CallProtobufAsync<TResponse, TRequest>( HttpMethod method, string func, TRequest request, int version = 1 )
                where TResponse : IExtensible, new()
                where TRequest : IExtensible, new()
            {
                ArgumentNullException.ThrowIfNull( method );

                ArgumentNullException.ThrowIfNull( func );

                if ( request == null )
                {
                    throw new ArgumentNullException( nameof( request ) );
                }

                using var inputPayload = new MemoryStream();
                Serializer.Serialize<TRequest>( inputPayload, request );

                var base64 = Convert.ToBase64String( inputPayload.ToArray() );
                var args = new Dictionary<string, object?>
                {
                    { "input_protobuf_encoded", base64 },
                };

                var response = await CallAsyncInternal( method, func, version, args, "protobuf_raw" );
                var eresult = EResult.Invalid;

                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait( false );

                if ( response.Headers.TryGetValues( "x-eresult", out var result ) && int.TryParse( result.First(), out var resultInt ) )
                {
                    eresult = ( EResult )resultInt;
                }

                var body = Serializer.Deserialize<TResponse>( stream );
                var apiResponse = new WebAPIResponse<TResponse>( eresult, body );

                return apiResponse;
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
            public async Task<KeyValue> CallAsync( HttpMethod method, string func, int version = 1, Dictionary<string, object?>? args = null )
            {
                var response = await CallAsyncInternal( method, func, version, args, "vdf" );

                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait( false );

                try
                {
                    var kv = new KeyValue();
                    kv.ReadAsText( stream );
                    return kv;
                }
                catch ( Exception ex )
                {
                    throw new InvalidDataException(
                        "An internal error occurred when attempting to parse the response from the WebAPI server. This can indicate a change in the VDF format.",
                        ex
                    );
                }
            }

            private async Task<HttpResponseMessage> CallAsyncInternal( HttpMethod method, string func, int version, Dictionary<string, object?>? args, string expectedFormat )
            {
                ArgumentNullException.ThrowIfNull( method );

                ArgumentNullException.ThrowIfNull( func );

                var formatProvided = false;

                if ( args != null && args.TryGetValue( "format", out var format ) )
                {
                    formatProvided = true;

                    if ( format is not string formatText || formatText != "vdf" )
                    {
                        throw new ArgumentException( $"Unsupported 'format' value '{format}'. Format must either be '{expectedFormat}' or omitted.", nameof( args ) );
                    }
                }

                var paramBuilder = new StringBuilder();

                var paramsGoInQueryString = HttpMethod.Get.Equals( method );

                var urlBuilder = paramsGoInQueryString ? paramBuilder : new StringBuilder();
                urlBuilder.AppendFormat( "{0}/{1}/v{2}", iface, func, version );

                if ( paramsGoInQueryString )
                {
                    urlBuilder.Append( "/?" );
                }

                if ( !string.IsNullOrEmpty( apiKey ) && args != null && !args.ContainsKey( "key" ) )
                {
                    paramBuilder.Append( "key=" );
                    paramBuilder.Append( Uri.EscapeDataString( apiKey ) );
                    paramBuilder.Append( '&' );
                }

                if ( !formatProvided )
                {
                    paramBuilder.Append( "format=" );
                    paramBuilder.Append( expectedFormat );
                }

                if ( args != null )
                {
                    foreach ( var (key, value) in args )
                    {
                        paramBuilder.Append( '&' );
                        paramBuilder.Append( Uri.EscapeDataString( key ) );
                        paramBuilder.Append( '=' );

                        switch ( value )
                        {
                            case null:
                                break;

                            case byte[] byteArrayValue:
                                paramBuilder.Append( HttpUtility.UrlEncode( byteArrayValue ) );
                                break;

                            default:
                                if ( value.ToString() is { } valueString )
                                {
                                    paramBuilder.Append( Uri.EscapeDataString( valueString ) );
                                }
                                break;
                        }
                    }
                }

                using var request = new HttpRequestMessage( method, urlBuilder.ToString() );

                if ( !paramsGoInQueryString )
                {
                    request.Content = new StringContent( paramBuilder.ToString() );
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/x-www-form-urlencoded" );
                }

                var response = await httpClient.SendAsync( request ).ConfigureAwait( false );

                if ( !response.IsSuccessStatusCode )
                {
                    throw new WebAPIRequestException( $"Response status code does not indicate success: {response.StatusCode:D} ({response.ReasonPhrase}).", response );
                }

                return response;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                httpClient.Dispose();
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
            ArgumentNullException.ThrowIfNull( baseAddress );

            ArgumentNullException.ThrowIfNull( iface );

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
            ArgumentNullException.ThrowIfNull( iface );

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
            ArgumentNullException.ThrowIfNull( iface );

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
            ArgumentNullException.ThrowIfNull( baseAddress );

            ArgumentNullException.ThrowIfNull( iface );

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
