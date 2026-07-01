using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class ConventionalRouteEntry
{
    public ConventionalRouteEntry(
        string? routeName,
        RoutePattern pattern,
        RouteValueDictionary dataTokens,
        int order,
        IReadOnlyList<Action<EndpointBuilder>> conventions,
        IReadOnlyList<Action<EndpointBuilder>> finallyConventions)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(dataTokens);
        ArgumentNullException.ThrowIfNull(conventions);
        ArgumentNullException.ThrowIfNull(finallyConventions);

        RouteName = routeName;
        Pattern = pattern;
        DataTokens = dataTokens;
        Order = order;
        Conventions = conventions;
        FinallyConventions = finallyConventions;
    }

    public string? RouteName { get; }

    public RoutePattern Pattern { get; }

    public RouteValueDictionary DataTokens { get; }

    public int Order { get; }

    public IReadOnlyList<Action<EndpointBuilder>> Conventions { get; }

    public IReadOnlyList<Action<EndpointBuilder>> FinallyConventions { get; }
}