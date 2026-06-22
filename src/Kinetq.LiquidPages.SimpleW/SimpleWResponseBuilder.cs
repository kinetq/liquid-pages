using Kinetq.LiquidPages.Builders;
using SimpleW;

namespace Kinetq.LiquidPages.SimpleW;

public class SimpleWResponseBuilder : LiquidResponseBuilder<HttpResponse>
{
    public override void Initialize(HttpResponse response, TextWriter bodyWriter)
    {
        Response = response;
        BodyWriter = bodyWriter;
    }

    public override void SetStatusCode(int statusCode, string? message = null)
    {
        Response.Status(statusCode, message);
    }

    public override void SetContentType(string contentType)
    {
        Response.ContentType(contentType);
    }
}