using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Maui;

public class MauiLiquidResponseBuilder(MauiLiquidResponse response, TextWriter bodyWriter) 
    : LiquidResponseBuilder<MauiLiquidResponse>(response, bodyWriter)
{
    public override void SetStatusCode(int statusCode, string? message = null)
    {
        Response.StatusCode = statusCode;
        Response.StatusCodeDescription = message ?? string.Empty;
    }

    public override void SetContentType(string contentType)
    {
        Response.ContentType = contentType;
    }

    public override void AddHeader(string key, string value)
    {
        Response.Headers[key] = value;
    }

    public override void RemoveHeader(string key)
    {
        Response.Headers.Remove(key);
    }

    public override void AddCookie(string key, string value, LiquidCookieOptions? cookieOptions = null)
    {
        Response.Cookies[key] = value;
    }

    public override void RemoveCookie(string key)
    {
        Response.Cookies.Remove(key);
    }

    public override Task StartResponse()
    {
        return Task.CompletedTask;
    }
}