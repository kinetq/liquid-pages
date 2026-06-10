using Fluid;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidTemplateManager
{
    IDictionary<string, IFluidTemplate> FluidTemplates { get; }
    void RegisterTemplate(string templateContents, IFluidTemplate fluidTemplate);
}