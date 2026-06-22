using GenHTTP.Api.Protocol;
using Kinetq.LiquidPages.Builders;

namespace Kinetq.LiquidPages.GenHTTP;

public class GenHTTPResponseBuilder : LiquidResponseBuilder<IResponseBuilder>
{
    public override void Initialize(IResponseBuilder response, TextWriter bodyWriter)
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
        Response
            .Type(FlexibleContentType.Parse(contentType));
    }
}