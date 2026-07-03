namespace Kinetq.LiquidPages.GenHTTP;

public class GenHTTPLiquidResponse
{
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
    public string ContentType { get; set; } = "text/html";
    public int StatusCode { get; set; } = 200;
    public string? StatusDescription { get; set; } = "OK";
}