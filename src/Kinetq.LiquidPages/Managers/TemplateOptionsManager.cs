using System.Collections.Concurrent;
using Fluid;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Managers;

public class TemplateOptionsManager : ITemplateOptionsManager
{
    private readonly ILiquidRegisteredTypesManager _registeredTypesManager;
    private readonly ILiquidFilterManager _filterManager;

    private readonly Lazy<IDictionary<string, TemplateOptions>> _templateOptionsMap = new(() => new ConcurrentDictionary<string, TemplateOptions>());

    public TemplateOptionsManager(ILiquidRegisteredTypesManager registeredTypesManager, ILiquidFilterManager filterManager)
    {
        _registeredTypesManager = registeredTypesManager;
        _filterManager = filterManager;
    }

    public IDictionary<string, TemplateOptions> TemplateOptionsMap => 
        _templateOptionsMap.Value
            .OrderByDescending(x => x.Key.Length)
            .ToDictionary();

    public void RegisterTemplateOptions(string prefix, IFileProvider fileProvider)
    {
        if (TemplateOptionsMap.TryGetValue(prefix, out var templateOptions))
        {
            return;
        }

        var options = new TemplateOptions
        {
            FileProvider = fileProvider,
            MemberAccessStrategy = new DefaultMemberAccessStrategy
            {
                MemberNameStrategy = MemberNameStrategies.SnakeCase
            }
        };

        foreach (var registeredType in _registeredTypesManager.RegisteredTypes)
        {
            options.MemberAccessStrategy.Register(registeredType);
        }

        foreach (var filterDelegate in _filterManager.LiquidFilters)
        {
            options.Filters.AddFilter(filterDelegate.Key, filterDelegate.Value);
        }
        
        TemplateOptionsMap.Add(prefix, options);
    }

    public TemplateOptions GetTemplateOptions(string path)
    {
        foreach (var templateOptionsMap in TemplateOptionsMap)
        {
            if (path.StartsWith(templateOptionsMap.Key, StringComparison.OrdinalIgnoreCase))
                return templateOptionsMap.Value;
        }
        
        return null;
    }
}