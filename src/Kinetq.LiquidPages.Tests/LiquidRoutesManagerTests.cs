using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Managers;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kinetq.LiquidPages.Tests;

public class LiquidRoutesManagerTests : IAsyncLifetime
{
    private ILiquidRoutesManager _liquidRoutesManager;
    private IFileProvider _embeddedFileProvider;
    private ServiceProvider _serviceProvider;
    private ILogger<LiquidRoutesManager> _logger;

    public Task InitializeAsync()
    {
        var serviceCollection = new ServiceCollection();
        _serviceProvider = serviceCollection
            .AddScoped<ILiquidRoutesManager, LiquidRoutesManager>()
            .AddScoped<ILiquidResponseMiddleware, LiquidResponseMiddleware>()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();

        _liquidRoutesManager = _serviceProvider.GetRequiredService<ILiquidRoutesManager>();
        _logger = _serviceProvider.GetRequiredService<ILogger<LiquidRoutesManager>>();
        _embeddedFileProvider = new EmbeddedFileProvider(typeof(LiquidResponseMiddlewareTests).Assembly, "Kinetq.LiquidMiddleware.Tests.Templates");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        return Task.CompletedTask;
    }

    private LiquidRoute CreateTestRoute(string pattern, string templatePath = "test.liquid", IFileProvider? fileProvider = null)
    {
        return new LiquidRoute
        {
            RoutePattern = new Regex(pattern),
            LiquidTemplatePath = templatePath,
            FileProvider = fileProvider ?? _embeddedFileProvider,
            Execute = async (model) => await Task.FromResult(new { Message = "Test" }),
            QueryParams = new Dictionary<string, string>()
        };
    }

    [Fact]
    public void RegisterRoute_ShouldAddRoute_WhenValidRouteProvided()
    {
        // Arrange
        var route = CreateTestRoute("^/test$");

        // Act
        _liquidRoutesManager.RegisterRoute(route);

        // Assert
        _liquidRoutesManager.LiquidRoutes.Should().Contain(route);
        _liquidRoutesManager.LiquidRoutes.Should().HaveCount(1);
    }

    //[Fact]
    //public void RegisterRoute_ShouldNotAddDuplicateRoute_WhenSamePatternExists()
    //{
    //    // Arrange
    //    var route1 = CreateTestRoute("^/test$", "template1.liquid");
    //    var route2 = CreateTestRoute("^/test$", "template2.liquid");

    //    // Act
    //    _liquidRoutesManager.RegisterRoute(route1);
    //    _liquidRoutesManager.RegisterRoute(route2);

    //    // Assert
    //    _liquidRoutesManager.LiquidRoutes.Should().HaveCount(1);
    //    _liquidRoutesManager.LiquidRoutes.Should().Contain(route1);
    //    _liquidRoutesManager.LiquidRoutes.Should().NotContain(route2);
    //}

    [Fact]
    public void RegisterRoute_ShouldAddMultipleRoutes_WhenDifferentPatternsProvided()
    {
        // Arrange
        var route1 = CreateTestRoute("^/test1$");
        var route2 = CreateTestRoute("^/test2$");
        var route3 = CreateTestRoute("^/api/users/(?<id>\\d+)$");

        // Act
        _liquidRoutesManager.RegisterRoute(route1);
        _liquidRoutesManager.RegisterRoute(route2);
        _liquidRoutesManager.RegisterRoute(route3);

        // Assert
        _liquidRoutesManager.LiquidRoutes.Should().HaveCount(3);
        _liquidRoutesManager.LiquidRoutes.Should().Contain(route1);
        _liquidRoutesManager.LiquidRoutes.Should().Contain(route2);
        _liquidRoutesManager.LiquidRoutes.Should().Contain(route3);
    }

    [Fact]
    public void RegisterErrorRoute_ShouldAddErrorRoute_WhenValidStatusCodeProvided()
    {
        // Arrange
        var statusCode = 404;
        var errorRoute = CreateTestRoute(".*", "404.liquid");

        // Act
        _liquidRoutesManager.RegisterErrorRoute(statusCode, errorRoute);

        // Assert
        _liquidRoutesManager.ErrorRoutes.Should().ContainKey(statusCode);
        _liquidRoutesManager.ErrorRoutes[statusCode].Should().Be(errorRoute);
    }

    [Fact]
    public void RegisterErrorRoute_ShouldNotOverwriteExistingErrorRoute_WhenSameStatusCodeProvided()
    {
        // Arrange
        var statusCode = 500;
        var errorRoute1 = CreateTestRoute(".*", "error1.liquid");
        var errorRoute2 = CreateTestRoute(".*", "error2.liquid");

        // Act
        _liquidRoutesManager.RegisterErrorRoute(statusCode, errorRoute1);
        _liquidRoutesManager.RegisterErrorRoute(statusCode, errorRoute2);

        // Assert
        _liquidRoutesManager.ErrorRoutes.Should().ContainKey(statusCode);
        _liquidRoutesManager.ErrorRoutes[statusCode].Should().Be(errorRoute1);
        _liquidRoutesManager.ErrorRoutes[statusCode].Should().NotBe(errorRoute2);
    }

    [Fact]
    public void GetRouteForStatusCode_ShouldReturnRoute_WhenStatusCodeExists()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;
        var errorRoute = CreateTestRoute(".*", "404.liquid");
        _liquidRoutesManager.RegisterErrorRoute((int)statusCode, errorRoute);

        // Act
        var result = _liquidRoutesManager.GetRouteForStatusCode(statusCode);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(errorRoute);
    }

    [Fact]
    public void GetRouteForStatusCode_ShouldReturnNull_WhenStatusCodeDoesNotExist()
    {
        // Arrange
        var statusCode = HttpStatusCode.InternalServerError;

        // Act
        var result = _liquidRoutesManager.GetRouteForStatusCode(statusCode);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRouteForPath_ShouldReturnMatchingRoute_WhenExactPathMatches()
    {
        // Arrange
        var route = CreateTestRoute("^/test$");
        _liquidRoutesManager.RegisterRoute(route);

        // Act
        var result = _liquidRoutesManager.GetRouteForPath("/test");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(route);
    }

    [Fact]
    public void GetRouteForPath_ShouldReturnNull_WhenNoRouteMatches()
    {
        // Arrange
        var route = CreateTestRoute("^/test$");
        _liquidRoutesManager.RegisterRoute(route);

        // Act
        var result = _liquidRoutesManager.GetRouteForPath("/nomatch");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRouteForPath_ShouldReturnFirstMatchingRoute_WhenMultipleRoutesMatch()
    {
        // Arrange
        var route1 = CreateTestRoute("^/test.*", "template1.liquid");
        var route2 = CreateTestRoute("^/test$", "template2.liquid");
        _liquidRoutesManager.RegisterRoute(route1);
        _liquidRoutesManager.RegisterRoute(route2);

        // Act
        var result = _liquidRoutesManager.GetRouteForPath("/test");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(route1); // First registered route should match first
    }

    [Fact]
    public void GetRouteForPath_ShouldPopulateQueryParams_WhenParameterizedRouteMatches()
    {
        // Arrange
        var route = CreateTestRoute("^/users/(?<id>\\d+)$");
        _liquidRoutesManager.RegisterRoute(route);
        var queryParams = new Dictionary<string, string>();

        // Act
        var result = _liquidRoutesManager.GetRouteForPath("/users/123", queryParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(route);
        queryParams.Should().ContainKey("id");
        queryParams["id"].Should().Be("123");
    }

    [Fact]
    public void GetRouteForPath_ShouldHandleUrlEncodedParameters()
    {
        // Arrange
        var route = CreateTestRoute("^/search/(?<query>.+)$");
        _liquidRoutesManager.RegisterRoute(route);
        var queryParams = new Dictionary<string, string>();

        // Act
        var result = _liquidRoutesManager.GetRouteForPath("/search/hello%20world", queryParams);

        // Assert
        result.Should().NotBeNull();
        queryParams.Should().ContainKey("query");
        queryParams["query"].Should().Be("hello world");
    }

    [Fact]
    public void GetFileProviderForAsset_ShouldReturnFileProvider_WhenAssetExists()
    {
        // Arrange
        var mockFileProvider = new Mock<IFileProvider>();
        var mockFileInfo = new Mock<IFileInfo>();
        mockFileInfo.Setup(f => f.Exists).Returns(true);
        mockFileProvider.Setup(fp => fp.GetFileInfo("test.css")).Returns(mockFileInfo.Object);

        var route = CreateTestRoute("^/test$", "test.liquid", mockFileProvider.Object);
        _liquidRoutesManager.RegisterRoute(route);

        // Act
        var result = _liquidRoutesManager.GetFileProviderForAsset("test.css");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(mockFileProvider.Object);
    }

    [Fact]
    public void GetFileProviderForAsset_ShouldReturnNull_WhenAssetDoesNotExist()
    {
        // Arrange
        var mockFileProvider = new Mock<IFileProvider>();
        var mockFileInfo = new Mock<IFileInfo>();
        mockFileInfo.Setup(f => f.Exists).Returns(false);
        mockFileProvider.Setup(fp => fp.GetFileInfo("nonexistent.css")).Returns(mockFileInfo.Object);

        var route = CreateTestRoute("^/test$", "test.liquid", mockFileProvider.Object);
        _liquidRoutesManager.RegisterRoute(route);

        // Act
        var result = _liquidRoutesManager.GetFileProviderForAsset("nonexistent.css");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFileProviderForAsset_ShouldCheckMultipleProviders_WhenAssetNotFoundInFirst()
    {
        // Arrange
        var mockFileProvider1 = new Mock<IFileProvider>();
        var mockFileInfo1 = new Mock<IFileInfo>();
        mockFileInfo1.Setup(f => f.Exists).Returns(false);
        mockFileProvider1.Setup(fp => fp.GetFileInfo("test.css")).Returns(mockFileInfo1.Object);

        var mockFileProvider2 = new Mock<IFileProvider>();
        var mockFileInfo2 = new Mock<IFileInfo>();
        mockFileInfo2.Setup(f => f.Exists).Returns(true);
        mockFileProvider2.Setup(fp => fp.GetFileInfo("test.css")).Returns(mockFileInfo2.Object);

        var route1 = CreateTestRoute("^/test1$", "test1.liquid", mockFileProvider1.Object);
        var route2 = CreateTestRoute("^/test2$", "test2.liquid", mockFileProvider2.Object);
        _liquidRoutesManager.RegisterRoute(route1);
        _liquidRoutesManager.RegisterRoute(route2);

        // Act
        var result = _liquidRoutesManager.GetFileProviderForAsset("test.css");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(mockFileProvider2.Object);
    }

    [Fact]
    public void LiquidRoutes_ShouldReturnEmptyList_WhenNoRoutesRegistered()
    {
        // Act
        var routes = _liquidRoutesManager.LiquidRoutes;

        // Assert
        routes.Should().NotBeNull();
        routes.Should().BeEmpty();
    }

    [Fact]
    public void ErrorRoutes_ShouldReturnEmptyDictionary_WhenNoErrorRoutesRegistered()
    {
        // Act
        var errorRoutes = _liquidRoutesManager.ErrorRoutes;

        // Assert
        errorRoutes.Should().NotBeNull();
        errorRoutes.Should().BeEmpty();
    }
}