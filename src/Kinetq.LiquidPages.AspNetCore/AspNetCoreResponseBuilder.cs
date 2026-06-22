using Kinetq.LiquidPages.Builders;
using Microsoft.AspNetCore.Http;

namespace Kinetq.LiquidPages.AspNetCore;

public class AspNetCoreResponseBuilder : LiquidResponseBuilder<HttpResponse>
{
    public override void Initialize(HttpResponse response, TextWriter bodyWriter)
    {
        Response = response;
        BodyWriter = bodyWriter;
    }

    public override void SetStatusCode(int statusCode, string? message = null)
    {
        Response.StatusCode = statusCode;
    }

    public override void SetContentType(string contentType)
    {
        Response.ContentType = contentType;
    }
}