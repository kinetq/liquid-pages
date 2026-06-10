using EmbedIO;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.EmbedIO;

public static class EmbedIOLiquidRoutingExtensions
{
    public static WebServer WithLiquidPages(this WebServer webServer, ILiquidResponseMiddleware middleware, ILiquidRoutesManager routesManager)
    {
        webServer.WithModule(new LiquidWebModule("/", routesManager)
        {
            LiquidResponseMiddleware = middleware
        });

        return webServer;
    }
}
