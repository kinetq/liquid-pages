using SimpleW;
using SimpleW.Helper.Razor;
using SimpleW.Modules;
using System.Net;

namespace Kinetq.LiquidPages.SimpleW.Razor.Sample;

class Program
{
    static async Task Main()
    {
        var server = new SimpleWServer(IPAddress.Any, 2015);
        server.Configure(options => {
            // Always beneficial socket options
            options.TcpNoDelay = true;
            options.ReuseAddress = true;
            options.TcpKeepAlive = true;

            // Advanced tuning (platform dependent)
            options.AcceptPerCore = true;
        });

        server.UseRazorModule(options =>
              {
                  options.ViewsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views");
                  // or: options.ViewsPath = Path.GetFullPath("Views");
              });

        server.UseStaticFilesModule(options =>
        {
            options.Path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Static");
            options.Prefix = "/Static";
            options.CacheTimeout = TimeSpan.FromDays(1d);
            options.AutoIndex = true;
        });

        server.MapGet("/", () =>
        {
            var model = new { Title = "SimpleW Razor Sample", H1 = "Welcome to SimpleW.Helper.Razor" };
            return RazorResults.View("Home/Index", model)
                .WithViewBag(vb =>
                {
                    vb.Title = "SimpleW Razor Sample";
                });
        });

        await server.RunAsync();
    }
}
