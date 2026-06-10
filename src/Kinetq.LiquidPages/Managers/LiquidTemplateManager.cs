using System.Collections.Concurrent;
using Fluid;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.Managers;

public class LiquidTemplateManager : ILiquidTemplateManager
{
    private readonly Lazy<IDictionary<string, IFluidTemplate>> _fluidTemplates =
        new(() => new ConcurrentDictionary<string, IFluidTemplate>());

    public IDictionary<string, IFluidTemplate> FluidTemplates => _fluidTemplates.Value;

    public void RegisterTemplate(string templateContents, IFluidTemplate fluidTemplate)
    {
        if (!FluidTemplates.TryGetValue(templateContents, out var value))
        {
            _fluidTemplates.Value[templateContents] = fluidTemplate;
        }
    }
}