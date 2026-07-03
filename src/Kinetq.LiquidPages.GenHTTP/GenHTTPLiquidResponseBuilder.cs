using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Interfaces;

namespace Kinetq.LiquidPages.GenHTTP;

public class GenHTTPLiquidResponseBuilder(GenHTTPLiquidResponse response, StreamWriter bodyWriter) 
    : LiquidResponseBuilder<GenHTTPLiquidResponse>(response, bodyWriter), ILiquidResponseBuilder
{
    public override void SetStatusCode(int statusCode, string? message = null)
    {
        response.StatusCode = statusCode;
        response.StatusDescription = message;
    }

    public override void SetContentType(string contentType)
    {
        response.ContentType = contentType;
    }

    public override void AddHeader(string key, string value)
    {
        response.Headers.Add(key, value);
    }
}