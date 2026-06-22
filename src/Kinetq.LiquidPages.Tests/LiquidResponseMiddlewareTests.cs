using System.Net;
using System.Text;
using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kinetq.LiquidPages.Tests
{
    public class LiquidResponseMiddlewareTests : IAsyncLifetime
    {
        private ILiquidResponseMiddleware _liquidResponseMiddleware;
        private Mock<ILiquidRoutesManager> _liquidRoutesManagerMock;
        private Mock<IHtmlRenderer> _htmlRendererMock;

        public async Task InitializeAsync()
        {
            _liquidRoutesManagerMock = new Mock<ILiquidRoutesManager>();
            _htmlRendererMock = new Mock<IHtmlRenderer>();
            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>(), It.IsAny<StreamWriter>()))
                .Returns(Task.CompletedTask);

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection
                .AddSingleton(_liquidRoutesManagerMock.Object)
                .AddSingleton(_htmlRendererMock.Object)
                .AddScoped<ILiquidResponseMiddleware, LiquidResponseMiddleware>()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            _liquidResponseMiddleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>();
            await Task.CompletedTask;
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
            var liquidRoute = new LiquidRoute
            {
                RouteTemplate = "/",
                LiquidTemplatePath = "index.liquid"
            };

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>(), It.IsAny<StreamWriter>()))
                .Returns((RenderModel _, LiquidRoute _, StreamWriter writer) => writer.WriteAsync(expectedRenderedHtml));

            var (responseModel, responseStream, statusCodeAccessor, _) = CreateResponseModel();

            // Act
            await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = expectedRoute,
                LiquidRoute = liquidRoute,
                QueryParams = new Dictionary<string, string>()
            }, responseModel);

            var actualHtml = Encoding.UTF8.GetString(responseStream.ToArray());

            // Assert
            Assert.Equal(expectedRenderedHtml, actualHtml);
            Assert.True(statusCodeAccessor() == (int)HttpStatusCode.OK);
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
                    RouteTemplate = "/",
                    LiquidTemplatePath = "404.liquid"
                });

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>(), It.IsAny<StreamWriter>()))
                .Returns((RenderModel _, LiquidRoute _, StreamWriter writer) => writer.WriteAsync(expectedRenderedHtml));

            var (responseModel, responseStream, statusCodeAccessor, _) = CreateResponseModel();

            // Act
            await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = "/",
                QueryParams = new Dictionary<string, string>()
            }, responseModel);

            var actualHtml = Encoding.UTF8.GetString(responseStream.ToArray());

            // Assert
            Assert.Equal(expectedRenderedHtml, actualHtml);
            Assert.False(statusCodeAccessor() == (int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task HandleRequestAsync_ShouldReturnInternalServerError_WhenGeneralExceptionThrown_Doesnt_Exceed_CallStackLimit()
        {
            // Arrange
            const string expectedRoute = "/";
            var expectedException = new InvalidOperationException("Test exception");

            var liquidRoute = new LiquidRoute
            {
                RouteTemplate = "/",
                LiquidTemplatePath = "index.liquid",
                Execute = _ => throw expectedException
            };

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForStatusCode(HttpStatusCode.InternalServerError))
                .Returns(new LiquidRoute
                {
                    RouteTemplate = "/",
                    LiquidTemplatePath = "503.liquid",
                    Execute = _ => throw expectedException
                });
            var (responseModel, _, statusCodeAccessor, _) = CreateResponseModel();

            // Act
            await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = expectedRoute,
                LiquidRoute = liquidRoute,
                QueryParams = new Dictionary<string, string>()
            }, responseModel);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeAccessor());
        }

        [Fact]
        public async Task HandleRequestAsync_ShouldReturnInternalServerError_WhenGeneralExceptionThrown()
        {
            // Arrange
            const string expectedRoute = "/";
            var expectedException = new InvalidOperationException("Test exception");

            var liquidRoute = new LiquidRoute
            {
                RouteTemplate = "/",
                LiquidTemplatePath = "index.liquid",
                Execute = _ => throw expectedException
            };

            const string expectedRenderedHtml = "<html><body>Unhandled Exception</body></html>";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForStatusCode(HttpStatusCode.InternalServerError))
                .Returns(new LiquidRoute
                {
                    RouteTemplate = "/",
                    LiquidTemplatePath = "500.liquid"
                });

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>(), It.IsAny<StreamWriter>()))
                .Returns((RenderModel _, LiquidRoute _, StreamWriter writer) => writer.WriteAsync(expectedRenderedHtml));

            var (responseModel, _, statusCodeAccessor, _) = CreateResponseModel();

            // Act
            await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = expectedRoute,
                LiquidRoute = liquidRoute,
                QueryParams = new Dictionary<string, string>()
            }, responseModel);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeAccessor());
        }

        [Fact]
        public async Task HandleRequestAsync_ShouldReturnBadGateway_WhenHttpRequestExceptionThrown()
        {
            // Arrange
            const string expectedRoute = "/";
            var expectedException = new HttpRequestException(
                "Service Unavailable", 
                new Exception(), 
                HttpStatusCode.ServiceUnavailable);

            var liquidRoute = new LiquidRoute
            {
                RouteTemplate = "/",
                LiquidTemplatePath = "index.liquid",
                Execute = _ => throw expectedException
            };

            const string expectedRenderedHtml = "<html><body>Service Unavailable</body></html>";

            _liquidRoutesManagerMock
                .Setup(x => x.GetRouteForStatusCode(HttpStatusCode.ServiceUnavailable))
                .Returns(new LiquidRoute
                {
                    RouteTemplate = "/",
                    LiquidTemplatePath = "503.liquid"
                });

            _htmlRendererMock
                .Setup(x => x.RenderHtml(It.IsAny<RenderModel>(), It.IsAny<LiquidRoute>(), It.IsAny<StreamWriter>()))
                .Returns((RenderModel _, LiquidRoute _, StreamWriter writer) => writer.WriteAsync(expectedRenderedHtml));

            var (responseModel, _, statusCodeAccessor, _) = CreateResponseModel();

            // Act
            await _liquidResponseMiddleware.HandleRequestAsync(new LiquidRequestModel()
            {
                Route = expectedRoute,
                LiquidRoute = liquidRoute,
                QueryParams = new Dictionary<string, string>()
            }, responseModel);

            // Assert
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, statusCodeAccessor());
        }

        private static (TestLiquidResponseBuilder ResponseModel, MemoryStream ResponseStream, Func<int> StatusCodeAccessor, Func<string> ContentTypeAccessor) CreateResponseModel()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
            writer.AutoFlush = true;
            var statusCode = 0;
            var contentType = string.Empty;

            var responseModel = new TestLiquidResponseBuilder();

            return (responseModel, stream, () => statusCode, () => contentType);
        }
    }

    class TestLiquidResponseBuilder : LiquidResponseBuilder<dynamic>
    {
        public override void Initialize(dynamic response, TextWriter bodyWriter)
        {
            Response = response;
            BodyWriter = bodyWriter;
        }

        public override void SetStatusCode(int statusCode, string? message = null)
        {
            Response.StatusCode = statusCode;
        }

        public override void SetContentType(string contentType)
        {
            Response.ContentType = contentType;
        }
    }
}
