namespace Kinetq.LiquidPages.Builders;

public abstract class LiquidResponseBuilder<T>(T response, TextWriter bodyWriter)
{
    protected readonly T Response = response;
    public TextWriter BodyWriter { get; } = bodyWriter;
    public abstract void SetStatusCode(int statusCode, string? message = null);
    public abstract void SetContentType(string contentType);
    public abstract void AddHeader(string key, string value);
}