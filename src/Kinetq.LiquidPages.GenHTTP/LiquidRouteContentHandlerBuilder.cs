using GenHTTP.Api.Content;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.GenHTTP;

public sealed class LiquidRouteContentHandlerBuilder : IHandlerBuilder
{
    private readonly ILiquidResponseMiddleware _middleware;
    private readonly LiquidRoute _liquidRoute;

    public LiquidRouteContentHandlerBuilder(ILiquidResponseMiddleware middleware, LiquidRoute liquidRoute)
    {
        _middleware = middleware;
        _liquidRoute = liquidRoute;
    }

    public IHandler Build()
    {
        return new LiquidContentHandler(_middleware, _liquidRoute);
    }
}
