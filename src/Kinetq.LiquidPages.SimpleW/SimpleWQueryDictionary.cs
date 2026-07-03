using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.SimpleW;

public class SimpleWQueryDictionary : IReadOnlyQueryDictionary
{
    private readonly IDictionary<string, string> _queryDictionary;

    public SimpleWQueryDictionary(IDictionary<string, string> queryDictionary)
    {
        _queryDictionary = queryDictionary;
    }

    public string? this[string key] => _queryDictionary[key];

    public bool TryGetValue(string key, out string? value)
    {
        return _queryDictionary.TryGetValue(key, out value);
    }

    public bool ContainsKey(string key)
    {
        return _queryDictionary.ContainsKey(key);
    }
}