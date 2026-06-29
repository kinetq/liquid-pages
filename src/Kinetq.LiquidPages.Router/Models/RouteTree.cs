using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Maui.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Maui.Models;

public class RouteTree : IRouteTree
{
    private readonly ILiquidRoutesManager _liquidRoutesManager;
    private readonly Lazy<RouteNode> _rootRouteNode = new(() => new RouteNode(""));

    public RouteTree(ILiquidRoutesManager liquidRoutesManager)
    {
        _liquidRoutesManager = liquidRoutesManager;
    }

    private RouteNode Root => _rootRouteNode.Value;
    
    public void Initialize()
    {
        foreach (var liquidRoute in _liquidRoutesManager.LiquidRoutes)
        {
            AddRoute(liquidRoute);
        }
    }

    public void AddRoute(LiquidRoute liquidRoute)
    {
        // Split the path, ignoring empty entries (e.g., "/" -> empty, "/products/5" -> ["products", "5"])
        var segments = liquidRoute.RouteTemplate.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentNode = Root;

        foreach (var segment in segments)
        {
            // Check if a child with this exact segment already exists
            var child = currentNode.Children.FirstOrDefault(c => c.Segment == segment);
            if (child == null)
            {
                child = new RouteNode(segment);
                currentNode.Children.Add(child);
            }
            currentNode = child;
        }

        // Assign the Page Model Type to the final node
        currentNode.LiquidRoute = liquidRoute;
    }

    public RouteMatch? Match(string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var parameters = new Dictionary<string, object?>();
        var currentNode = Root;

        foreach (var segment in segments)
        {
            var matchedNode =
                // 1. PRIORITY: Try to match a static segment first (exact match)
                currentNode.Children.FirstOrDefault(c => !c.IsParameter && c.Segment == segment);

            // 2. FALLBACK: Match a dynamic segment (e.g., "{id}")
            if (matchedNode == null)
            {
                matchedNode = currentNode.Children.FirstOrDefault(c => c.IsParameter);
                if (matchedNode is { ParameterName: not null })
                {
                    parameters[matchedNode.ParameterName] = segment;
                }
            }

            if (matchedNode == null)
            {
                return null;
            }

            currentNode = matchedNode;
        }

        // Return the Page Model Type from the final node (or null if this path is intermediate)
        return new RouteMatch
        {
            LiquidRoute = currentNode.LiquidRoute,
            RouteValues = new LiquidRouteValuesDictionary(parameters)
        };
    }
}