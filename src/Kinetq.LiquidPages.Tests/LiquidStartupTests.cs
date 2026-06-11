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
    private readonly Mock<ITemplateOptionsManager> _templateOptionsManagerMock;
    private readonly Mock<IEnumerable<ILiquidFilter>> _liquidFiltersMock;
    private readonly Mock<ILiquidRegisteredTypesManager> _liquidRegisteredTypesManagerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IEnumerable<LiquidPageModel>> _liquidPageModelsMock;

    public LiquidStartupTests()
    {
        _liquidRoutesManagerMock = new Mock<ILiquidRoutesManager>();
        _liquidFilterManagerMock = new Mock<ILiquidFilterManager>();
        _templateOptionsManagerMock = new Mock<ITemplateOptionsManager>();
        _liquidFiltersMock = new Mock<IEnumerable<ILiquidFilter>>();
        _liquidPageModelsMock = new Mock<IEnumerable<LiquidPageModel>>();
        _liquidRegisteredTypesManagerMock = new Mock<ILiquidRegisteredTypesManager>();

        var serviceCollection = new ServiceCollection();
        _serviceProvider = serviceCollection
            .AddSingleton(_liquidRoutesManagerMock.Object)
            .AddSingleton(_liquidFilterManagerMock.Object)
            .AddSingleton(_templateOptionsManagerMock.Object)
            .AddSingleton(_liquidFiltersMock.Object)
            .AddSingleton(_liquidPageModelsMock.Object)
            .AddSingleton(_liquidRegisteredTypesManagerMock.Object)
            .AddScoped<ILiquidStartup, LiquidStartup>()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();

        _liquidStartup = _serviceProvider.GetRequiredService<ILiquidStartup>();
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

        mockFilter1.Setup(f => f.GetFilter()).Returns(new LiquidFilter { Name = "uppercase", FilterDelegate = filterDelegate1 });
        mockFilter2.Setup(f => f.GetFilter()).Returns(new LiquidFilter { Name = "lowercase", FilterDelegate = filterDelegate2 });
        mockFilter3.Setup(f => f.GetFilter()).Returns(new LiquidFilter { Name = "trim", FilterDelegate = filterDelegate3 });

        var filters = new List<ILiquidFilter> { mockFilter1.Object, mockFilter2.Object, mockFilter3.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act
        _liquidStartup.RegisterFilters();

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

        mockFilter1.Setup(f => f.GetFilter()).Returns(new LiquidFilter { Name = "uppercase", FilterDelegate = filterDelegate1 });
        mockFilter2.Setup(f => f.GetFilter()).Returns(new LiquidFilter { Name = "lowercase", FilterDelegate = filterDelegate2 });

        var filters = new List<ILiquidFilter> { mockFilter1.Object, mockFilter2.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act
        _liquidStartup.RegisterFilters();

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
        _liquidStartup.RegisterFilters();

        // Assert
        _liquidFilterManagerMock.Verify(m => m.RegisterFilter(It.IsAny<string>(), It.IsAny<FilterDelegate>()), Times.Never);
    }

    [Fact]
    public async Task RegisterFilters_ShouldPropagateExceptionFromGetFilter()
    {
        // Arrange
        var mockFilter = new Mock<ILiquidFilter>();
        var expectedException = new InvalidOperationException("Filter exception");

        mockFilter.Setup(f => f.GetFilter()).Throws(expectedException);

        var filters = new List<ILiquidFilter> { mockFilter.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _liquidStartup.RegisterFilters());
        exception.Should().Be(expectedException);
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
            .Returns(new LiquidFilter { Name = "uppercase", FilterDelegate = filterDelegate1 });
        mockFilter2.Setup(f => f.GetFilter())
            .Callback(() => callOrder.Add("filter2"))
            .Returns(new LiquidFilter { Name = "lowercase", FilterDelegate = filterDelegate2 });

        _liquidFilterManagerMock.Setup(m => m.RegisterFilter("uppercase", filterDelegate1))
            .Callback(() => callOrder.Add("registerfilter1"));
        _liquidFilterManagerMock.Setup(m => m.RegisterFilter("lowercase", filterDelegate2))
            .Callback(() => callOrder.Add("registerfilter2"));

        var filters = new List<ILiquidFilter> { mockFilter1.Object, mockFilter2.Object };
        _liquidFiltersMock.Setup(f => f.GetEnumerator()).Returns(filters.GetEnumerator());

        // Act
        _liquidStartup.RegisterFilters();

        // Assert
        callOrder.Should().Equal("filter1", "registerfilter1", "filter2", "registerfilter2");
    }

    [Fact]
    public void RegisterFileProvider_ShouldRegisterTemplateOptionsForPrefix()
    {
        // Arrange
        var fileProvider = new NullFileProvider();

        // Act
        _liquidStartup.RegisterFileProvider("/test", fileProvider);

        // Assert
        _templateOptionsManagerMock.Verify(m => m.RegisterTemplateOptions("/test", fileProvider), Times.Once);
    }

    // --- Concrete test page model helpers ---

    [LiquidPage("/test1", "test1.liquid")]
    private class TestPageModel1 : LiquidPageModel
    {
        public bool OnGetCalled { get; private set; }
        public bool OnPostCalled { get; private set; }

        public override Task OnGetAsync(LiquidRequestModel request) { OnGetCalled = true; return Task.CompletedTask; }
        public override Task OnPostAsync(LiquidRequestModel request) { OnPostCalled = true; return Task.CompletedTask; }
    }

    [LiquidPage("test3.liquid")]
    private class TestPageModel3 : LiquidPageModel
    {
        public bool OnGetCalled { get; private set; }
        public bool OnPostCalled { get; private set; }

        public override Task OnGetAsync(LiquidRequestModel request) { OnGetCalled = true; return Task.CompletedTask; }
        public override Task OnPostAsync(LiquidRequestModel request) { OnPostCalled = true; return Task.CompletedTask; }
    }

    [LiquidPage("/test2", "test2.liquid")]
    private class TestPageModel2 : LiquidPageModel
    {
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
        _liquidStartup.RegisterPageModels();

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
        _liquidStartup.RegisterPageModels((options) => { options.AddPageRoute(typeof(TestPageModel3), customRoutePattern); });

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
        _liquidStartup.RegisterPageModels((options) => { options.AddPageRoute(typeof(TestPageModel1), customRoutePattern); });

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
        _liquidStartup.RegisterPageModels();

        // Assert
        registeredRoute.Should().NotBeNull();
        registeredRoute!.RouteTemplate.Should().Be("/test1");
        registeredRoute.LiquidTemplatePath.Should().Be("test1.liquid");
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

        _liquidStartup.RegisterPageModels();

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

        _liquidStartup.RegisterPageModels();

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

        _liquidStartup.RegisterPageModels();

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
        _liquidStartup.RegisterPageModels();

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
        _liquidStartup.RegisterPageModels();

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