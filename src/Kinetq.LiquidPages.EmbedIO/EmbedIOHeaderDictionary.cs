using System.Collections.Specialized;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.EmbedIO;

public class EmbedIOHeaderDictionary : IReadOnlyHeaderDictionary
{
    private readonly NameValueCollection _headers;

    public EmbedIOHeaderDictionary(NameValueCollection headers) => _headers = headers;

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

    public IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        foreach (var key in _headers.AllKeys)
        {
            yield return new KeyValuePair<string, string>(key, _headers[key]);
        }
    }
}