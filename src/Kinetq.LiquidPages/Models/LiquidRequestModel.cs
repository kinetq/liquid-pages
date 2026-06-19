using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Models;

public class LiquidRequestModel
{
    public string Route { get; set; }
    public IDictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, object?> RouteValues { get; set; } = new Dictionary<string, object?>();
    public object? Body { get; set; }
    public IReadOnlyHeaderDictionary? Headers { get; set; }
    public string Method { get; set; } = "GET";
    public int? ErrorStatusCode { get; set; }
    public LiquidRoute? LiquidRoute { get; set; }
    public LiquidPageModel? LiquidPageModel { get; set; }
}