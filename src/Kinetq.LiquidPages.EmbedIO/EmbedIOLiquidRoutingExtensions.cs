using EmbedIO;
using EmbedIO.Routing;
using Kinetq.LiquidPages.Interfaces;
using System.Text.RegularExpressions;

namespace Kinetq.LiquidPages.EmbedIO;

public static class EmbedIOLiquidRoutingExtensions
{
    public static WebServer WithLiquidPages(this WebServer webServer, ILiquidResponseMiddleware middleware, ILiquidRoutesManager routesManager, Regex[]? excludedPaths = null)
    {
        foreach (var route in routesManager.LiquidRoutes)
        {
            if (string.IsNullOrWhiteSpace(route.RouteTemplate))
            {
                continue;
            }

            webServer.WithModule(new LiquidWebModule(route.RouteTemplate)
            {
                LiquidResponseMiddleware = middleware,
                LiquidRoute = route,
                ExcludedPaths = excludedPaths ?? []
            });
        }

        webServer.WithModule(new LiquidWebModule("/")
        {
            LiquidResponseMiddleware = middleware,
            ExcludedPaths = excludedPaths ?? []
        });

        return webServer;
    }
}
