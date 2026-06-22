using System.Reflection;
using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages;

public class LiquidStartup : ILiquidStartup
{
    private readonly ILiquidRoutesManager _liquidRoutesManager;
    private readonly ILiquidFilterManager _liquidFilterManager;
    private readonly ILiquidRegisteredTypesManager _liquidRegisteredTypesManager;
    private readonly ITemplateOptionsManager _templateOptionsManager;
    private readonly IEnumerable<ILiquidFilter> _liquidFilters;
    private readonly IEnumerable<LiquidPageModel> _liquidPageModels;

    public LiquidStartup(
        ILiquidRoutesManager liquidRoutesManager,
        IEnumerable<ILiquidFilter> liquidFilters,
        ILiquidFilterManager liquidFilterManager,
        IEnumerable<LiquidPageModel> liquidPageModels,
        ILiquidRegisteredTypesManager liquidRegisteredTypesManager, 
        ITemplateOptionsManager templateOptionsManager)
    {
        _liquidRoutesManager = liquidRoutesManager;
        _liquidFilters = liquidFilters;
        _liquidFilterManager = liquidFilterManager;
        _liquidPageModels = liquidPageModels;
        _liquidRegisteredTypesManager = liquidRegisteredTypesManager;
        _templateOptionsManager = templateOptionsManager;
    }

    public void RegisterFileProvider(string prefix, IFileProvider fileProvider)
    {
        if (!_liquidRegisteredTypesManager.RegisteredTypes.Any())
        {
            throw new Exception(
                "There are no registered types! Call RegisterPageModels before registering the file provider.");
        }
        
        _templateOptionsManager.RegisterTemplateOptions(prefix, fileProvider);
    }

    public void RegisterFilters()
    {
        foreach (var liquidFilter in _liquidFilters)
        {
            var filter = liquidFilter.GetFilter();
            _liquidFilterManager.RegisterFilter(filter.Name, filter.FilterDelegate);
        }
    }

    public void RegisterPageModels(Action<LiquidPagesOptionsBuilder> buildOptionsAction)
    {
        var optionsBuilder = new LiquidPagesOptionsBuilder();
        buildOptionsAction(optionsBuilder);

        var options = optionsBuilder.Build();
        foreach (var optionsPageRoute in options.PageRoutes)
        {
            var liquidPageModel = _liquidPageModels.FirstOrDefault(x => x.GetType() == optionsPageRoute.PageType);
            if (liquidPageModel == null)
            {
                continue;
            }

            var liquidPageAttribute = liquidPageModel.GetType().GetCustomAttribute<LiquidPageAttribute>();
            if (liquidPageAttribute == null)
            {
                continue;
            }

            var pageModelType = liquidPageModel.GetType();
            _liquidRoutesManager.RegisterRoute(new LiquidRoute
            {
                RouteTemplate = optionsPageRoute.RouteTemplate,
                LiquidTemplatePath = liquidPageAttribute.TemplatePath,
                PageModelType = pageModelType,
                Execute = async (request) =>
                {
                    if (request.Method == "POST")
                        await request.LiquidPageModel!.OnPostAsync(request);
                    else
                        await request.LiquidPageModel!.OnGetAsync(request);

                    return request.LiquidPageModel;
                }
            });
        }

        RegisterPageModels();
    }

    public void RegisterPageModels()
    {
        foreach (var liquidPageModel in _liquidPageModels)
        {
            var pageModelType = liquidPageModel.GetType();
            var liquidPageAttribute = liquidPageModel.GetType().GetCustomAttribute<LiquidPageAttribute>();
            if (liquidPageAttribute != null && !string.IsNullOrEmpty(liquidPageAttribute.RouteTemplate))
            {
                _liquidRoutesManager.RegisterRoute(new LiquidRoute
                {
                    RouteTemplate = liquidPageAttribute.RouteTemplate,
                    LiquidTemplatePath = liquidPageAttribute.TemplatePath,
                    PageModelType = pageModelType,
                    Execute = async (request) =>
                    {
                        if (request.Method == "POST")
                            await request.LiquidPageModel!.OnPostAsync(request);
                        else
                            await request.LiquidPageModel!.OnGetAsync(request);

                        return request.LiquidPageModel;
                    }
                });
            }

            var liquidErrorPageAttribute =
                liquidPageModel.GetType().GetCustomAttribute<LiquidErrorPageAttribute>();
            if (liquidErrorPageAttribute != null)
            {
                _liquidRoutesManager.RegisterErrorRoute((int)liquidErrorPageAttribute.StatusCode, new LiquidRoute
                {
                    LiquidTemplatePath = liquidErrorPageAttribute.TemplatePath,
                    PageModelType = pageModelType,
                    Execute = async (request) =>
                    {
                        if (request.Method == "POST")
                            await request.LiquidPageModel!.OnPostAsync(request);
                        else
                            await request.LiquidPageModel!.OnGetAsync(request);

                        return request.LiquidPageModel;
                    }
                });
            }

            _liquidRegisteredTypesManager.RegisterType(liquidPageModel.GetType());
            var derivedType = liquidPageModel.GetType();
            var baseType = typeof(LiquidPageModel);
            var baseProperties = baseType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var derivedProperties = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var additionalProperties = derivedProperties.Where(p => !baseProperties.Any(bp => bp.Name == p.Name));

            var processedTypes = new HashSet<Type>();
            foreach (var property in additionalProperties)
            {
                RegisterTypeRecursively(property.PropertyType, processedTypes);
            }
        }
    }

    private void RegisterTypeRecursively(Type type, HashSet<Type> processedTypes)
    {
        if (processedTypes.Contains(type))
            return;

        processedTypes.Add(type);

        // Skip primitive types, strings, and other basic value types
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) || type == typeof(Guid) || type.IsEnum)
        {
            return;
        }

        // Handle enumerable types - recurse into element types but don't register the collection itself
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                foreach (var arg in genericArgs)
                {
                    RegisterTypeRecursively(arg, processedTypes);
                }
            }
            else if (type.IsArray)
            {
                RegisterTypeRecursively(type.GetElementType()!, processedTypes);
            }
            return;
        }

        // Register complex types only
        _liquidRegisteredTypesManager.RegisterType(type);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            RegisterTypeRecursively(property.PropertyType, processedTypes);
        }
    }
}