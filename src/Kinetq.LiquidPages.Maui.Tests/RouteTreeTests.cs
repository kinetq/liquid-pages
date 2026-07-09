using FluentAssertions;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Moq;
using Xunit;

namespace Kinetq.LiquidPages.Maui.Tests;

public class RouteTreeTests
{
    [Fact]
    public void Initialize_ShouldAddRoutesFromManager()
    {
        var homeRoute = CreateRoute("/");
        var productRoute = CreateRoute("/products/{id}");

        var routesManagerMock = new Mock<ILiquidRoutesManager>();
        routesManagerMock.SetupGet(x => x.LiquidRoutes).Returns(new List<LiquidRoute> { homeRoute, productRoute });

        var routeTree = new RouteTree(routesManagerMock.Object);

        routeTree.Initialize();

        var homeMatch = routeTree.Match("/");
        var productMatch = routeTree.Match("/products/42");

        homeMatch.Should().NotBeNull();
        homeMatch!.LiquidRoute.Should().Be(homeRoute);

        productMatch.Should().NotBeNull();
        productMatch!.LiquidRoute.Should().Be(productRoute);
        productMatch.RouteValues!.TryGetValue("id", out var id).Should().BeTrue();
        id.Should().Be("42");
    }

    [Fact]
    public void Match_ShouldPreferStaticSegmentOverParameterSegment()
    {
        var staticRoute = CreateRoute("/items/new");
        var parameterRoute = CreateRoute("/items/{id}");

        var routeTree = new RouteTree(CreateRoutesManager(staticRoute, parameterRoute));
        routeTree.Initialize();

        var match = routeTree.Match("/items/new");

        match.Should().NotBeNull();
        match!.LiquidRoute.Should().Be(staticRoute);
        match.RouteValues!.ContainsKey("id").Should().BeFalse();
    }

    [Fact]
    public void Match_ShouldReturnNull_WhenPathDoesNotExist()
    {
        var routeTree = new RouteTree(CreateRoutesManager(CreateRoute("/users/{id}")));
        routeTree.Initialize();

        var match = routeTree.Match("/products/123");

        match.Should().BeNull();
    }

    [Fact]
    public void Match_ShouldCaptureAllRouteParameters_ForNestedRoutes()
    {
        var route = CreateRoute("/users/{userId}/orders/{orderId}");
        var routeTree = new RouteTree(CreateRoutesManager(route));
        routeTree.Initialize();

        var match = routeTree.Match("/users/7/orders/99");

        match.Should().NotBeNull();
        match!.LiquidRoute.Should().Be(route);
        match.RouteValues!.TryGetValue("userId", out var userId).Should().BeTrue();
        match.RouteValues!.TryGetValue("orderId", out var orderId).Should().BeTrue();
        userId.Should().Be("7");
        orderId.Should().Be("99");
    }

    private static ILiquidRoutesManager CreateRoutesManager(params LiquidRoute[] routes)
    {
        var routesManagerMock = new Mock<ILiquidRoutesManager>();
        routesManagerMock.SetupGet(x => x.LiquidRoutes).Returns(routes.ToList());
        return routesManagerMock.Object;
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
