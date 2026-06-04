using Fluid;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.Statiq;

internal interface IConfigureLiquidFilter
{
    string Name { get; }
    FilterDelegate FilterDelegate { get; }
}

internal interface IConfigureLiquidType
{
    Type Type { get; }
}

internal interface IConfigureLiquidPageModel
{
    Type PageModelType { get; }
}

internal sealed class ConfigureLiquidFilter : IConfigureLiquidFilter
{
    public ConfigureLiquidFilter(string name, FilterDelegate filterDelegate)
    {
        Name = name;
        FilterDelegate = filterDelegate;
    }

    public string Name { get; }
    public FilterDelegate FilterDelegate { get; }
}

internal sealed class ConfigureLiquidType : IConfigureLiquidType
{
    public ConfigureLiquidType(Type type) => Type = type;
    public Type Type { get; }
}

internal sealed class ConfigureLiquidPageModel : IConfigureLiquidPageModel
{
    public ConfigureLiquidPageModel(Type type) => PageModelType = type;
    public Type PageModelType { get; }
}
