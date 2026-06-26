using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Router.Models;

public class RouteMatch
{
    public IReadOnlyRouteValuesDictionary? RouteValues { get; set; }
    public LiquidRoute? LiquidRoute { get; set; }
}