using Kinetq.LiquidPages.Maui.Models;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Maui.Interfaces;

public interface IRouteTree
{
    void AddRoute(LiquidRoute liquidRoute);
    RouteMatch? Match(string path);
    void Initialize();
}