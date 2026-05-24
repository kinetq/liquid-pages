using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Builders;

public class LiquidPagesOptionsBuilder
{
    private readonly LiquidPagesOptions _liquidPagesOptions = new LiquidPagesOptions();
    
    public LiquidPagesOptionsBuilder AddPageRoute(Type pageType, string route)
    {
        _liquidPagesOptions.PageRoutes[pageType] = route;
        return this;
    }

    public LiquidPagesOptions Build()
    {
        return _liquidPagesOptions;
    }
}