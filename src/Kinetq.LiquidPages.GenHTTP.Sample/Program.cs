using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Kinetq.LiquidPages.GenHTTP.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            var serviceProvider = GetContainer();
            var startup = serviceProvider.GetService<ILiquidStartup>();

            startup.RegisterFileProvider("/", new EmbeddedFileProvider(typeof(Program).Assembly));
            startup.RegisterPageModels();

            var middleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>();
            var routesManager = serviceProvider.GetRequiredService<ILiquidRoutesManager>();

            var staticResources = Resources.From(ResourceTree.FromDirectory(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Static")));
            var app = Layout.Create()
                .Add("Static", staticResources)
                .Add(new LiquidHandlerBuilder(middleware, routesManager));

            var server = await Host.Create()
                 .Handler(app)
                 .Bind(IPAddress.Any, 8080)
                 .RunAsync();
        }

        static IServiceProvider GetContainer()
        {
            // Create the container builder.
            var services = new ServiceCollection()
                .AddLogging(builder =>
            {
                builder.ClearProviders();
                // Clear Microsoft's default providers (like eventlogs and others)
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                }).SetMinimumLevel(LogLevel.Debug);
            });
            services.AddLiquidPages(typeof(Program).Assembly);

            return services.BuildServiceProvider();
        }
    }
}
