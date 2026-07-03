using System.Reflection;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Managers;
using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetq.LiquidPages.Helpers;

public static class ServiceCollectionHelpers
{
    public static IServiceCollection AddLiquidPages(
        this IServiceCollection serviceCollection, params Assembly[] assembliesToScan)
    {
        serviceCollection.AddSingleton<ILiquidFilterManager, LiquidFilterManager>();
        serviceCollection.AddSingleton<ILiquidRegisteredTypesManager, LiquidRegisteredTypesManager>();
        serviceCollection.AddSingleton<ILiquidRoutesManager, LiquidRoutesManager>();
        serviceCollection.AddSingleton<IFluidParserManager, FluidParserManager>();
        serviceCollection.AddSingleton<ILiquidPartialsManager, LiquidPartialsManager>();
        serviceCollection.AddSingleton<ILiquidTemplateManager, LiquidTemplateManager>();
        serviceCollection.AddSingleton<ITemplateOptionsManager, TemplateOptionsManager>();
        serviceCollection.AddScoped<ILiquidResponseMiddleware, LiquidResponseMiddleware>();
        serviceCollection.AddScoped<IHtmlRenderer, HtmlRenderer>();
        serviceCollection.AddScoped<ILiquidStartup, LiquidStartup>();

        var allTypes = assembliesToScan.SelectMany(a => a.GetTypes()).ToList();
        IEnumerable<Type> liquidPageModels = allTypes
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(LiquidPageModel)));

        foreach (Type type in liquidPageModels)
        {
            serviceCollection.AddTransient(typeof(LiquidPageModel), type);
            serviceCollection.AddTransient(type);
        }

        return serviceCollection;
    }
}