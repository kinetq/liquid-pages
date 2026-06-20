namespace Kinetq.LiquidPages.Interfaces;

public interface IReadOnlyRouteValuesDictionary : IEnumerable<KeyValuePair<string, object?>>
{
    object? this[string key] { get; }
    bool ContainsKey(string key);
    bool TryGetValue(string key, out object? value);
}
