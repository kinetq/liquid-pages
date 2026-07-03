namespace Kinetq.LiquidPages.Maui;

public sealed class MauiLiquidResponse
{
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

    public IDictionary<string, string> Cookies { get; } = new Dictionary<string, string>();

    public int StatusCode { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public string? StatusCodeDescription { get; set; }
}
