namespace Kinetq.LiquidPages.Models;

public class LiquidPagesOptions
{
    public IList<PageRoute> PageRoutes = new List<PageRoute>();
}

public class PageRoute
{
    public string RouteTemplate { get; set; }
    public Type PageType { get; set; }
}