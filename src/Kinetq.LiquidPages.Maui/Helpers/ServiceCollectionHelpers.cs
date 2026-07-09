using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetq.LiquidPages.Maui.Helpers;

public static class ServiceCollectionHelpers
{
    public static IServiceCollection AddLiquidRouter(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IRouteTree, RouteTree>();
        return serviceCollection;
    }
}