using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface IRouteTree
{
    void AddRoute(LiquidRoute liquidRoute);
    RouteMatch? Match(string path);
    void Initialize();
}