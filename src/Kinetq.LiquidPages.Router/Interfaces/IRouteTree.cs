using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Router.Models;

namespace Kinetq.LiquidPages.Router.Interfaces;

public interface IRouteTree
{
    void AddRoute(LiquidRoute liquidRoute);
    RouteMatch? Match(string path);
    void Initialize();
}