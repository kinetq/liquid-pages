namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidResponseBuilder
{
    void SetStatusCode(int statusCode, string? message = null);
    void SetContentType(string contentType);
    void AddHeader(string key, string value);
    TextWriter BodyWriter { get; }
}