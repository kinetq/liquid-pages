using EmbedIO;
using EmbedIO.Files;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Kinetq.LiquidPages.EmbedIO.Sample
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
            string workingDirectory = Directory.GetCurrentDirectory();
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            startup.RegisterFileProvider("/", new PhysicalFileProvider(projectDirectory));

            var webServer = new WebServer("http://*:5662");
            
            var staticFolderPath = Path.Combine(AppContext.BaseDirectory, "Static");
            webServer.WithStaticFolder("/Static", staticFolderPath, true, m => m
                .WithContentCaching());

            var middleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>();
            var routesManager = serviceProvider.GetRequiredService<ILiquidRoutesManager>();
            webServer.WithLiquidPages(middleware, routesManager);


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
            services.AddLiquidPages(typeof(Program).Assembly);

            return services.BuildServiceProvider();
        }
    }
}
