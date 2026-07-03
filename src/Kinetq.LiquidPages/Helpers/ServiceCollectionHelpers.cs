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

        IEnumerable<Type> liquidPageModels = assembliesToScan
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(LiquidPageModel)));

        foreach (Type type in liquidPageModels)
        {
            serviceCollection.AddTransient(typeof(LiquidPageModel), type);
            serviceCollection.AddTransient(type);
        }

        //Type liquidResponseBuilderType = assembliesToScan
        //    .SelectMany(a => a.GetTypes())
        //    .Single(t => t.IsClass && !t.IsAbstract && t.GetInterface(nameof(ILiquidResponseBuilder)) != null);

        return serviceCollection;
    }
}