using Kinetq.LiquidPages.Interfaces;
using System.Collections.Specialized;
using EmbedIO.Utilities;

namespace Kinetq.LiquidPages.EmbedIO;

public class EmbedIOLiquidQueryDictionary : IReadOnlyQueryDictionary
{
    private readonly NameValueCollection _headers;

    public EmbedIOLiquidQueryDictionary(NameValueCollection headers) => _headers = headers;

    public string? this[string key] => TryGetValue(key, out var values) ? values : null;

    public bool TryGetValue(string key, out string? value)
    {
        value = _headers.Get(key);
        if (!string.IsNullOrEmpty(value))
        {
            return true;
        }
        value = null;
        return false;
    }

    public bool ContainsKey(string key)
    {
        return _headers.ContainsKey(key);
    }
}