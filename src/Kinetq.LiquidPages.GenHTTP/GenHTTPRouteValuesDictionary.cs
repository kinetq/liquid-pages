using System.Collections;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.GenHTTP;

public sealed class GenHTTPRouteValuesDictionary : IReadOnlyRouteValuesDictionary
{
    private readonly IReadOnlyDictionary<string, object?> _routeValues;

    public GenHTTPRouteValuesDictionary(IReadOnlyDictionary<string, object?> routeValues) => _routeValues = routeValues;

    public object? this[string key] => _routeValues.TryGetValue(key, out var value) ? value : null;

    public bool ContainsKey(string key) => _routeValues.ContainsKey(key);

    public bool TryGetValue(string key, out object? value) => _routeValues.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _routeValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
