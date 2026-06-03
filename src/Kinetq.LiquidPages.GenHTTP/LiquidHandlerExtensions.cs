using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.GenHTTP;

public static class LiquidHandlerExtensions
{
    public static LiquidHandlerBuilder LiquidPages(this ILiquidResponseMiddleware middleware)
        => new LiquidHandlerBuilder(middleware);
}
