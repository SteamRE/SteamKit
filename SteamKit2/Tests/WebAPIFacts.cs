using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class WebAPIFacts
    {
        [TestMethod]
        public void WebAPIHasDefaultTimeout()
        {
            var iface = WebAPI.GetInterface( new Uri("https://whatever/"), "ISteamWhatever" );

            Assert.AreEqual( iface.Timeout, TimeSpan.FromSeconds( 100 ) );
        }

        [TestMethod]
        public void WebAPIAsyncHasDefaultTimeout()
        {
            var iface = WebAPI.GetAsyncInterface( new Uri("https://whatever/"), "ISteamWhatever" );

            Assert.AreEqual( iface.Timeout, TimeSpan.FromSeconds( 100 ) );
        }

        [TestMethod]
        public void SteamConfigWebAPIInterface()
        {
            var config = SteamConfiguration.Create(b =>
                b.WithWebAPIBaseAddress(new Uri("http://example.com"))
                 .WithWebAPIKey("hello world"));

            var iface = config.GetAsyncWebAPIInterface("TestInterface");

            Assert.AreEqual("TestInterface", iface.iface);
            Assert.AreEqual("hello world", iface.apiKey);
            Assert.AreEqual(new Uri("http://example.com"), iface.httpClient.BaseAddress);
        }

        [TestMethod]
        public async Task ThrowsWebAPIRequestExceptionIfRequestUnsuccessful()
        {
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( new ServiceUnavailableHttpMessageHandler() ) ) );
            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" ); 

           await Assert.ThrowsExceptionAsync<WebAPIRequestException>(() => (Task)iface.PerformFooOperation());
        }
        
        [TestMethod]
        public async Task ThrowsOnIncorrectFormatInArgsProvided()
        {
            var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            WebAPI.AsyncInterface iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
                [ "format" ] = "json"
            };
            
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => iface.CallAsync( HttpMethod.Get, "GetFoo", args: args ));
        }
        
        [TestMethod]
        public async Task DoesntThrowWhenCorrectFormatInArgsProvided()
        {
            var hookableHandler = new HookableHandler();
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
        
        [TestMethod]
        public async Task DoesntThrowWhenKeyInArgsProvided()
        {
            var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithWebAPIKey( "test1" ).WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            WebAPI.AsyncInterface iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
                [ "key" ] = "test2"
            };

            await iface.CallAsync( HttpMethod.Get, "GetFoo", args: args );
            
            Assert.AreEqual( "test2", args["key"] );
        }
        
        [TestMethod]
        public async Task DoesntThrowOnArgumentsReuse()
        {
            var hookableHandler = new HookableHandler();
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

        [TestMethod]
        public async Task UsesArgsAsQueryStringParams()
        {
            var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var handlerCalled = false;
            hookableHandler.OnRequest = request =>
            {
                Assert.IsNotNull( request );
                Assert.AreEqual( HttpMethod.Get, request.Method );
                Assert.AreEqual( "/IFooService/PerformFooOperation/v2/", request.RequestUri.AbsolutePath );

                var values = request.RequestUri.ParseQueryString();
                Assert.AreEqual( 3, values.Count );
                Assert.AreEqual( "foo", values[ "f" ] );
                Assert.AreEqual( "bar", values[ "b" ] );
                Assert.AreEqual( "vdf", values[ "format" ] );

                handlerCalled = true;
                return Task.CompletedTask;
            };

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
            };

            var response = await iface.PerformFooOperation2( args );
            Assert.IsTrue( handlerCalled );
        }

        [TestMethod]
        public async Task SupportsNullArgsDictionary()
        {
            var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( hookableHandler ) ) );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var handlerCalled = false;
            hookableHandler.OnRequest = request =>
            {
                Assert.IsNotNull( request );
                Assert.AreEqual( HttpMethod.Get, request.Method );
                Assert.AreEqual( "/IFooService/PerformFooOperation/v2/", request.RequestUri.AbsolutePath );

                var values = request.RequestUri.ParseQueryString();
                Assert.IsTrue( values.Count == 1 );
                Assert.AreEqual( "vdf", values[ "format" ] );

                handlerCalled = true;
                return Task.CompletedTask;
            };

            var args = default( Dictionary<string, object> );
            var response = await iface.CallAsync( HttpMethod.Get, "PerformFooOperation", 2, args );
            Assert.IsTrue( handlerCalled );
        }

        [TestMethod]
        public async Task UsesSingleParameterArgumentsDictionary()
        {
            var hookableHandler = new HookableHandler();
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
                Assert.IsNotNull( request );
                Assert.AreEqual( "/IFooService/PerformFooOperation/v2", request.RequestUri.AbsolutePath );
                Assert.AreEqual( HttpMethod.Put, request.Method );

                var formData = await request.Content.ReadAsFormDataAsync();
                Assert.AreEqual( 3, formData.Count );
                Assert.AreEqual( "foo", formData[ "f" ] );
                Assert.AreEqual( "bar", formData[ "b" ] );
                Assert.AreEqual( "vdf", formData[ "format" ] );

                handlerCalled = true;
            };

            var response = await iface.PerformFooOperation2( args );
            Assert.IsTrue( handlerCalled );
        }

        [TestMethod]
        public async Task IncludesApiKeyInParams()
        {
            var hookableHandler = new HookableHandler();
            var configuration = SteamConfiguration.Create( c => c
                .WithHttpClientFactory( () => new HttpClient( hookableHandler ) )
                .WithWebAPIKey("MySecretApiKey") );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
            };

            var handlerCalled = false;
            hookableHandler.OnRequest = request =>
            {
                Assert.IsNotNull( request );
                Assert.AreEqual( HttpMethod.Get, request.Method );
                Assert.AreEqual( "/IFooService/PerformFooOperation/v2/", request.RequestUri.AbsolutePath );

                var values = request.RequestUri.ParseQueryString();
                Assert.AreEqual( 4, values.Count );
                Assert.AreEqual( "MySecretApiKey", values[ "key" ] );
                Assert.AreEqual( "foo", values[ "f" ] );
                Assert.AreEqual( "bar", values[ "b" ] );
                Assert.AreEqual( "vdf", values[ "format" ] );

                handlerCalled = true;
                return Task.CompletedTask;
            };

            var response = await iface.PerformFooOperation2( args );
            Assert.IsTrue( handlerCalled );
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
                    Content = new ByteArrayContent( Array.Empty<byte>() )
                };
            }
        }
    }
}
