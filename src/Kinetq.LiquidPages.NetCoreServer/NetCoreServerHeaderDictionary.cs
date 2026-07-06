using Kinetq.LiquidPages.Interfaces;
using NetCoreServer;

namespace Kinetq.LiquidPages.NetCoreServer;

public class NetCoreServerHeaderDictionary : IReadOnlyHeaderDictionary
{
    private readonly HttpRequest _httpRequest;

    public NetCoreServerHeaderDictionary(HttpRequest httpRequest)
    {
        _httpRequest = httpRequest;
    }

    public string? this[string key] => throw new NotImplementedException();

    public bool TryGetValue(string key, out string? value)
    {
        for (var i = 0; i < _httpRequest.Headers; i++)
        {
            var header = _httpRequest.Header(i);
            if (!header.Item1.Equals(key)) continue;
            
            value = header.Item2;
            return true;
        }

        value = null;
        return false;
    }

    public IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        for (var i = 0; i < _httpRequest.Headers; i++)
        {
            var header = _httpRequest.Header(i);
            yield return new KeyValuePair<string, string>(header.Item1, header.Item2);
        }
    }
}