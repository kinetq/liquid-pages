using EmbedIO;
using Kinetq.LiquidPages.EmbedIO;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Kinetq.LiquidPages.Sample
{ public class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            var serviceProvider = GetContainer();
            var startup = serviceProvider.GetService<ILiquidStartup>();

            await startup.RegisterPageModels();

            var webServer = new WebServer("http://*:5662");
            webServer.WithModule(new LiquidWebModule("/")
            {
                LiquidResponseMiddleware = serviceProvider.GetService<ILiquidResponseMiddleware>(),
                ExcludedPaths = new Regex[]
                {
                    new Regex("^/api/.*")
                }
            });

            await webServer.RunAsync();
        }

        static IServiceProvider GetContainer()
        {
            // Create the container builder.
            var services = new ServiceCollection().AddLogging(builder =>
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
            services.AddLiquidPages();

            return services.BuildServiceProvider();
        }
    }
}
