using System.Net;
using EmbedIO;
using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.EmbedIO;

public class EmbedIOLiquidResponseBuilder(IHttpResponse response, TextWriter bodyWriter) 
    : LiquidResponseBuilder<IHttpResponse>(response, bodyWriter)
{
    public override void SetStatusCode(int statusCode, string? message = null)
    {
        Response.StatusCode = statusCode;
    }

    public override void SetContentType(string contentType)
    {
        Response.ContentType = contentType;
    }

    public override void AddHeader(string key, string value)
    {
        Response.Headers.Add(key, value);
    }

    public override void RemoveHeader(string key) => Response.Headers.Remove(key);

    public override void AddCookie(string key, string value, LiquidCookieOptions? cookieOptions = null)
    {
        var options = cookieOptions ?? new LiquidCookieOptions();
        Response.Cookies.Add(new Cookie
        {
            Name = key,
            Value = value,
            Domain = options.Domain ?? string.Empty,
            Path = options.Path ?? string.Empty,
            Expires = options.Expires?.UtcDateTime ?? default,
            Secure = options.Secure,
            HttpOnly = options.HttpOnly
        });
    }

    public override void RemoveCookie(string key)
    {
        Response.Cookies.Add(new Cookie
        {
            Name = key,
            Value = string.Empty,
            Expired = true,
            Expires = DateTime.UtcNow.AddDays(-1)
        });
    }
}