using FluentAssertions;
using Fluid;
using Fluid.Values;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using Kinetq.LiquidPages.Tests.Pages;
using Kinetq.LiquidPages.Tests.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kinetq.LiquidPages.Tests;

public class LiquidStartupTests
{
    private readonly ILiquidStartup _liquidStartup;
    private readonly Mock<ILiquidRoutesManager> _liquidRoutesManagerMock;
    private readonly Mock<ILiquidFilterManager> _liquidFilterManagerMock;
    private readonly Mock<IEnumerable<ILiquidRoute>> _liquidRoutesMock;
    private readonly Mock<IEnumerable<ILiquidErrorRoute>> _liquidErrorRoutesMock;
    private readonly Mock<IEnumerable<ILiquidFilter>> _liquidFiltersMock;
    private readonly Mock<ILiquidRegisteredTypesManager> _liquidRegisteredTypesManagerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IEnumerable<LiquidPageModel>> _liquidPageModelsMock;

    public LiquidStartupTests()
    {
        _liquidRoutesManagerMock = new Mock<ILiquidRoutesManager>();
        _liquidFilterManagerMock = new Mock<ILiquidFilterManager>();
        _liquidRoutesMock = new Mock<IEnumerable<ILiquidRoute>>();
        _liquidErrorRoutesMock = new Mock<IEnumerable<ILiquidErrorRoute>>();
        _liquidFiltersMock = new Mock<IEnumerable<ILiquidFilter>>();
        _liquidPageModelsMock = new Mock<IEnumerable<LiquidPageModel>>();
        _liquidRegisteredTypesManagerMock = new Mock<ILiquidRegisteredTypesManager>();

        var routes = new List<ILiquidRoute> { };
        _liquidRoutesMock.Setup(r => r.GetEnumerator()).Returns(routes.GetEnumerator());

        var errorRoutes = new List<ILiquidErrorRoute> { };
        _liquidErrorRoutesMock.Setup(r => r.GetEnumerator()).Returns(errorRoutes.GetEnumerator());

        var serviceCollection = new ServiceCollection();
        _serviceProvider = serviceCollection
            .AddSingleton(_liquidRoutesManagerMock.Object)
            .AddSingleton(_liquidFilterManagerMock.Object)
            .AddSingleton(_liquidRoutesMock.Object)
            .AddSingleton(_liquidErrorRoutesMock.Object)
            .AddSingleton(_liquidFiltersMock.Object)
            .AddSingleton(_liquidPageModelsMock.Object)
            .AddSingleton(_liquidRegisteredTypesManagerMock.Object)
            .AddScoped<ILiquidStartup, LiquidStartup>()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();

        _liquidStartup = _serviceProvider.GetRequiredService<ILiquidStartup>();
    }

    [Fact]
    public async Task RegisterRoutes_ShouldCallGetRouteOnEachLiquidRoute()
    {
        // Arrange
        var mockRoute1 = new Mock<ILiquidRoute>();
        var mockRoute2 = new Mock<ILiquidRoute>();
        var mockRoute3 = new Mock<ILiquidRoute>();

        var liquidRoute1 = CreateTestLiquidRoute("/test1", "test1.liquid");
        var liquidRoute2 = CreateTestLiquidRoute("/test2", "test2.liquid");
        var liquidRoute3 = CreateTestLiquidRoute("/api/users/{id}", "user.liquid");

        mockRoute1.Setup(r => r.GetRoute()).ReturnsAsync(liquidRoute1);
        mockRoute2.Setup(r => r.GetRoute()).ReturnsAsync(liquidRoute2);
        mockRoute3.Setup(r => r.GetRoute()).ReturnsAsync(liquidRoute3);

        var routes = new List<ILiquidRoute> { mockRoute1.Object, mockRoute2.Object, mockRoute3.Object };
        _liquidRoutesMock.Setup(r => r.GetEnumerator()).Returns(routes.GetEnumerator());

        // Act
        await _liquidStartup.RegisterRoutes();

        // Assert
        mockRoute1.Verify(r => r.GetRoute(), Times.Once);
        mockRoute2.Verify(r => r.GetRoute(), Times.Once);
        mockRoute3.Verify(r => r.GetRoute(), Times.Once);
    }

    [Fact]
    public async Task RegisterRoutes_ShouldRegisterEachRouteWithRoutesManager()
    {
        // Arrange
        var mockRoute1 = new Mock<ILiquidRoute>();
        var mockRoute2 = new Mock<ILiquidRoute>();

        var liquidRoute1 = CreateTestLiquidRoute("/test1", "test1.liquid");
        var liquidRoute2 = CreateTestLiquidRoute("/test2", "test2.liquid");

        mockRoute1.Setup(r => r.GetRoute()).ReturnsAsync(liquidRoute1);
        mockRoute2.Setup(r => r.GetRoute()).ReturnsAsync(liquidRoute2);

        var routes = new List<ILiquidRoute> { mockRoute1.Object, mockRoute2.Object };
        _liquidRoutesMock.Setup(r => r.GetEnumerator()).Returns(routes.GetEnumerator());

        // Act
        await _liquidStartup.RegisterRoutes();

        // Assert
        _liquidRoutesManagerMock.Verify(m => m.RegisterRoute(liquidRoute1), Times.Once);
        _liquidRoutesManagerMock.Verify(m => m.RegisterRoute(liquidRoute2), Times.Once);
    }

    [Fact]
    public async Task RegisterRoutes_ShouldRegisterEachErrorRouteWithRoutesManager()
    {
        // Arrange
        var mockRoute1 = new Mock<ILiquidErrorRoute>();
        var mockRoute2 = new Mock<ILiquidErrorRoute>();

        var liquidRoute1 = CreateTestErrorLiquidRoute("test1.liquid");
        var liquidRoute2 = CreateTestErrorLiquidRoute("test2.liquid");

        mockRoute1.Setup(r => r.GetRoute()).ReturnsAsync(liquidRoute1);
        mockRoute1.Setup(r => r.StatusCode).Returns(404);
        mockRoute2.Setup(r => r.GetRoute()).ReturnsAsync(liquidRoute2);
        mockRoute2.Setup(r => r.StatusCode).Returns(500);

        var errorRoutes = new List<ILiquidErrorRoute> { mockRoute1.Object, mockRoute2.Object };
        _liquidErrorRoutesMock.Setup(r => r.GetEnumerator()).Returns(errorRoutes.GetEnumerator());


        // Act
        await _liquidStartup.RegisterRoutes();

        // Assert
        _liquidRoutesManagerMock.Verify(m => m.RegisterErrorRoute(It.IsAny<int>(), liquidRoute1), Times.Once);
        _liquidRoutesManagerMock.Verify(m => m.RegisterErrorRoute(It.IsAny<int>(), liquidRoute2), Times.Once);
    }

    [Fact]
    public async Task RegisterRoutes_ShouldHandleEmptyRouteCollection()
    {
        // Arrange
        var routes = new List<ILiquidRoute>();
        _liquidRoutesMock.Setup(r => r.GetEnumerator()).Returns(routes.GetEnumerator());

        // Act
        await _liquidStartup.RegisterRoutes();

        // Assert
        _liquidRoutesManagerMock.Verify(m => m.RegisterRoute(It.IsAny<LiquidRoute>()), Times.Never);
    }

    [Fact]
    public async Task RegisterRoutes_ShouldPropagateExceptionFromGetRoute()
    {
        // Arrange
        var mockRoute = new Mock<ILiquidRoute>();
        var expectedException = new InvalidOperationException("Test exception");

        mockRoute.Setup(r => r.GetRoute()).ThrowsAsync(expectedException);

        var routes = new List<ILiquidRoute> { mockRoute.Object };
        _liquidRoutesMock.Setup(r => r.GetEnumerator()).Returns(routes.GetEnumerator());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _liquidStartup.RegisterRoutes());
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task RegisterFilters_ShouldCallGetFilterOnEachLiquidFilter()
    {
        // Arrange
        var mockFilter1 = new Mock<ILiquidFilter>();
        var mockFilter2 = new Mock<ILiquidFilter>();
        var mockFilter3 = new Mock<ILiquidFilter>();

        var filterDelegate1 = new FilterDelegate((input, args, tmpl) => new ValueTask<FluidValue>());
        var filterDelegate2 = new FilterDelegate((input, args, tmpl) => new ValueTask<FluidValue>());
        var filterDelegate3 = new FilterDelegate((input, args, tmpl) => new ValueTask<FluidValue>());

        mockFilter1.Setup(f => f.GetFilter()).ReturnsAsync(new LiquidFilter { Name = "uppercase", FilterDelegate = filterDelegate1 });
        mockFilter2.Setup(f => f.GetFilter()).ReturnsAsync(new LiquidFilter { Name = "lowercase", FilterDelegate = filterDelegate2 });
        mockFilter3.Setup(f => f.GetFilter()).ReturnsAsync(new LiquidFilter { Name = "trim", FilterDelegate = filterDelegate3 });

        var filters = new List<ILiquidFilter> { mockFilter1.Object, mockFilter2.Object, mockFilter3.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act
        await _liquidStartup.RegisterFilters();

        // Assert
        mockFilter1.Verify(f => f.GetFilter(), Times.Once);
        mockFilter2.Verify(f => f.GetFilter(), Times.Once);
        mockFilter3.Verify(f => f.GetFilter(), Times.Once);
    }

    [Fact]
    public async Task RegisterFilters_ShouldRegisterEachFilterWithFilterManager()
    {
        // Arrange
        var mockFilter1 = new Mock<ILiquidFilter>();
        var mockFilter2 = new Mock<ILiquidFilter>();

        var filterDelegate1 = new FilterDelegate((input, args, tmpl) => new ValueTask<FluidValue>());
        var filterDelegate2 = new FilterDelegate((input, args, tmpl) => new ValueTask<FluidValue>());

        mockFilter1.Setup(f => f.GetFilter()).ReturnsAsync(new LiquidFilter { Name = "uppercase", FilterDelegate = filterDelegate1 });
        mockFilter2.Setup(f => f.GetFilter()).ReturnsAsync(new LiquidFilter { Name = "lowercase", FilterDelegate = filterDelegate2 });

        var filters = new List<ILiquidFilter> { mockFilter1.Object, mockFilter2.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act
        await _liquidStartup.RegisterFilters();

        // Assert
        _liquidFilterManagerMock.Verify(m => m.RegisterFilter("uppercase", filterDelegate1), Times.Once);
        _liquidFilterManagerMock.Verify(m => m.RegisterFilter("lowercase", filterDelegate2), Times.Once);
    }

    [Fact]
    public async Task RegisterFilters_ShouldHandleEmptyFilterCollection()
    {
        // Arrange
        var filters = new List<ILiquidFilter>();
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act
        await _liquidStartup.RegisterFilters();

        // Assert
        _liquidFilterManagerMock.Verify(m => m.RegisterFilter(It.IsAny<string>(), It.IsAny<FilterDelegate>()), Times.Never);
    }

    [Fact]
    public async Task RegisterFilters_ShouldPropagateExceptionFromGetFilter()
    {
        // Arrange
        var mockFilter = new Mock<ILiquidFilter>();
        var expectedException = new InvalidOperationException("Filter exception");

        mockFilter.Setup(f => f.GetFilter()).ThrowsAsync(expectedException);

        var filters = new List<ILiquidFilter> { mockFilter.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _liquidStartup.RegisterFilters());
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task RegisterRoutes_ShouldProcessRoutesSequentially()
    {
        // Arrange
        var callOrder = new List<string>();
        var mockRoute1 = new Mock<ILiquidRoute>();
        var mockRoute2 = new Mock<ILiquidRoute>();

        var liquidRoute1 = CreateTestLiquidRoute("/test1", "test1.liquid");
        var liquidRoute2 = CreateTestLiquidRoute("/test2", "test2.liquid");

        mockRoute1.Setup(r => r.GetRoute())
            .Callback(() => callOrder.Add("route1"))
            .ReturnsAsync(liquidRoute1);
        mockRoute2.Setup(r => r.GetRoute())
            .Callback(() => callOrder.Add("route2"))
            .ReturnsAsync(liquidRoute2);

        _liquidRoutesManagerMock.Setup(m => m.RegisterRoute(liquidRoute1))
            .Callback(() => callOrder.Add("register1"));
        _liquidRoutesManagerMock.Setup(m => m.RegisterRoute(liquidRoute2))
            .Callback(() => callOrder.Add("register2"));

        var routes = new List<ILiquidRoute> { mockRoute1.Object, mockRoute2.Object };
        _liquidRoutesMock.Setup(r => r.GetEnumerator()).Returns(routes.GetEnumerator());

        // Act
        await _liquidStartup.RegisterRoutes();

        // Assert
        callOrder.Should().Equal("route1", "register1", "route2", "register2");
    }



    [Fact]
    public async Task RegisterFilters_ShouldProcessFiltersSequentially()
    {
        // Arrange
        var callOrder = new List<string>();
        var mockFilter1 = new Mock<ILiquidFilter>();
        var mockFilter2 = new Mock<ILiquidFilter>();

        var filterDelegate1 = new FilterDelegate((input, args, tmpl) => new ValueTask<FluidValue>());
        var filterDelegate2 = new FilterDelegate((input, args, tmpl) => new ValueTask<FluidValue>());

        mockFilter1.Setup(f => f.GetFilter())
            .Callback(() => callOrder.Add("filter1"))
            .ReturnsAsync(new LiquidFilter { Name = "uppercase", FilterDelegate = filterDelegate1 });
        mockFilter2.Setup(f => f.GetFilter())
            .Callback(() => callOrder.Add("filter2"))
            .ReturnsAsync(new LiquidFilter { Name = "lowercase", FilterDelegate = filterDelegate2 });

        _liquidFilterManagerMock.Setup(m => m.RegisterFilter("uppercase", filterDelegate1))
            .Callback(() => callOrder.Add("registerfilter1"));
        _liquidFilterManagerMock.Setup(m => m.RegisterFilter("lowercase", filterDelegate2))
            .Callback(() => callOrder.Add("registerfilter2"));

        var filters = new List<ILiquidFilter> { mockFilter1.Object, mockFilter2.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act
        await _liquidStartup.RegisterFilters();

        // Assert
        callOrder.Should().Equal("filter1", "registerfilter1", "filter2", "registerfilter2");
    }

    private LiquidRoute CreateTestLiquidRoute(string routeTemplate, string templatePath)
    {
        return new LiquidRoute
        {
            RouteTemplate = routeTemplate,
            LiquidTemplatePath = templatePath,
            FileProvider = new Microsoft.Extensions.FileProviders.NullFileProvider(),
            Execute = async (model) => await Task.FromResult(new { Message = "Test" }),
            QueryParams = new Dictionary<string, string>()
        };
    }

    private LiquidRoute CreateTestErrorLiquidRoute(string templatePath)
    {
        return new LiquidRoute
        {
            LiquidTemplatePath = templatePath,
            FileProvider = new Microsoft.Extensions.FileProviders.NullFileProvider(),
            QueryParams = new Dictionary<string, string>()
        };
    }

    // --- Concrete test page model helpers (add alongside existing private helpers ---

    [LiquidPage("/test1", "test1.liquid")]
    private class TestPageModel1 : LiquidPageModel
    {
        public bool OnGetCalled { get; private set; }
        public bool OnPostCalled { get; private set; }

        public override IFileProvider GetFileProvider() => new NullFileProvider();
        public override Task OnGetAsync(LiquidRequestModel request) { OnGetCalled = true; return Task.CompletedTask; }
        public override Task OnPostAsync(LiquidRequestModel request) { OnPostCalled = true; return Task.CompletedTask; }
    }

    [LiquidPage("test3.liquid")]
    private class TestPageModel3 : LiquidPageModel
    {
        public bool OnGetCalled { get; private set; }
        public bool OnPostCalled { get; private set; }

        public override IFileProvider GetFileProvider() => new NullFileProvider();
        public override Task OnGetAsync(LiquidRequestModel request) { OnGetCalled = true; return Task.CompletedTask; }
        public override Task OnPostAsync(LiquidRequestModel request) { OnPostCalled = true; return Task.CompletedTask; }
    }

    [LiquidPage("/test2", "test2.liquid")]
    private class TestPageModel2 : LiquidPageModel
    {
        public override IFileProvider GetFileProvider() => new NullFileProvider();
    }

    // --- Tests ---

    [Fact]
    public async Task RegisterPageModels_ShouldRegisterRouteForEachPageModel()
    {
        // Arrange
        var pageModel1 = new TestPageModel1();
        var pageModel2 = new TestPageModel2();

        var pageModels = new List<LiquidPageModel> { pageModel1, pageModel2 };
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        // Act
        await _liquidStartup.RegisterPageModels();

        // Assert
        _liquidRoutesManagerMock.Verify(m => m.RegisterRoute(It.IsAny<LiquidRoute>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterPageModels_ShouldRegisterRouteCustomRoute_WhenPageModelAvailable()
    {
        // Arrange
        var pageModel3 = new TestPageModel3();

        var pageModels = new List<LiquidPageModel> { pageModel3 };
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        string customRoutePattern = "/test-1-1";
        // Act
        await _liquidStartup.RegisterPageModels((options) =>
        {
            options.AddPageRoute(typeof(TestPageModel3), customRoutePattern);
        });

        // Assert
        _liquidRoutesManagerMock.Verify(m => 
            m.RegisterRoute(It.Is<LiquidRoute>(lr => lr.RouteTemplate.ToString() == customRoutePattern)), 
            Times.Exactly(1));
    }

    [Fact]
    public async Task RegisterPageModels_ShouldRegisterRouteCustomRouteAndAttributeRoute_WhenPageModelAvailable()
    {
        // Arrange
        var pageModel1 = new TestPageModel1();

        var pageModels = new List<LiquidPageModel> { pageModel1 };
        _liquidPageModelsMock.Setup(p => 
            p.GetEnumerator())
            .Returns(() => pageModels.GetEnumerator());

        string customRoutePattern = "/test-1-1";
        // Act
        await _liquidStartup.RegisterPageModels((options) =>
        {
            options.AddPageRoute(typeof(TestPageModel1), customRoutePattern);
        });

        // Assert
        _liquidRoutesManagerMock.Verify(m =>
                m.RegisterRoute(It.IsAny<LiquidRoute>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterPageModels_ShouldRegisterRouteWithCorrectPatternAndTemplatePath()
    {
        // Arrange
        var pageModel = new TestPageModel1();
        var pageModels = new List<LiquidPageModel> { pageModel };
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        LiquidRoute? registeredRoute = null;
        _liquidRoutesManagerMock
            .Setup(m => m.RegisterRoute(It.IsAny<LiquidRoute>()))
            .Callback<LiquidRoute>(r => registeredRoute = r);

        // Act
        await _liquidStartup.RegisterPageModels();

        // Assert
        registeredRoute.Should().NotBeNull();
        registeredRoute!.RouteTemplate.Should().Be("/test1");
        registeredRoute.LiquidTemplatePath.Should().Be("test1.liquid");
    }

    [Fact]
    public async Task RegisterPageModels_ShouldRegisterRouteWithFileProviderFromPageModel()
    {
        // Arrange
        var pageModel = new TestPageModel1();
        var pageModels = new List<LiquidPageModel> { pageModel };
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        LiquidRoute? registeredRoute = null;
        _liquidRoutesManagerMock
            .Setup(m => m.RegisterRoute(It.IsAny<LiquidRoute>()))
            .Callback<LiquidRoute>(r => registeredRoute = r);

        // Act
        await _liquidStartup.RegisterPageModels();

        // Assert
        registeredRoute!.FileProvider.Should().BeOfType<NullFileProvider>();
    }

    [Fact]
    public async Task RegisterPageModels_ExecuteDelegate_ShouldCallOnGetAsync_WhenMethodIsGet()
    {
        // Arrange
        var pageModel = new TestPageModel1();
        var pageModels = new List<LiquidPageModel> { pageModel };
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        LiquidRoute? registeredRoute = null;
        _liquidRoutesManagerMock
            .Setup(m => m.RegisterRoute(It.IsAny<LiquidRoute>()))
            .Callback<LiquidRoute>(r => registeredRoute = r);

        await _liquidStartup.RegisterPageModels();

        var request = new LiquidRequestModel { Method = "GET", LiquidPageModel = pageModel};

        // Act
        await registeredRoute!.Execute!(request);

        // Assert
        pageModel.OnGetCalled.Should().BeTrue();
        pageModel.OnPostCalled.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterPageModels_ExecuteDelegate_ShouldCallOnPostAsync_WhenMethodIsPost()
    {
        // Arrange
        var pageModel = new TestPageModel1();
        var pageModels = new List<LiquidPageModel> { pageModel };
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        LiquidRoute? registeredRoute = null;
        _liquidRoutesManagerMock
            .Setup(m => m.RegisterRoute(It.IsAny<LiquidRoute>()))
            .Callback<LiquidRoute>(r => registeredRoute = r);

        await _liquidStartup.RegisterPageModels();

        var request = new LiquidRequestModel { Method = "POST", LiquidPageModel = pageModel};

        // Act
        await registeredRoute!.Execute!(request);

        // Assert
        pageModel.OnPostCalled.Should().BeTrue();
        pageModel.OnGetCalled.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterPageModels_ExecuteDelegate_ShouldReturnPageModelInstance()
    {
        // Arrange
        var pageModel = new TestPageModel1();
        var pageModels = new List<LiquidPageModel> { pageModel };
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        LiquidRoute? registeredRoute = null;
        _liquidRoutesManagerMock
            .Setup(m => m.RegisterRoute(It.IsAny<LiquidRoute>()))
            .Callback<LiquidRoute>(r => registeredRoute = r);

        await _liquidStartup.RegisterPageModels();

        // Act
        var result = await registeredRoute!.Execute!(new LiquidRequestModel { Method = "GET", LiquidPageModel = pageModel});

        // Assert
        result.Should().BeSameAs(pageModel);
    }

    [Fact]
    public async Task RegisterPageModels_ShouldHandleEmptyPageModelCollection()
    {
        // Arrange
        var pageModels = new List<LiquidPageModel>();
        _liquidPageModelsMock.Setup(p => p.GetEnumerator()).Returns(pageModels.GetEnumerator());

        // Act
        await _liquidStartup.RegisterPageModels();

        // Assert
        _liquidRoutesManagerMock.Verify(m => m.RegisterRoute(It.IsAny<LiquidRoute>()), Times.Never);
    }

    [Fact]
    public async Task RegisterPageModels_ShouldRegisterTypes()
    {
        // Arrange
        var pageModels = new List<LiquidPageModel>()
        {
            new AboutUsModel()
        };
        _liquidPageModelsMock
            .Setup(p =>
                p.GetEnumerator())
            .Returns(pageModels.GetEnumerator()
            );

        // Act
        await _liquidStartup.RegisterPageModels();

        // Assert
        _liquidRegisteredTypesManagerMock.Verify(m => m.RegisterType(typeof(NavItemViewModel)), Times.Once);
        _liquidRegisteredTypesManagerMock.Verify(m => m.RegisterType(typeof(NestedTypeOne)), Times.Once);
        _liquidRegisteredTypesManagerMock.Verify(m => m.RegisterType(typeof(NestedTypeTwo)), Times.Once);
        _liquidRegisteredTypesManagerMock.Verify(m => m.RegisterType(typeof(AboutUsModel)), Times.Once);
    }


    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}