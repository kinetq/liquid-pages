using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Models;

public class LiquidRoute
{
    public Regex RoutePattern { get; set; }
    public string LiquidTemplatePath { get; set; }
    public IFileProvider FileProvider { get; set; }
    public Func<LiquidRequestModel, Task<object>> Execute { get; set; }
    public IDictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
    public Type? PageModelType { get; set; }
}