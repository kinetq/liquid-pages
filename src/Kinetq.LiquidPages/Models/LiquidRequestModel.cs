using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Models;

public sealed class LiquidRequestModel
{
    public string Route { get; init; }
    public IReadOnlyQueryDictionary? QueryParams { get; init; }
    public IReadOnlyRouteValuesDictionary RouteValues { get; init; } = EmptyRouteValuesDictionary.Instance;
    public object? Body { get; set; }
    public IReadOnlyHeaderDictionary? Headers { get; init; }
    public string Method { get; init; } = "GET";
    public int? ErrorStatusCode { get; init; }
    public LiquidRoute? LiquidRoute { get; init; }
    public LiquidPageModel? LiquidPageModel { get; set; }
}