using Kinetq.LiquidPages.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Kinetq.LiquidPages.AspNetCore;

public class AspNetCoreHeaderDictionary : IReadOnlyHeaderDictionary
{
    private readonly IHeaderDictionary _headers;

    public AspNetCoreHeaderDictionary(IHeaderDictionary headers) => _headers = headers;

    public string? this[string key] => _headers.TryGetValue(key, out var values) ? values.ToString() : null;

    public bool TryGetValue(string key, out string? value)
    {
        if (_headers.TryGetValue(key, out var values))
        {
            value = values.ToString();
            return true;
        }
        value = null;
        return false;
    }

    public IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        foreach (var kv in _headers)
        {
            yield return new KeyValuePair<string, string>(kv.Key, kv.Value.ToString());
        }
    }
}