using EmbedIO;
using Kinetq.LiquidPages.Builders;

namespace Kinetq.LiquidPages.EmbedIO;

public class EmbedIOResponseBuilder : LiquidResponseBuilder<IHttpResponse>
{
    public override void Initialize(IHttpResponse response, TextWriter bodyWriter)
    {
        Response = response;
        BodyWriter = bodyWriter;
    }

    public override void SetStatusCode(int statusCode, string? message = null)
    {
        Response.StatusCode = statusCode;
        if (!string.IsNullOrEmpty(message))
        {
            Response.StatusDescription = message;
        }
    }

    public override void SetContentType(string contentType)
    {
        Response.ContentType = contentType;
    }
}