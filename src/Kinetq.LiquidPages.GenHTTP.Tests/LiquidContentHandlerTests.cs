using System.Net;
using System.Text;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Internal;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Moq;
using Xunit;

namespace Kinetq.LiquidPages.GenHTTP.Tests
{
    [Collection("Sequential")]
    public class LiquidContentHandlerTests : IAsyncLifetime
    {
        private Mock<ILiquidResponseMiddleware> _mockLiquidResponseMiddleware;
        private IServer _server;
        private string _urlPrefix;
        private int _port;

        public async Task InitializeAsync()
        {
            _mockLiquidResponseMiddleware = new Mock<ILiquidResponseMiddleware>();

            _port = HttpHelpers.GetRandomUnusedPort();
            _urlPrefix = $"http://localhost:{_port}";

            _server = Host.Create()
                .Handler(new LiquidContentHandler(_mockLiquidResponseMiddleware.Object))
                .Bind(IPAddress.Loopback, (ushort)_port)
                .Build();

            await _server.StartAsync();

            // Wait until the server is actually listening before returning
            for (var i = 0; i < 50; i++)
            {
                if (_server.Running)
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
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseModel>()))
                .Callback<LiquidRequestModel, LiquidResponseModel>((req, response) =>
                {
                    capturedRequest = req;
                    response.SetStatusCode(200);
                    response.SetContentType("text/html");
                    response.BodyWriter.Write("<h1>Page Found</h1>");
                })
                .Returns(Task.CompletedTask);

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
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseModel>()))
                .Callback<LiquidRequestModel, LiquidResponseModel>((req, response) =>
                {
                    capturedRequest = req;
                    response.SetStatusCode(200);
                    response.SetContentType("text/html");
                    response.BodyWriter.Write("<h1>Page Found</h1>");
                })
                .Returns(Task.CompletedTask);

            using var httpClient = new HttpClient();

            await httpClient.PostAsync(_urlPrefix, new StringContent("{\"test\": 0}"));

            _mockLiquidResponseMiddleware.Verify(
                m => m.HandleRequestAsync(It.Is<LiquidRequestModel>(r => r.Body != null), It.IsAny<LiquidResponseModel>()),
                Times.Once);

            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.Body);
        }

        [Fact]
        public async Task Server_ShouldReturn500_WhenHandleRequestAsyncThrows()
        {
            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseModel>()))
                .ThrowsAsync(new Exception("Simulated failure"));

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(_urlPrefix);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Server_ShouldCallHandleRequestAsync_WhenPathIsRequested()
        {
            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseModel>()))
                .Callback<LiquidRequestModel, LiquidResponseModel>((_, response) =>
                {
                    response.SetStatusCode(200);
                    response.SetContentType("text/html");
                    response.BodyWriter.Write("<h1>Page Found</h1>");
                })
                .Returns(Task.CompletedTask);

            using var httpClient = new HttpClient();

            await httpClient.GetAsync($"{_urlPrefix}/some-page");

            _mockLiquidResponseMiddleware.Verify(
                m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseModel>()),
                Times.Once);
        }

        [Fact]
        public async Task Server_ShouldPassQueryParams_OnLiquidRequestModel()
        {
            LiquidRequestModel? capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseModel>()))
                .Callback<LiquidRequestModel, LiquidResponseModel>((req, response) =>
                {
                    capturedRequest = req;
                    response.SetStatusCode(200);
                    response.SetContentType("text/html");
                    response.BodyWriter.Write("<h1>Page Found</h1>");
                })
                .Returns(Task.CompletedTask);

            using var httpClient = new HttpClient();

            await httpClient.GetAsync($"{_urlPrefix}/page?foo=bar");

            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.QueryParams.ContainsKey("foo"));
            Assert.Equal("bar", capturedRequest.QueryParams["foo"]);
        }

        [Fact]
        public async Task Server_ShouldPassRoute_OnLiquidRequestModel()
        {
            LiquidRequestModel? capturedRequest = null;

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseModel>()))
                .Callback<LiquidRequestModel, LiquidResponseModel>((req, response) =>
                {
                    capturedRequest = req;
                    response.SetStatusCode(200);
                    response.SetContentType("text/html");
                    response.BodyWriter.Write("<h1>Page Found</h1>");
                })
                .Returns(Task.CompletedTask);

            using var httpClient = new HttpClient();

            await httpClient.GetAsync($"{_urlPrefix}/my-route");

            Assert.NotNull(capturedRequest);
            Assert.Contains("/my-route", capturedRequest.Route);
        }

        public async Task DisposeAsync()
        {
            await _server.DisposeAsync();
        }
    }
}
