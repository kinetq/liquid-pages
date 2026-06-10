using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Builders;

public class LiquidPagesOptionsBuilder
{
    private readonly LiquidPagesOptions _liquidPagesOptions = new LiquidPagesOptions();

    public LiquidPagesOptionsBuilder AddPageRoute(Type pageType, string route)
    {
        _liquidPagesOptions.PageRoutes.Add(new PageRoute()
        {
            RouteTemplate = route,
            PageType = pageType
        });
        return this;
    }

     internal LiquidPagesOptions Build()
    {
        return _liquidPagesOptions;
    }
}