using System.Collections.Specialized;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Models;

public class LiquidRequestModel
{
    public string Route { get; set; }
    public IDictionary<string, string> QueryParams { get; set; }
    public object? Body { get; set; }
    public NameValueCollection Headers { get; set; }
    public string Method { get; set; } = "GET";
    public LiquidRoute? LiquidRoute { get; set; }
    public LiquidPageModel? LiquidPageModel { get; set; }
}