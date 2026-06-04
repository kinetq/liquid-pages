using Fluid;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Managers;
using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Statiq.App;
using System.Reflection;

namespace Kinetq.LiquidPages.Statiq;

public static class BootstrapperExtensions
{
    /// <summary>
    /// Registers the LiquidPages core services (parser, filter manager, registered-types manager)
    /// into the Statiq engine's DI container so that <see cref="Modules.RenderLiquidTemplate"/>
    /// can resolve them at execution time.
    /// </summary>
    public static Bootstrapper AddLiquidPages(this Bootstrapper bootstrapper) =>
        bootstrapper.ConfigureServices(services =>
        {
            services.TryAddSingleton<IFluidParserManager, FluidParserManager>();
            services.TryAddSingleton<ILiquidFilterManager, LiquidFilterManager>();
            services.TryAddSingleton<ILiquidRegisteredTypesManager, LiquidRegisteredTypesManager>();
        });

    /// <summary>
    /// Registers the LiquidPages core services and adds a custom Liquid filter that will be
    /// available to all <see cref="Modules.RenderLiquidTemplate"/> module invocations.
    /// </summary>
    public static Bootstrapper AddLiquidFilter(
        this Bootstrapper bootstrapper,
        string name,
        FilterDelegate filterDelegate) =>
        bootstrapper
            .AddLiquidPages()
            .ConfigureServices(services =>
                services.AddSingleton<IConfigureLiquidFilter>(
                    new ConfigureLiquidFilter(name, filterDelegate)));

    /// <summary>
    /// Registers the LiquidPages core services and registers a type with the
    /// <see cref="ILiquidRegisteredTypesManager"/> so its members are accessible in templates.
    /// </summary>
    public static Bootstrapper AddLiquidType<T>(this Bootstrapper bootstrapper) =>
        bootstrapper
            .AddLiquidPages()
            .ConfigureServices(services =>
                services.AddSingleton<IConfigureLiquidType>(new ConfigureLiquidType(typeof(T))));

    /// <summary>
    /// Scans <paramref name="assemblies"/> for all concrete <see cref="LiquidPageModel"/> subclasses,
    /// registers each one as a transient service, and adds an <see cref="IConfigureLiquidPageModel"/>
    /// descriptor so that <see cref="Modules.ExecutePageModel"/> can resolve and execute them.
    /// </summary>
    public static Bootstrapper AddLiquidPageModels(
        this Bootstrapper bootstrapper,
        params Assembly[] assemblies) =>
        bootstrapper
            .AddLiquidPages()
            .ConfigureServices(services =>
            {
                var pageModelTypes = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(LiquidPageModel)));

                foreach (var type in pageModelTypes)
                {
                    services.AddTransient(type);
                    services.AddSingleton<IConfigureLiquidPageModel>(new ConfigureLiquidPageModel(type));
                }
            });
}
