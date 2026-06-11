using System.Net;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidRoutesManager
{
    void RegisterRoute(LiquidRoute route);
    void RegisterErrorRoute(int statusCode, LiquidRoute route);
    IList<LiquidRoute> LiquidRoutes { get; }
    IDictionary<int, LiquidRoute> ErrorRoutes { get; }
    LiquidRoute? GetRouteForStatusCode(HttpStatusCode statusCode);
}