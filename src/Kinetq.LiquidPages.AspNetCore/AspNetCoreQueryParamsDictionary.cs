using System.Collections;
using Microsoft.AspNetCore.Http;

namespace Kinetq.LiquidPages.AspNetCore;

public sealed class AspNetCoreQueryParams : IDictionary<string, string>
{
    private readonly IQueryCollection _queryCollection;

    public AspNetCoreQueryParams(IQueryCollection queryCollection) => _queryCollection = queryCollection;

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var pair in _queryCollection)
        {
            yield return new KeyValuePair<string, string>(pair.Key, pair.Value.ToString());
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _queryCollection.Count;
    public bool IsReadOnly => true;

    public void Add(string key, string value) => throw new NotSupportedException();

    public bool Remove(string key) => throw new NotSupportedException();

    public bool ContainsKey(string key)
    {
        return _queryCollection.ContainsKey(key);
    }

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

    public string this[string key]
    {
        get => _queryCollection[key].ToString();
        set => throw new NotSupportedException();
    }

    public ICollection<string> Keys => _queryCollection.Keys.ToList();
    public ICollection<string> Values => _queryCollection.Select(pair => pair.Value.ToString()).ToList();

    public void Add(KeyValuePair<string, string> item) => throw new NotSupportedException();

    public void Clear() => throw new NotSupportedException();

    public bool Contains(KeyValuePair<string, string> item)
    {
        return TryGetValue(item.Key, out var value) && value == item.Value;
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        foreach (var pair in this)
        {
            array[arrayIndex++] = pair;
        }
    }

    public bool Remove(KeyValuePair<string, string> item) => throw new NotSupportedException();
}