﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class WebAPIFacts
    {
        [Fact]
        public void WebAPIHasDefaultTimeout()
        {
            var iface = WebAPI.GetInterface( new Uri( "https://whatever/" ), "ISteamWhatever" );

            Assert.Equal( iface.Timeout, TimeSpan.FromSeconds( 100 ) );
        }

        [Fact]
        public void WebAPIAsyncHasDefaultTimeout()
        {
            var iface = WebAPI.GetAsyncInterface( new Uri( "https://whatever/" ), "ISteamWhatever" );

            Assert.Equal( iface.Timeout, TimeSpan.FromSeconds( 100 ) );
        }

#if DEBUG
        [Fact]
        public void SteamConfigWebAPIInterface()
        {
            var config = SteamConfiguration.Create( b =>
                b.WithWebAPIBaseAddress( new Uri( "http://example.com" ) )
                 .WithWebAPIKey( "hello world" ) );

            var iface = config.GetAsyncWebAPIInterface( "TestInterface" );

            Assert.Equal( "TestInterface", iface.iface );
            Assert.Equal( "hello world", iface.apiKey );
            Assert.Equal( new Uri( "http://example.com" ), iface.httpClient.BaseAddress );
        }
#endif

        [Fact]
        public async Task ThrowsWebAPIRequestExceptionIfRequestUnsuccessful()
        {
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( new ServiceUnavailableHttpMessageHandler() ) ) );
            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            await Assert.ThrowsAsync<WebAPIRequestException>( () => ( Task )iface.PerformFooOperation() );
        }

        [Fact]
        public async Task ThrowsOnIncorrectFormatInArgsProvided()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            WebAPI.AsyncInterface iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
                [ "format" ] = "json"
            };

            await Assert.ThrowsAsync<ArgumentException>( () => iface.CallAsync( HttpMethod.Get, "GetFoo", args: args ) );
        }

        [Fact]
        public async Task DoesntThrowWhenCorrectFormatInArgsProvided()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            WebAPI.AsyncInterface iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
                [ "format" ] = "vdf"
            };

            await iface.CallAsync( HttpMethod.Get, "GetFoo", args: args );
        }

        [Fact]
        public async Task DoesntThrowWhenKeyInArgsProvided()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithWebAPIKey( "test1" ).WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            WebAPI.AsyncInterface iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
                [ "key" ] = "test2"
            };

            await iface.CallAsync( HttpMethod.Get, "GetFoo", args: args );

            Assert.Equal( "test2", args[ "key" ] );
        }

        [Fact]
        public async Task DoesntThrowOnArgumentsReuse()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            WebAPI.AsyncInterface iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar"
            };

            await iface.CallAsync( HttpMethod.Get, "GetFoo", args: args );
            await iface.CallAsync( HttpMethod.Get, "GetFoo", args: args );
        }

        [Fact]
        public async Task UsesArgsAsQueryStringParams()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var handlerCalled = false;
            hookableHandler.OnRequest = request =>
            {
                Assert.NotNull( request );
                Assert.Equal( HttpMethod.Get, request.Method );
                Assert.Equal( "/IFooService/PerformFooOperation/v2/", request.RequestUri.AbsolutePath );

                var values = HttpUtility.ParseQueryString( request.RequestUri.Query );
                Assert.Equal( 3, values.Count );
                Assert.Equal( "foo", values[ "f" ] );
                Assert.Equal( "bar", values[ "b" ] );
                Assert.Equal( "vdf", values[ "format" ] );

                handlerCalled = true;
                return Task.CompletedTask;
            };

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
            };

            var response = await iface.PerformFooOperation2( args );
            Assert.True( handlerCalled );
        }

        [Fact]
        public async Task SupportsNullArgsDictionary()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var handlerCalled = false;
            hookableHandler.OnRequest = request =>
            {
                Assert.NotNull( request );
                Assert.Equal( HttpMethod.Get, request.Method );
                Assert.Equal( "/IFooService/PerformFooOperation/v2/", request.RequestUri.AbsolutePath );

                var values = HttpUtility.ParseQueryString( request.RequestUri.Query );
                Assert.Single( values );
                Assert.Equal( "vdf", values[ "format" ] );

                handlerCalled = true;
                return Task.CompletedTask;
            };

            var args = default( Dictionary<string, object> );
            var response = await iface.CallAsync( HttpMethod.Get, "PerformFooOperation", 2, args );
            Assert.True( handlerCalled );
        }

        [Fact]
        public async Task UsesSingleParameterArgumentsDictionary()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
                [ "method" ] = "PUT"
            };

            var handlerCalled = false;
            hookableHandler.OnRequest = async request =>
            {
                Assert.NotNull( request );
                Assert.Equal( "/IFooService/PerformFooOperation/v2/", request.RequestUri.AbsolutePath );
                Assert.Equal( HttpMethod.Put, request.Method );

                var content = await request.Content.ReadAsStringAsync( TestContext.Current.CancellationToken ); // This technically should be ReadAsFormDataAsync
                var formData = HttpUtility.ParseQueryString( content );
                Assert.Equal( 3, formData.Count );
                Assert.Equal( "foo", formData[ "f" ] );
                Assert.Equal( "bar", formData[ "b" ] );
                Assert.Equal( "vdf", formData[ "format" ] );

                handlerCalled = true;
            };

            var response = await iface.PerformFooOperation2( args );
            Assert.True( handlerCalled );
        }

        [Fact]
        public async Task IncludesApiKeyInParams()
        {
            using var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c
                .WithHttpClientFactory( () => new HttpClient( hookableHandler ) )
                .WithWebAPIKey( "MySecretApiKey" ) );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
            };

            var handlerCalled = false;
            hookableHandler.OnRequest = request =>
            {
                Assert.NotNull( request );
                Assert.Equal( HttpMethod.Get, request.Method );
                Assert.Equal( "/IFooService/PerformFooOperation/v2/", request.RequestUri.AbsolutePath );

                var values = HttpUtility.ParseQueryString( request.RequestUri.Query );
                Assert.Equal( 4, values.Count );
                Assert.Equal( "MySecretApiKey", values[ "key" ] );
                Assert.Equal( "foo", values[ "f" ] );
                Assert.Equal( "bar", values[ "b" ] );
                Assert.Equal( "vdf", values[ "format" ] );

                handlerCalled = true;
                return Task.CompletedTask;
            };

            var response = await iface.PerformFooOperation2( args );
            Assert.True( handlerCalled );
        }

        sealed class ServiceUnavailableHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
                => Task.FromResult( new HttpResponseMessage( HttpStatusCode.ServiceUnavailable ) );
        }

        sealed class HookableHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, Task> OnRequest { get; set; }

            protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                if ( OnRequest is { } handler )
                {
                    await handler( request );
                }

                return new HttpResponseMessage( HttpStatusCode.OK )
                {
                    Content = new ByteArrayContent( [] )
                };
            }
        }
    }
}
