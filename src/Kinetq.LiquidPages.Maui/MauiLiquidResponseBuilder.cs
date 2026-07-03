using Kinetq.LiquidPages.Builders;

namespace Kinetq.LiquidPages.Maui;

public class MauiLiquidResponseBuilder(MauiLiquidResponse response, TextWriter bodyWriter) : LiquidResponseBuilder<MauiLiquidResponse>(response, bodyWriter)
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
        throw new NotImplementedException();
    }
}