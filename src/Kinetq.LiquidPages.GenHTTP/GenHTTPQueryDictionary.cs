using GenHTTP.Api.Protocol;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.GenHTTP;

public class GenHTTPQueryDictionary : IReadOnlyQueryDictionary
{
    private readonly IRequestQuery _requestQuery;

    public GenHTTPQueryDictionary(IRequestQuery requestQuery)
    {
        _requestQuery = requestQuery;
    }

    public string? this[string key] => _requestQuery[key];

    public bool TryGetValue(string key, out string? value)
    {
        return _requestQuery.TryGetValue(key, out value);
    }

    public bool ContainsKey(string key)
    {
        return _requestQuery.ContainsKey(key);
    }
}