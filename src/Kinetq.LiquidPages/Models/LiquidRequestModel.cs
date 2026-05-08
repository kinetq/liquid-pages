using System.Collections.Specialized;

namespace Kinetq.LiquidPages.Models;

public class LiquidRequestModel
{
    public string Route { get; set; }
    public IDictionary<string, string> QueryParams { get; set; }
    public object? Body { get; set; }
    public NameValueCollection Headers { get; set; }
    public string Method { get; set; } = "GET";
}