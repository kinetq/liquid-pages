using GenHTTP.Api.Content;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.GenHTTP;

public sealed class LiquidHandlerBuilder : IHandlerBuilder<LiquidHandlerBuilder>
{
    private readonly ILiquidResponseMiddleware _middleware;
    private readonly List<IConcernBuilder> _concerns = new();

    public LiquidHandlerBuilder(ILiquidResponseMiddleware middleware)
    {
        _middleware = middleware;
    }

    public LiquidHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new LiquidContentHandler(_middleware));
    }
}
