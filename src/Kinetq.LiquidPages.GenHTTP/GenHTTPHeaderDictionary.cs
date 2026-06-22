using GenHTTP.Api.Protocol;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.GenHTTP;

public class GenHTTPHeaderDictionary : IReadOnlyHeaderDictionary
{
    private readonly IHeaderCollection _headers;

    public GenHTTPHeaderDictionary(IHeaderCollection headers) => _headers = headers;

    public string? this[string key] => _headers.GetValueOrDefault(key);

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
        foreach (var kv in _headers)
        {
            yield return kv;
        }
    }
}