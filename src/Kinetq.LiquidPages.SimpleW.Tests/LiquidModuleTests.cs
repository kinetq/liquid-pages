using System.Net;
using System.Text;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Managers;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleW;
using Xunit;

namespace Kinetq.LiquidPages.SimpleW.Tests
{
    [Collection("Sequential")]
    public class LiquidModuleTests : IAsyncLifetime
    {
        private Mock<ILiquidResponseMiddleware> _mockLiquidResponseMiddleware = null!;
        private SimpleWServer _server = null!;
        private string _urlPrefix = string.Empty;

        public async Task InitializeAsync()
        {
            _mockLiquidResponseMiddleware = new Mock<ILiquidResponseMiddleware>();

            var routesManager = new LiquidRoutesManager(new NullLogger<LiquidRoutesManager>());
            routesManager.RegisterRoute(new LiquidRoute
            {
                RouteTemplate = "/",
                LiquidTemplatePath = "home.liquid",
                Execute = _ => Task.FromResult<object>(new { })
            });
            routesManager.RegisterRoute(new LiquidRoute
            {
                RouteTemplate = "/users/:id",
                LiquidTemplatePath = "users.liquid",
                Execute = model => Task.FromResult<object>(new { Id = model.RouteValues?["id"] })
            });

            var port = HttpHelpers.GetRandomUnusedPort();
            _urlPrefix = $"http://localhost:{port}";

            _server = new SimpleWServer(IPAddress.Loopback, port);
            _server.UseModule(new LiquidPagesModule(routesManager, _mockLiquidResponseMiddleware.Object)
            {
                MapFallback404 = true
            });

            await _server.StartAsync(CancellationToken.None);

            for (var i = 0; i < 50; i++)
            {
                if (_server.IsStarted)
                {
                    return;
                }

                await Task.Delay(100);
            }

            throw new TimeoutException("Web server did not start in time.");
        }

        [Fact]
        public void Server_ShouldNotBeNull()
        {
            Assert.NotNull(_server);
        }

        [Fact]
        public async Task Server_ShouldRespondToRequests()
        {
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(_urlPrefix);

            Assert.NotNull(response);
        }

        [Fact]
        public async Task Server_ShouldSetHttpMethod_OnLiquidRequestModel()
        {
            LiquidRequestModel? capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .Callback<LiquidRequestModel>(req => capturedRequest = req)
                .ReturnsAsync(new LiquidResponseModel
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Page Found</h1>"),
                    ContentType = "text/html",
                    StatusCode = 200
                });

            using var httpClient = new HttpClient();

            await httpClient.PostAsync(_urlPrefix, new StringContent("{\"test\": 0}"));

            Assert.NotNull(capturedRequest);
            Assert.Equal("POST", capturedRequest.Method, ignoreCase: true);
        }

        [Fact]
        public async Task Server_ShouldCallHandleRequestAsync_WithNonNullBody()
        {
            LiquidRequestModel? capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .Callback<LiquidRequestModel>(req => capturedRequest = req)
                .ReturnsAsync(new LiquidResponseModel
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Page Found</h1>"),
                    ContentType = "text/html",
                    StatusCode = 200
                });

            using var httpClient = new HttpClient();

            await httpClient.PostAsync(_urlPrefix, new StringContent("{\"test\": 0}"));

            _mockLiquidResponseMiddleware.Verify(
                m => m.HandleRequestAsync(It.Is<LiquidRequestModel>(r => r.Body != null)),
                Times.Once);

            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.Body);
        }

        [Fact]
        public async Task Server_ShouldReturn500_WhenHandleRequestAsyncThrows()
        {
            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .ThrowsAsync(new Exception("Simulated failure"));

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(_urlPrefix);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Server_ShouldPassQueryParams_OnLiquidRequestModel()
        {
            LiquidRequestModel? capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .Callback<LiquidRequestModel>(req => capturedRequest = req)
                .ReturnsAsync(new LiquidResponseModel
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Page Found</h1>"),
                    ContentType = "text/html",
                    StatusCode = 200
                });

            using var httpClient = new HttpClient();

            await httpClient.GetAsync($"{_urlPrefix}/?foo=bar");

            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.QueryParams.ContainsKey("foo"));
            Assert.Equal("bar", capturedRequest.QueryParams["foo"]);
        }

        [Fact]
        public async Task Server_ShouldPassRoute_OnLiquidRequestModel()
        {
            LiquidRequestModel? capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .Callback<LiquidRequestModel>(req => capturedRequest = req)
                .ReturnsAsync(new LiquidResponseModel
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Page Found</h1>"),
                    ContentType = "text/html",
                    StatusCode = 200
                });

            using var httpClient = new HttpClient();

            await httpClient.GetAsync($"{_urlPrefix}/my-route");

            Assert.NotNull(capturedRequest);
            Assert.Contains("/my-route", capturedRequest.Route);
        }

        [Fact]
        public async Task Server_ShouldPopulateRouteValues_WhenRouteMatches()
        {
            LiquidRequestModel? capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .Callback<LiquidRequestModel>(req => capturedRequest = req)
                .ReturnsAsync(new LiquidResponseModel
                {
                    Content = Encoding.UTF8.GetBytes("ok"),
                    ContentType = "text/plain",
                    StatusCode = 200
                });

            using var httpClient = new HttpClient();

            await httpClient.GetAsync($"{_urlPrefix}/users/42");

            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.LiquidRoute);
            Assert.Equal("/users/:id", capturedRequest.LiquidRoute.RouteTemplate);
            Assert.Contains(capturedRequest.RouteValues, kvp => kvp.Value?.ToString() == "42");
        }

        public async Task DisposeAsync()
        {
            await _server.StopAsync();
        }
    }
}
