using Microsoft.Extensions.DependencyInjection;
using Kinetq.LiquidPages.Router.Interfaces;
using Kinetq.LiquidPages.Router.Models;

namespace Kinetq.LiquidPages.Router.Helpers;

public static class ServiceCollectionHelpers
{
    public static IServiceCollection AddLiquidRouter(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IRouteTree, RouteTree>();
        return serviceCollection;
    }
}