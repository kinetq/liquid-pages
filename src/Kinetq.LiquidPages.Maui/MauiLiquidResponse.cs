namespace Kinetq.LiquidPages.Maui;

public sealed class MauiLiquidResponse
{
    public int StatusCode { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public string? StatusCodeDescription { get; set; }
}
