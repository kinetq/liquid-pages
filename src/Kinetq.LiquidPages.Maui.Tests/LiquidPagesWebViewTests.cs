using FluentAssertions;
using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Maui.Interfaces;
using Kinetq.LiquidPages.Maui.Models;
using Kinetq.LiquidPages.Models;
using Moq;
using Xunit;

namespace Kinetq.LiquidPages.Maui.Tests;

public class LiquidPagesWebViewTests
{
    [Fact]
    public async Task NavigateToPathAsync_ShouldNormalizePath_RenderHtml_AndRaiseResponseRendered()
    {
        var route = CreateRoute("/products/{id}");
        var routeTreeMock = new Mock<IRouteTree>();
        var middlewareMock = new Mock<ILiquidResponseMiddleware>();

        routeTreeMock
            .Setup(x => x.Match("/products/42"))
            .Returns(new RouteMatch
            {
                LiquidRoute = route,
                RouteValues = new LiquidRouteValuesDictionary(new Dictionary<string, object?> { ["id"] = "42" })
            });

        LiquidRequestModel? capturedRequest = null;
        middlewareMock
            .Setup(x => x.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseBuilder>()))
            .Returns<LiquidRequestModel, LiquidResponseBuilder>(async (request, response) =>
            {
                capturedRequest = request;
                response.SetStatusCode(201);
                response.SetContentType("text/plain");
                await response.BodyWriter.WriteAsync("Rendered body");
            });

        var webView = new LiquidPagesWebView
        {
            RouteTree = routeTreeMock.Object,
            LiquidResponseMiddleware = middlewareMock.Object
        };

        LiquidPagesResponseEventArgs? renderedArgs = null;
        webView.ResponseRendered += (_, args) => renderedArgs = args;

        await webView.NavigateToPathAsync("products/42");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Route.Should().Be("/products/42");
        capturedRequest.Method.Should().Be("GET");
        capturedRequest.LiquidRoute.Should().Be(route);
        capturedRequest.RouteValues.TryGetValue("id", out var id).Should().BeTrue();
        id.Should().Be("42");

        var htmlSource = webView.Source as HtmlWebViewSource;
        htmlSource.Should().NotBeNull();
        htmlSource!.Html.Should().Be("Rendered body");

        renderedArgs.Should().NotBeNull();
        renderedArgs!.Path.Should().Be("/products/42");
        renderedArgs.StatusCode.Should().Be(201);
        renderedArgs.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task NavigateToPathAsync_ShouldUseRootRoute_WhenPathIsNull()
    {
        var routeTreeMock = new Mock<IRouteTree>();
        var middlewareMock = new Mock<ILiquidResponseMiddleware>();

        routeTreeMock
            .Setup(x => x.Match("/"))
            .Returns((RouteMatch?)null);

        LiquidRequestModel? capturedRequest = null;
        middlewareMock
            .Setup(x => x.HandleRequestAsync(It.IsAny<LiquidRequestModel>(), It.IsAny<LiquidResponseBuilder>()))
            .Returns<LiquidRequestModel, LiquidResponseBuilder>(async (request, response) =>
            {
                capturedRequest = request;
                response.SetStatusCode(200);
                response.SetContentType("text/html");
                await response.BodyWriter.WriteAsync("<h1>Home</h1>");
            });

        var webView = new LiquidPagesWebView
        {
            RouteTree = routeTreeMock.Object,
            LiquidResponseMiddleware = middlewareMock.Object
        };

        await webView.NavigateToPathAsync(null);

        routeTreeMock.Verify(x => x.Match("/"), Times.Once);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Route.Should().Be("/");
        capturedRequest.LiquidRoute.Should().BeNull();
        capturedRequest.RouteValues.Should().BeSameAs(EmptyRouteValuesDictionary.Instance);

        var htmlSource = webView.Source as HtmlWebViewSource;
        htmlSource.Should().NotBeNull();
        htmlSource!.Html.Should().Be("<h1>Home</h1>");
    }

    private static LiquidRoute CreateRoute(string routeTemplate)
    {
        return new LiquidRoute
        {
            RouteTemplate = routeTemplate,
            LiquidTemplatePath = "test.liquid",
            Execute = _ => Task.FromResult<object>(new { })
        };
    }
}
