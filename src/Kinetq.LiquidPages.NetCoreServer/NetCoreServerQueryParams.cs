using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.NetCoreServer;

public class NetCoreServerQueryParams : IReadOnlyQueryDictionary
{
    private readonly IDictionary<string, string> _queryParams;

    public NetCoreServerQueryParams(IDictionary<string, string> queryParams)
    {
        _queryParams = queryParams;
    }

    public string? this[string key] => _queryParams[key];

    public bool TryGetValue(string key, out string? value)
    {
        return _queryParams.TryGetValue(key, out value);
    }

    public bool ContainsKey(string key)
    {
        return _queryParams.ContainsKey(key);
    }
}