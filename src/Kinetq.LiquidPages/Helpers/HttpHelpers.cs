using System.Net;
using System.Net.Sockets;

namespace Kinetq.LiquidPages.Helpers;

public static class HttpHelpers
{
    public static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public static IDictionary<string, string> GetQueryParams(this string queryString)
    {
        var queryParams = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(queryString))
        {
            var queryPairs = queryString.TrimStart('?').Split('&');

            foreach (var pair in queryPairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = WebUtility.UrlDecode(keyValue[0]);
                    var value = WebUtility.UrlDecode(keyValue[1]);

                    if (queryParams.TryGetValue(key, out var existingValue))
                    {
                        queryParams[key] = $"{existingValue},{value}";
                    }
                    else
                    {
                        queryParams[key] = value;
                    }
                }
            }
        }

        return queryParams;
    }

    public static IDictionary<string, string> ParseMultipartFormData(this string body)
    {
        var formData = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(body))
            return formData;

        var pairs = body.Split('&');

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                formData[key] = value;
            }
        }

        return formData;
    }
}