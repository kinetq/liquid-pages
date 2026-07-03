using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Kinetq.LiquidPages.AspNetCore;

public class AspNetCoreLiquidResponseBuilder(HttpResponse response, TextWriter bodyWriter) 
    : LiquidResponseBuilder<HttpResponse>(response, bodyWriter), ILiquidResponseBuilder
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
}