using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidResponseBuilder
{
    void SetStatusCode(int statusCode, string? message = null);
    void SetContentType(string contentType);
    void AddHeader(string key, string value);
    void RemoveHeader(string key);
    void AddCookie(string key, string value, LiquidCookieOptions? cookieOptions = null);
    void RemoveCookie(string key);
    TextWriter BodyWriter { get; }
}