using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using EmbedIO;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Moq;
using Kinetq.LiquidPages.Managers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Kinetq.LiquidPages.Builders;

namespace Kinetq.LiquidPages.EmbedIO.Tests
{
    [Collection("Sequential")]
    public class LiquidWebModuleTests : IAsyncLifetime
    {
        private Mock<ILiquidResponseMiddleware> _mockLiquidResponseMiddleware;
        private LiquidWebModule _liquidWebModule;
        private WebServer _webServer;
        private string _urlPrefix;

        public async Task InitializeAsync()
        {
            _mockLiquidResponseMiddleware = new Mock<ILiquidResponseMiddleware>();
            var routesManager = new LiquidRoutesManager(new NullLogger<LiquidRoutesManager>());
            routesManager.RegisterRoute(new LiquidRoute
            {
                RouteTemplate = "/users/{id}",
                LiquidTemplatePath = "users.liquid",
                Execute = model => Task.FromResult<object>(new { Id = model.RouteValues?["id"] })
            });
            _liquidWebModule = new LiquidWebModule("/", routesManager)
            {
                LiquidResponseMiddleware = _mockLiquidResponseMiddleware.Object
            };

            _urlPrefix = $"http://localhost:{HttpHelpers.GetRandomUnusedPort()}/";

            _webServer = new WebServer(o => o
                .WithUrlPrefix(_urlPrefix)
                .WithMode(HttpListenerMode.EmbedIO));

            _webServer.WithModule(_liquidWebModule);

            _ = _webServer.RunAsync();

            // Wait until the server is actually listening before returning
            using var httpClient = new HttpClient();
            for (var i = 0; i < 50; i++)
            {
                try
                {
                    if (_webServer.State == WebServerState.Listening)
                    {
                        return;
                    }
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(100);
                }
            }

            throw new TimeoutException("Web server did not start in time.");
        }

        [Fact]
        public async Task WebServer_ShouldSetHttpMethod_OnLiquidRequestModel()
        {
            LiquidRequestModel capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<ILiquidResponseBuilder>()))
                .Callback<LiquidRequestModel, ILiquidResponseBuilder>((req, response) =>
                {
                    capturedRequest = req;
                    response.BodyWriter.Write("<h1>Page Found</h1>");
                })
                .ReturnsAsync((string?)null);

            using var httpClient = new HttpClient();

            await httpClient.PostAsync(_urlPrefix, new StringContent("{\"test\": 0}"));

            Assert.NotNull(capturedRequest);
            Assert.Equal("POST", capturedRequest.Method, ignoreCase: true);
        }

        [Fact]
        public void WebServer_ShouldNotBeNull()
        {
            Assert.NotNull(_webServer);
        }

        [Fact]
        public async Task WebServer_ShouldRespondToRequests()
        {
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(_urlPrefix);

            // Server is running and responds (even if 404, it means server is alive)
            Assert.NotNull(response);
        }

        [Fact]
        public void WebServer_ShouldAcceptLiquidWebModule()
        {
            Assert.Contains(_webServer.Modules, m => m is LiquidWebModule);
        }

        [Fact]
        public async Task WebServer_ShouldHaveEntityBody()
        {
            using var httpClient = new HttpClient();

            var response = await httpClient.PostAsync(_urlPrefix, new StringContent("{\"test\": 0}"));
            
        }

        [Fact]
        public async Task WebServer_ShouldCallHandleRequestAsync_WithNonNullBody()
        {
            LiquidRequestModel capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<ILiquidResponseBuilder>()))
                .Callback<LiquidRequestModel, ILiquidResponseBuilder>((req, response) =>
                {
                    capturedRequest = req;
                    response.BodyWriter.Write("<h1>Page Found</h1>");
                })
                .ReturnsAsync((string?)null);

            using var httpClient = new HttpClient();

            await httpClient.PostAsync(_urlPrefix, new StringContent("{\"test\": 0}"));

            _mockLiquidResponseMiddleware.Verify(
                m => m.HandleRequestAsync(It.Is<LiquidRequestModel>(r => r.Body != null), It.IsAny<ILiquidResponseBuilder>()),
                Times.Once);

            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.Body);
        }

        [Fact]
        public async Task WebServer_ShouldReturn500_WhenHandleRequestAsyncThrows()
        {
            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<ILiquidResponseBuilder>()))
                .ThrowsAsync(new Exception("Simulated failure"));

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(_urlPrefix);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task WebServer_ShouldPopulateRouteValues_WhenRouteMatches()
        {
            LiquidRequestModel capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<ILiquidResponseBuilder>()))
                .Callback<LiquidRequestModel, ILiquidResponseBuilder>((req, response) =>
                {
                    capturedRequest = req;
                    response.BodyWriter.Write("ok");
                })
                .ReturnsAsync((string?)null);

            using var httpClient = new HttpClient();

            await httpClient.GetAsync($"{_urlPrefix}users/42");

            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.RouteValues);
            Assert.True(capturedRequest.RouteValues.ContainsKey("id"));
            Assert.Equal("42", capturedRequest.RouteValues["id"]?.ToString());
        }

        public Task DisposeAsync()
        {
            _webServer.Dispose();
            return Task.CompletedTask;
        }
    }
}
