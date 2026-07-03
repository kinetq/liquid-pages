using EmbedIO;
using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.EmbedIO;

public class EmbedIOLiquidResponseBuilder(IHttpResponse response, TextWriter bodyWriter) 
    : LiquidResponseBuilder<IHttpResponse>(response, bodyWriter), ILiquidResponseBuilder
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
}