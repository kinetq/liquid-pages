using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Models;
using SimpleW;
using static SimpleW.HttpResponse;

namespace Kinetq.LiquidPages.SimpleW;

public class SimpleWLiquidResponseBuilder(HttpResponse response, StreamWriter bodyWriter)
    : LiquidResponseBuilder<HttpResponse>(response, bodyWriter)
{
    public override void SetStatusCode(int statusCode, string? message = null)
    {
        Response.Status(statusCode, message);
    }

    public override void SetContentType(string contentType)
    {
        Response.ContentType(contentType);
    }

    public override void AddHeader(string key, string value)
    {
        Response.AddHeader(key, value);
    }

    public override void RemoveHeader(string key)
    {
        Response.AddHeader(key, string.Empty);
    }

    public override void AddCookie(string key, string value, LiquidCookieOptions? cookieOptions = null)
    {
        if (cookieOptions != null)
        {
            var sameSite = cookieOptions.SameSite switch
            {
                LiquidSameSiteMode.Unspecified => SameSiteMode.Unspecified,
                LiquidSameSiteMode.Lax => SameSiteMode.Lax,
                LiquidSameSiteMode.Strict => SameSiteMode.Strict,
                _ => SameSiteMode.None
            };

            var options =
                new HttpResponse.CookieOptions(
                  path: cookieOptions.Path,
                  domain: cookieOptions.Domain,
                  maxAgeSeconds: cookieOptions.MaxAge?.Seconds,
                  expires: cookieOptions.Expires,
                  secure: cookieOptions.Secure,
                  httpOnly: cookieOptions.HttpOnly,
                  sameSite: sameSite
                  );

            Response.SetCookie(key, value, options);
        }
        else
        {
            Response.SetCookie(key, value);
        }

    }

    public override void RemoveCookie(string key)
    {
        Response.DeleteCookie(key);
    }
}