using Microsoft.AspNetCore.Routing;

namespace Kinetq.LiquidPages.Models;

internal class LiquidRoute
{
    public string RouteTemplate { get; set; }
    public string LiquidTemplatePath { get; set; }
    public Func<LiquidRequestModel, Task<object>> Execute { get; set; }
    public IDictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
    public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();
    public Type? PageModelType { get; set; }
}