using System.Collections;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.Models;

public sealed class EmptyRouteValuesDictionary : IReadOnlyRouteValuesDictionary
{
    public static EmptyRouteValuesDictionary Instance { get; } = new();

    private EmptyRouteValuesDictionary() { }

    public object? this[string key] => null;

    public bool ContainsKey(string key) => false;

    public bool TryGetValue(string key, out object? value)
    {
        value = null;
        return false;
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
