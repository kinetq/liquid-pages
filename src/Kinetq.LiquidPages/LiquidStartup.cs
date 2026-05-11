using System.Reflection;
using System.Text.RegularExpressions;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages;

public class LiquidStartup : ILiquidStartup
{
    private readonly ILiquidRoutesManager _liquidRoutesManager;
    private readonly ILiquidFilterManager _liquidFilterManager;
    private readonly ILiquidRegisteredTypesManager _liquidRegisteredTypesManager;
    private readonly IEnumerable<ILiquidRoute> _liquidRoutes;
    private readonly IEnumerable<ILiquidFilter> _liquidFilters;
    private readonly IEnumerable<ILiquidErrorRoute> _liquidErrorRoutes;
    private readonly IEnumerable<LiquidPageModel> _liquidPageModels;

    public LiquidStartup(
        ILiquidRoutesManager liquidRoutesManager,
        IEnumerable<ILiquidRoute> liquidRoutes,
        IEnumerable<ILiquidFilter> liquidFilters,
        ILiquidFilterManager liquidFilterManager, 
        IEnumerable<ILiquidErrorRoute> liquidErrorRoutes, 
        IEnumerable<LiquidPageModel> liquidPageModels, ILiquidRegisteredTypesManager liquidRegisteredTypesManager)
    {
        _liquidRoutesManager = liquidRoutesManager;
        _liquidRoutes = liquidRoutes;
        _liquidFilters = liquidFilters;
        _liquidFilterManager = liquidFilterManager;
        _liquidErrorRoutes = liquidErrorRoutes;
        _liquidPageModels = liquidPageModels;
        _liquidRegisteredTypesManager = liquidRegisteredTypesManager;
    }

    public async Task RegisterRoutes()
    {
        foreach (var route in _liquidRoutes)
        {
            _liquidRoutesManager.RegisterRoute(await route.GetRoute());
        }

        foreach (var liquidErrorRoute in _liquidErrorRoutes)
        {
            var route = await liquidErrorRoute.GetRoute();
            _liquidRoutesManager.RegisterErrorRoute(liquidErrorRoute.StatusCode, route);
        }
    }

    public async Task RegisterFilters()
    {
        foreach (var liquidFilter in _liquidFilters)
        {
            var filter = await liquidFilter.GetFilter();
            _liquidFilterManager.RegisterFilter(filter.Name, filter.FilterDelegate);
        }
    }

    public async Task RegisterPageModels()
    {
        foreach (var liquidPageModel in _liquidPageModels)
        {
            var attr = liquidPageModel.GetType().GetCustomAttribute<LiquidPageAttribute>()!;

            _liquidRoutesManager.RegisterRoute(new LiquidRoute
            {
                RoutePattern = new Regex(attr.RoutePattern),
                LiquidTemplatePath = attr.TemplatePath,
                FileProvider = liquidPageModel.GetFileProvider(),
                Execute = async (request) =>
                {
                    if (request.Method == "POST")
                        await liquidPageModel.OnPostAsync(request);
                    else
                        await liquidPageModel.OnGetAsync(request);

                    return liquidPageModel;
                }
            });

            var derivedType = liquidPageModel.GetType();
            var baseType = typeof(LiquidPageModel);
            var baseProperties = baseType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var derivedProperties = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            var additionalProperties = derivedProperties.Where(p => !baseProperties.Any(bp => bp.Name == p.Name));
            
            foreach (var property in additionalProperties)
            {
                _liquidRegisteredTypesManager.RegisterType(property.PropertyType);
            }
        }
    }
}