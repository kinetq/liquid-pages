using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.Models;

public class RouteMatch
{
    public IReadOnlyRouteValuesDictionary? RouteValues { get; set; }
    public LiquidRoute? LiquidRoute { get; set; }
}