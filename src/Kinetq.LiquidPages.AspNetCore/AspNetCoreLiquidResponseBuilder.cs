using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Http;

namespace Kinetq.LiquidPages.AspNetCore;

public class AspNetCoreLiquidResponseBuilder(HttpResponse response, TextWriter bodyWriter) 
    : LiquidResponseBuilder<HttpResponse>(response, bodyWriter)
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
        Response.Headers.Append(key, value);
    }

    public override void RemoveHeader(string key)
    {
        Response.Headers.Remove(key);
    }

    public override void AddCookie(string key, string value, LiquidCookieOptions? cookieOptions = null)
    {
        var options = cookieOptions ?? new LiquidCookieOptions();

        Response.Cookies.Append(key, value, new CookieOptions
        {
            Domain = options.Domain,
            Path = options.Path,
            Expires = options.Expires,
            Secure = options.Secure,
            SameSite = options.SameSite switch
            {
                LiquidSameSiteMode.Unspecified => SameSiteMode.Unspecified,
                LiquidSameSiteMode.Lax => SameSiteMode.Lax,
                LiquidSameSiteMode.Strict => SameSiteMode.Strict,
                _ => SameSiteMode.None
            },
            HttpOnly = options.HttpOnly,
            MaxAge = options.MaxAge,
            IsEssential = options.IsEssential
        });
    }

    public override void RemoveCookie(string key)
    {
        Response.Cookies.Delete(key);
    }

    public override async Task StartResponse()
    {
        await Response.StartAsync();
    }
}