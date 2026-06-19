using Kinetq.LiquidPages.Interfaces;
using SimpleW;

namespace Kinetq.LiquidPages.SimpleW;

public class SimpleWHeaderDictionary : IReadOnlyHeaderDictionary
{
    private HttpHeaders _headers;

    public SimpleWHeaderDictionary(HttpHeaders headers) => _headers = headers;

    public string? this[string key] => _headers.TryGetValue(key, out var values) ? values : null;

    public bool TryGetValue(string key, out string? value)
    {
        if (_headers.TryGetValue(key, out var values))
        {
            value = values;
            return true;
        }
        value = null;
        return false;
    }

    public IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        foreach (var kv in _headers.EnumerateAll())
        {
            yield return kv;
        }
    }
}