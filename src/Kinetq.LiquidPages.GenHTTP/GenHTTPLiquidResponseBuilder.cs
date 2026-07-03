using GenHTTP.Api.Protocol;
using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.GenHTTP;

public class GenHTTPLiquidResponseBuilder(GenHTTPLiquidResponse response, StreamWriter bodyWriter)
    : LiquidResponseBuilder<GenHTTPLiquidResponse>(response, bodyWriter)
{
    public override void SetStatusCode(int statusCode, string? message = null)
    {
        Response.StatusCode = statusCode;
        Response.StatusDescription = message;
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
        ulong? ticks = (ulong?)cookieOptions?.MaxAge?.Ticks;
        Response.Cookies.Add(ticks.HasValue
            ? new Cookie(key, value, ticks.Value)
            : new Cookie(key, value));
    }

    public override void RemoveCookie(string key)
    {
        Cookie? cookie = Response.Cookies.FirstOrDefault(x => x.Name.Equals(key));
        if (cookie != null)
        {
            Response.Cookies.Remove(cookie.Value);
        }
    }
}