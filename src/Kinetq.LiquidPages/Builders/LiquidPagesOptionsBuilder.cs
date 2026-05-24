using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Builders;

public class LiquidPagesOptionsBuilder
{
    private readonly LiquidPagesOptions _liquidPagesOptions = new LiquidPagesOptions();

    public LiquidPagesOptionsBuilder AddPageRoute(Type pageType, string route)
    {
        _liquidPagesOptions.PageRoutes.Add(new PageRoute()
        {
            Route = route,
            PageType = pageType
        });
        return this;
    }

    public LiquidPagesOptions Build()
    {
        return _liquidPagesOptions;
    }
}