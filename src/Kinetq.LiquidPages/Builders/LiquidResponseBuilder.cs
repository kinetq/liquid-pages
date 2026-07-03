namespace Kinetq.LiquidPages.Builders;

public abstract class LiquidResponseBuilder<T>(T response, TextWriter bodyWriter)
{
    protected T Response = response;

    public readonly TextWriter BodyWriter = bodyWriter;
    public abstract void SetStatusCode(int statusCode, string? message = null);
    public abstract void SetContentType(string contentType);
    public abstract void AddHeader(string key, string value);
}