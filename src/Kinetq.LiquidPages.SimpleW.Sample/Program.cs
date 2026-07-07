using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SimpleW;
using SimpleW.Modules;
using System.Net;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Kinetq.LiquidPages.SimpleW.Sample
{
    class Program
    {

        static async Task Main()
        {
            var container = GetContainer();

            // listen to all IPs port 2015
            var server = new SimpleWServer(IPAddress.Any, 2015);
            server.Configure(options => {
                // Always beneficial socket options
                options.TcpNoDelay = true;
                options.ReuseAddress = true;
                options.TcpKeepAlive = true;

                // Advanced tuning (platform dependent)
                options.AcceptPerCore = true;
            });

            var liquidRoutesManager = container.GetRequiredService<ILiquidRoutesManager>();
            var liquidResponseMiddleware = container.GetRequiredService<ILiquidResponseMiddleware>();
            var liquidStartup = container.GetRequiredService<ILiquidStartup>();

            liquidStartup.RegisterPageModels();
            liquidStartup.RegisterFileProvider("/", new EmbeddedFileProvider(typeof(Program).Assembly));

            server.UseStaticFilesModule(options =>
            {
                options.Path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Static");                  // serve your files located here
                options.Prefix = "/Static";                           // to "/" endpoint
                options.CacheTimeout = TimeSpan.FromDays(1d);    // cached for 24h
                options.AutoIndex = true;                       // enable autoindex if no index.html exists in the directory
            });
            server.UseModule(new LiquidPagesModule(liquidRoutesManager, liquidResponseMiddleware)
            {
                MapFallback404 = true
            });

            // run server
            await server.RunAsync();
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
