using System.Collections.Specialized;
using Kinetq.LiquidPages.Pages;
using Microsoft.AspNetCore.Routing;

namespace Kinetq.LiquidPages.Models;

public class LiquidRequestModel
{
    public string Route { get; set; }
    public IDictionary<string, string> QueryParams { get; set; }
    public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();
    public object? Body { get; set; }
    public NameValueCollection Headers { get; set; }
    public string Method { get; set; } = "GET";
    public int? ErrorStatusCode { get; set; }
    public LiquidRoute? LiquidRoute { get; set; }
    public LiquidPageModel? LiquidPageModel { get; set; }
}