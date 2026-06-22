namespace Kinetq.LiquidPages.Builders;

public abstract class LiquidResponseBuilder<T>
{
    public T Response { get; set; }
    public abstract void Initialize(T response, TextWriter bodyWriter);
    public TextWriter BodyWriter { get; set; }
    public abstract void SetStatusCode(int statusCode, string? message = null);
    public abstract void SetContentType(string contentType);
}