using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Models;
using NetCoreServer;

namespace Kinetq.LiquidPages.NetCoreServer;

public class NetCoreServerResponseBuilder(HttpResponse response, TextWriter? bodyWriter)
    : LiquidResponseBuilder<HttpResponse>(response, bodyWriter)
{
    public override void SetStatusCode(int statusCode, string? message = null)
    {
        if (message is null)
            Response.SetBegin(statusCode); // uses default protocol + default phrase
        else
            Response.SetBegin(statusCode, message, "HTTP/1.1");
    }

    public override void SetContentType(string contentType)
    {
        Response.SetHeader("Content-Type", contentType);
    }

    public override void AddHeader(string key, string value)
    {
        Response.SetHeader(key, value);
    }

    public override void RemoveHeader(string key)
    {
        Response.SetHeader(key, string.Empty);
    }

    public override void AddCookie(string key, string value, LiquidCookieOptions? cookieOptions = null)
    {
        Response.SetCookie(
            key,
            value,
            maxAge: cookieOptions?.MaxAge?.Seconds ?? 86400,
            path: cookieOptions?.Path, 
            secure: cookieOptions?.Secure ?? true,
            httpOnly: cookieOptions?.HttpOnly ?? true
        );
    }

    public override void RemoveCookie(string key)
    {
        Response.SetCookie(key, string.Empty);
    }

    public override Task StartResponse()
    {
        return Task.CompletedTask;
    }
}