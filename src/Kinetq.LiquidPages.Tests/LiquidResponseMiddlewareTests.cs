using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kinetq.LiquidPages.Tests
{
    public class LiquidResponseMiddlewareTests : IAsyncLifetime
    {
        private ILiquidResponseMiddleware _liquidResponseMiddleware;
        private Mock<ILiquidRoutesManager> _liquidRoutesManagerMock;
        private Mock<IHtmlRenderer> _htmlRendererMock;
        private IFileProvider _embeddedFileProvider;

        public async Task InitializeAsync()
        {
            _liquidRoutesManagerMock = new Mock<ILiquidRoutesManager>();
            _htmlRendererMock = new Mock<IHtmlRenderer>();

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection
                .AddSingleton(_liquidRoutesManagerMock.Object)
                .AddSingleton(_htmlRendererMock.Object)
                .AddScoped<ILiquidResponseMiddleware, LiquidResponseMiddleware>()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            _liquidResponseMiddleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>();
            _embeddedFileProvider = new EmbeddedFileProvider(typeof(LiquidResponseMiddlewareTests).Assembly, "Kinetq.LiquidPages.Tests.Templates");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetHomePageAsync_ShouldReturnRenderedHtml_WhenRouteExists()
        {
            // Arrange
            const string expectedRoute = "/";
            const string expectedRenderedHtml = "<html><body>Welcome to Home Page</body></html>";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForPath(expectedRoute, It.IsAny<IDictionary<string, string>>()))
                .Returns(new LiquidRoute
                {
                    RoutePattern = new Regex("^/$"),
                    LiquidTemplatePath = "index.liquid",
                    FileProvider = null // Not needed for this test
                });

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync(expectedRenderedHtml);

            // Act
            var responseModel = await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = expectedRoute,
                QueryParams = new Dictionary<string, string>()
            });

            var actualHtml = System.Text.Encoding.UTF8.GetString(responseModel.Content);

            // Assert
            Assert.Equal(expectedRenderedHtml, actualHtml);
            Assert.True(responseModel.StatusCode == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetNotFoundAsync_ShouldReturnRenderedHtml_WhenRouteExists()
        {
            // Arrange
            const string expectedRenderedHtml = "<html><body>Not Found</body></html>";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForStatusCode(HttpStatusCode.NotFound))
                .Returns(new LiquidRoute
                {
                    RoutePattern = new Regex("^/$"),
                    LiquidTemplatePath = "404.liquid",
                    FileProvider = null
                });

            _htmlRendererMock
                .SetupSequence(x => 
                    x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync((string)null) // First call returns null to simulate not found
                .ReturnsAsync(expectedRenderedHtml);

            // Act
            var responseModel = await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = "/",
                QueryParams = new Dictionary<string, string>()
            });

            var actualHtml = System.Text.Encoding.UTF8.GetString(responseModel.Content);

            // Assert
            Assert.Equal(expectedRenderedHtml, actualHtml);
            Assert.False(responseModel.StatusCode == (int)HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("/styles/styles.css")]
        [InlineData("/scripts/site.js")]
        [InlineData("/assets/data.json")]
        [InlineData("/assets/image.svg")]
        [InlineData("/assets/image.png")]
        [InlineData("/assets/image.jpeg")]
        [InlineData("/scripts/site.js.map")]
        public async Task GetHomePageAsync_ShouldReturnAssetFile_WhenRouteExists(string assetPath)
        {
            // Arrange
            const string expectedRoute = "/";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForPath(expectedRoute, It.IsAny<IDictionary<string, string>>()))
                .Returns(new LiquidRoute
                {
                    RoutePattern = new Regex("^/$"),
                    LiquidTemplatePath = "index.liquid",
                    FileProvider = _embeddedFileProvider // Not needed for this test
                });

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync((string)null);


            var responseModel = await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = $"{assetPath.Substring(1, assetPath.Length - 1)}",
                QueryParams = new Dictionary<string, string>(),
                Headers = new NameValueCollection()
                {
                    {"Referer", "http://localhost/"}
                }
            });

            // Load expected CSS content from embedded file
            var fileInfo = _embeddedFileProvider.GetFileInfo(assetPath);
            var fileBytes = await fileInfo.GetFileContentsBytes();

            // Assert
            Assert.True(responseModel.StatusCode == (int)HttpStatusCode.OK);
            Assert.Equal(fileBytes, responseModel.Content);
        }

        [Theory]
        [InlineData("/styles/styles.css")]
        [InlineData("/scripts/site.js")]
        [InlineData("/assets/data.json")]
        [InlineData("/assets/image.svg")]
        [InlineData("/assets/image.png")]
        [InlineData("/assets/image.jpeg")]
        [InlineData("/scripts/site.js.map")]
        public async Task GetHomePageAsync_ShouldReturnAssetFile_WhenReferrerSet_WhenRouteNotExists(string assetPath)
        {
            // Arrange
            const string expectedRoute = "/";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForPath(expectedRoute, It.IsAny<IDictionary<string, string>>()))
                .Returns((LiquidRoute)null);

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync((string)null);

            _liquidRoutesManagerMock
                .Setup(x => x.GetFileProviderForAsset(It.IsAny<string>()))
                .Returns(_embeddedFileProvider);


            var responseModel = await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = $"{assetPath.Substring(1, assetPath.Length - 1)}",
                QueryParams = new Dictionary<string, string>(),
                Headers = new NameValueCollection()
                {
                    {"Referer", "http://localhost/"}
                }
            });

            // Load expected CSS content from embedded file
            var fileInfo = _embeddedFileProvider.GetFileInfo(assetPath);
            var fileBytes = await fileInfo.GetFileContentsBytes();

            // Assert
            Assert.True(responseModel.StatusCode == (int)HttpStatusCode.OK);
            Assert.Equal(fileBytes, responseModel.Content);
        }

        [Theory]
        [InlineData("/styles/styles.css")]
        [InlineData("/scripts/site.js")]
        [InlineData("/assets/data.json")]
        [InlineData("/assets/image.svg")]
        [InlineData("/assets/image.png")]
        [InlineData("/assets/image.jpeg")]
        [InlineData("/scripts/site.js.map")]
        public async Task GetHomePageAsync_ShouldReturnAssetFile_WhenNoRouteExists(string assetPath)
        {
            _liquidRoutesManagerMock
                .Setup(x => x.GetFileProviderForAsset(assetPath.TrimStart('/')))
                .Returns(_embeddedFileProvider);

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>()))
                .ReturnsAsync((string)null);

            var responseModel = await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = $"{assetPath.Substring(1, assetPath.Length - 1)}",
                QueryParams = new Dictionary<string, string>(),
                Headers = new NameValueCollection()
            });

            // Load expected CSS content from embedded file
            var fileInfo = _embeddedFileProvider.GetFileInfo(assetPath);
            var fileBytes = await fileInfo.GetFileContentsBytes();

            // Assert
            Assert.True(responseModel.StatusCode == (int)HttpStatusCode.OK);
            Assert.Equal(fileBytes, responseModel.Content);
        }
    }
}
