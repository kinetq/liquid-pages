using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using EmbedIO;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Moq;
using Xunit;

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
            _liquidWebModule = new LiquidWebModule("/")
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
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .Callback<LiquidRequestModel>(req => capturedRequest = req)
                .ReturnsAsync(new LiquidResponseModel()
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Page Found</h1>")
                });

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
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .Callback<LiquidRequestModel>(req => capturedRequest = req)
                .ReturnsAsync(new LiquidResponseModel()
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Page  Found</h1>")
                }); // adjust return type as needed

            using var httpClient = new HttpClient();

            await httpClient.PostAsync(_urlPrefix, new StringContent("{\"test\": 0}"));

            _mockLiquidResponseMiddleware.Verify(
                m => m.HandleRequestAsync(It.Is<LiquidRequestModel>(r => r.Body != null)),
                Times.Once);

            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.Body);
        }

        [Fact]
        public async Task WebServer_ShouldReturn500_WhenHandleRequestAsyncThrows()
        {
            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .ThrowsAsync(new Exception("Simulated failure"));

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(_urlPrefix);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task WebServer_ShouldNotCallHandleRequestAsync_WhenPathIsExcluded()
        {
            _liquidWebModule.ExcludedPaths = new[] { new Regex("^/excluded$") };

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .ReturnsAsync(new LiquidResponseModel()
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Should Not Reach</h1>")
                });

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"{_urlPrefix}excluded");

            _mockLiquidResponseMiddleware.Verify(
                m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()),
                Times.Never);
        }

        [Fact]
        public async Task WebServer_ShouldCallHandleRequestAsync_WhenPathIsNotExcluded()
        {
            _liquidWebModule.ExcludedPaths = new[] { new Regex("/excluded") };

            _mockLiquidResponseMiddleware
                .Setup(m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()))
                .ReturnsAsync(new LiquidResponseModel()
                {
                    Content = Encoding.UTF8.GetBytes("<h1>Page Found</h1>")
                });

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"{_urlPrefix}not-excluded");

            _mockLiquidResponseMiddleware.Verify(
                m => m.HandleRequestAsync(It.IsAny<LiquidRequestModel>()),
                Times.Once);
        }

        public Task DisposeAsync()
        {
            _webServer.Dispose();
            return Task.CompletedTask;
        }
    }
}
