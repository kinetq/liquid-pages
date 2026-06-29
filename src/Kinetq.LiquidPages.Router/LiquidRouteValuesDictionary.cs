using System.Collections;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.Maui;

public class LiquidRouteValuesDictionary : IReadOnlyRouteValuesDictionary
{
    private readonly IDictionary<string, object?> _routeValuesDictionary;

    public LiquidRouteValuesDictionary(IDictionary<string, object?> routeValuesDictionary)
    {
        _routeValuesDictionary = routeValuesDictionary;
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        foreach (var pair in _routeValuesDictionary)
        {
            yield return new KeyValuePair<string, object?>(pair.Key, pair.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public object? this[string key] => _routeValuesDictionary[key];

    public bool ContainsKey(string key)
    {
        return _routeValuesDictionary.ContainsKey(key);
    }

    public bool TryGetValue(string key, out object? value)
    {
        return _routeValuesDictionary.TryGetValue(key, out value);
    }
}