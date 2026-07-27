using Fluid;

namespace Kinetq.LiquidPages.Models;

public sealed class LiquidRoute
{
    public string RouteTemplate { get; init; }
    public string LiquidTemplatePath { get; init; }
    public Func<LiquidRequestModel, Task<object>> Execute { get; init; }
    public Type? PageModelType { get; init; }
    public TemplateOptions? TemplateOptions { get; set; }
    public bool DisableTemplateCache { get; set; }
}