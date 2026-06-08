using GenHTTP.Api.Content;
using Kinetq.LiquidPages.Interfaces;
using GenHTTP.Modules.Layouting;

namespace Kinetq.LiquidPages.GenHTTP;

public sealed class LiquidHandlerBuilder : IHandlerBuilder<LiquidHandlerBuilder>
{
    private readonly ILiquidResponseMiddleware _middleware;
    private readonly ILiquidRoutesManager _routesManager;
    private readonly List<IConcernBuilder> _concerns = new();

    public LiquidHandlerBuilder(ILiquidResponseMiddleware middleware, ILiquidRoutesManager routesManager)
    {
        _middleware = middleware;
        _routesManager = routesManager;
    }

    public LiquidHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var layout = Layout.Create();

        foreach (var route in _routesManager.LiquidRoutes)
        {
            if (string.IsNullOrWhiteSpace(route.RouteTemplate))
            {
                continue;
            }

            var cleanedTemplate = route.RouteTemplate.Trim('/');
            if (string.IsNullOrWhiteSpace(cleanedTemplate))
            {
                layout.Index(new LiquidRouteContentHandlerBuilder(_middleware, route));
                continue;
            }

            var segments = cleanedTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries);
            layout.Add(segments, new LiquidRouteContentHandlerBuilder(_middleware, route));
        }

        layout.Add(new LiquidContentHandler(_middleware));
        return Concerns.Chain(_concerns, layout.Build());
    }
}
