using Kinetq.LiquidPages.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Kinetq.LiquidPages.AspNetCore;

public sealed class AspNetCoreQueryParams : IReadOnlyQueryDictionary
{
    private readonly IQueryCollection _queryCollection;

    public AspNetCoreQueryParams(IQueryCollection queryCollection) => _queryCollection = queryCollection;

    public bool TryGetValue(string key, out string value)
    {
        if (_queryCollection.TryGetValue(key, out var values))
        {
            value = values.ToString();
            return true;
        }

        value = default!;
        return false;
    }

    public bool ContainsKey(string key)
    {
        return _queryCollection.ContainsKey(key);
    }

    public string this[string key]
    {
        get => _queryCollection[key].ToString();
        set => throw new NotSupportedException();
    }
}