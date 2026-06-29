using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Maui.Models;

public class RouteNode
{
    public string Segment { get; set; }               // e.g., "products" or "{id}"
    public bool IsParameter { get; set; }             // True if it's a route param
    public string? ParameterName { get; set; }        // "id" if IsParameter is true
    public LiquidRoute LiquidRoute { get; set; }          // Null if this is not an endpoint
    public List<RouteNode> Children { get; set; } = new();

    public RouteNode(string segment)
    {
        Segment = segment;
        IsParameter = segment.StartsWith('{') && segment.EndsWith('}');
        ParameterName = IsParameter ? segment.Trim('{', '}') : null;
    }
}