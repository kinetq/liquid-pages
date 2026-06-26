using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Router.Helpers;
using Kinetq.LiquidPages.Router.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Kinetq.LiquidPages.Avalonia.Sample;

internal static class ServiceRegistration
{
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection().AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "hh:mm:ss ";
            }).SetMinimumLevel(LogLevel.Debug);
        });

        services.AddLiquidPages(typeof(ServiceRegistration).Assembly);
        services.AddLiquidRouter();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var startup = scope.ServiceProvider.GetRequiredService<ILiquidStartup>();
        startup.RegisterPageModels();
        startup.RegisterFileProvider("/", new CompositeFileProvider(
            new EmbeddedFileProvider(typeof(ServiceRegistration).Assembly),
            new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "Static"))));

        var routeTree = scope.ServiceProvider.GetRequiredService<IRouteTree>();
        routeTree.Initialize();

        return serviceProvider;
    }
}
