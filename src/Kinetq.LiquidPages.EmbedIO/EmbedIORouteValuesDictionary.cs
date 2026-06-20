using System.Collections;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.EmbedIO;

public sealed class EmbedIORouteValuesDictionary : IReadOnlyRouteValuesDictionary
{
    private readonly IEnumerable<KeyValuePair<string, string>> _routeValues;

    public EmbedIORouteValuesDictionary(IEnumerable<KeyValuePair<string, string>> routeValues) => _routeValues = routeValues;

    public object? this[string key] => TryGetValue(key, out var value) ? value : null;

    public bool ContainsKey(string key) => TryGetValue(key, out _);

    public bool TryGetValue(string key, out object? value)
    {
        foreach (var pair in _routeValues)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        foreach (var pair in _routeValues)
        {
            yield return new KeyValuePair<string, object?>(pair.Key, pair.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
