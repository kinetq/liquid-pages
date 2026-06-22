using Fluid;

namespace Kinetq.LiquidPages.Models;

public class LiquidRoute
{
    public string RouteTemplate { get; set; }
    public string LiquidTemplatePath { get; set; }
    public Func<LiquidRequestModel, Task<object>> Execute { get; set; }
    public Type? PageModelType { get; set; }
    public TemplateOptions? TemplateOptions { get; set; }
}