using System.Net;
using FluentAssertions;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Managers;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetq.LiquidPages.Tests;

public class LiquidRoutesManagerTests : IAsyncLifetime
{
    private ILiquidRoutesManager _liquidRoutesManager;
    private ServiceProvider _serviceProvider;

    public Task InitializeAsync()
    {
        var serviceCollection = new ServiceCollection();
        _serviceProvider = serviceCollection
            .AddScoped<ILiquidRoutesManager, LiquidRoutesManager>()
            .AddScoped<ILiquidResponseMiddleware, LiquidResponseMiddleware>()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();

        _liquidRoutesManager = _serviceProvider.GetRequiredService<ILiquidRoutesManager>();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        return Task.CompletedTask;
    }

    private LiquidRoute CreateTestRoute(string routeTemplate, string templatePath = "test.liquid")
    {
        return new LiquidRoute
        {
            RouteTemplate = routeTemplate,
            LiquidTemplatePath = templatePath,
            Execute = async (model) => await Task.FromResult(new { Message = "Test" })
        };
    }

    [Fact]
    public void RegisterRoute_ShouldAddRoute_WhenValidRouteProvided()
    {
        // Arrange
        var route = CreateTestRoute("/test");

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
        var route1 = CreateTestRoute("/test1");
        var route2 = CreateTestRoute("/test2");
        var route3 = CreateTestRoute("/api/users/{id}");

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
        var errorRoute = CreateTestRoute("/{*path}", "404.liquid");

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
        var errorRoute1 = CreateTestRoute("/{*path}", "error1.liquid");
        var errorRoute2 = CreateTestRoute("/{*path}", "error2.liquid");

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
        var errorRoute = CreateTestRoute("/{*path}", "404.liquid");
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