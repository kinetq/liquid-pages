using System.Collections;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace Kinetq.LiquidPages.AspNetCore;

public sealed class AspNetCoreRouteValuesDictionary : IReadOnlyRouteValuesDictionary
{
    private readonly RouteValueDictionary _routeValues;

    public AspNetCoreRouteValuesDictionary(RouteValueDictionary routeValues) => _routeValues = routeValues;

    public object? this[string key] => _routeValues.TryGetValue(key, out var value) ? value : null;

    public bool ContainsKey(string key) => _routeValues.ContainsKey(key);

    public bool TryGetValue(string key, out object? value) => _routeValues.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _routeValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
