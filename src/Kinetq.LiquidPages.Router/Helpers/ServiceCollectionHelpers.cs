using Kinetq.LiquidPages.Maui.Interfaces;
using Kinetq.LiquidPages.Maui.Models;
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