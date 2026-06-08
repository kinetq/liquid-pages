using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Models;

public class LiquidRoute
{
    public string RouteTemplate { get; set; }
    public string LiquidTemplatePath { get; set; }
    public IFileProvider FileProvider { get; set; }
    public Func<LiquidRequestModel, Task<object>> Execute { get; set; }
    public IDictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
    public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();
    public Type? PageModelType { get; set; }
}