using Fluid;
using Fluid.Values;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Managers;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Kinetq.LiquidPages.Tests;

public class HtmlRendererTests : IAsyncLifetime
{
    private IHtmlRenderer _htmlRenderer;
    private TemplateOptions _postsTemplateOptions;
    private IFileProvider _embeddedFileProvider;
    private IFileProvider _physicalFileProvider;

    public Task InitializeAsync()
    {
        var templateOptionsManagerMock = new Mock<ITemplateOptionsManager>();

        _embeddedFileProvider = new EmbeddedFileProvider(typeof(LiquidResponseMiddlewareTests).Assembly, "Kinetq.LiquidPages.Tests.Templates");
        string executingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        _physicalFileProvider = new PhysicalFileProvider(executingDirectory);

        var embeddedOptions = CreateTemplateOptions(_embeddedFileProvider);
        var physicalOptions = CreateTemplateOptions(_physicalFileProvider);
        _postsTemplateOptions = CreateTemplateOptions(_physicalFileProvider);

        templateOptionsManagerMock
            .Setup(x => x.GetTemplateOptions(It.IsAny<string>()))
            .Returns((string path) =>
            {
                if (path.StartsWith("/posts", StringComparison.OrdinalIgnoreCase))
                    return _postsTemplateOptions;

                if (path.StartsWith("/physical", StringComparison.OrdinalIgnoreCase))
                    return physicalOptions;

                return embeddedOptions;
            });

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IFluidParserManager, FluidParserManager>();
        serviceCollection.AddSingleton<ILiquidTemplateManager, LiquidTemplateManager>();
        serviceCollection.AddSingleton(templateOptionsManagerMock.Object);
        serviceCollection.AddSingleton<IHtmlRenderer, HtmlRenderer>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _htmlRenderer = serviceProvider.GetRequiredService<IHtmlRenderer>();

        return Task.CompletedTask;
    }

    private static TemplateOptions CreateTemplateOptions(IFileProvider fileProvider)
    {
        var options = new TemplateOptions
        {
            FileProvider = fileProvider,
            MemberAccessStrategy = new DefaultMemberAccessStrategy
            {
                MemberNameStrategy = MemberNameStrategies.SnakeCase
            }
        };

        options.MemberAccessStrategy.Register(typeof(RenderViewModel));
        options.MemberAccessStrategy.Register(typeof(Page));
        options.MemberAccessStrategy.Register(typeof(Post));

        return options;
    }

    [Fact]
    private async Task Can_Find_Embedded_Templates()
    {
        var liquidRoute = new LiquidRoute()
        {
            RouteTemplate = "/",
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel();

        string? html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        Assert.NotNull(html);
    }

    [Fact]
    private async Task Can_Find_Physical_Templates()
    {
        var liquidRoute = new LiquidRoute()
        {
            RouteTemplate = "/physical",
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel();

        string? html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        Assert.NotNull(html);
    }

    [Fact]
    private async Task Can_Render_View_Model()
    {
        var liquidRoute = new LiquidRoute()
        {
            RouteTemplate = "/physical",
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel()
        {
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        string html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        Assert.Contains("<h2>Test Heading</h2>", html);
    }

    [Fact]
    private async Task Rertuns_Errors_For_Malformed_Liquid_Syntax()
    {
        var liquidRoute = new LiquidRoute()
        {
            RouteTemplate = "/physical",
            LiquidTemplatePath = "malformed.liquid"
        };

        var renderModel = new RenderModel()
        {
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _htmlRenderer.RenderHtml(renderModel, liquidRoute));
    }

    [Fact]
    private async Task Rertuns_Errors_For_Malformed_HTML_Syntax()
    {
        var liquidRoute = new LiquidRoute()
        {
            RouteTemplate = "/physical",
            LiquidTemplatePath = "malformed_html.liquid"
        };

        var renderModel = new RenderModel()
        {
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        string? html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        Assert.NotNull(html);
        Assert.Contains("Test Heading", html);
    }

    [Fact]
    private async Task Returns_Posts_From_Registered_Filter()
    {
        var liquidRoute = new LiquidRoute()
        {
            RouteTemplate = "/posts",
            LiquidTemplatePath = "index.liquid"
        };

        var renderModel = new RenderModel()
        {
            ViewModel = new RenderViewModel()
            {
                Page = new Page()
                {
                    Heading = "Test Heading"
                }
            }
        };

        _postsTemplateOptions.Filters.AddFilter(
            "get_posts",
            (input, arguments, context) =>
                    {
                        var posts = new List<Post>()
                        {
                            new Post()
                            {
                                Title = "First Post",
                                Url = "/posts/first-post",
                                Date = new DateTime(2024, 1, 1)
                            },
                            new Post()
                            {
                                Title = "Second Post",
                                Url = "/posts/second-post",
                                Date = new DateTime(2024, 1, 1)
                            },
                        };
                        return FluidValue.Create(posts, context.Options);
                    });

        string html = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        var postCount = html.Split("class=\"post\"").Length - 1;
        Assert.Equal(2, postCount);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

public class RenderViewModel
{
    public Page Page { get; set; }
}

public class Page
{
    public string Heading { get; set; }
}

public class Post
{
    public string Title { get; set; }
    public string Url { get; set; }
    public DateTime Date { get; set; }
}