using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using SimpleW;

namespace Kinetq.LiquidPages.SimpleW;

public class SimpleWLiquidResponseBuilder(HttpResponse response, StreamWriter bodyWriter) 
    : LiquidResponseBuilder<HttpResponse>(response, bodyWriter), ILiquidResponseBuilder
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
}