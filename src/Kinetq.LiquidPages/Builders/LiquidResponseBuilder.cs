using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Builders;

public abstract class LiquidResponseBuilder<T>(T response, TextWriter bodyWriter) : ILiquidResponseBuilder
{
    protected readonly T Response = response;
    public TextWriter BodyWriter { get; } = bodyWriter;

    public abstract void SetStatusCode(int statusCode, string? message = null);
    public abstract void SetContentType(string contentType);
    public abstract void AddHeader(string key, string value);
    public abstract void RemoveHeader(string key);
    public abstract void AddCookie(string key, string value, LiquidCookieOptions? cookieOptions = null);
    public abstract void RemoveCookie(string key);
}