using System.Collections.Specialized;
using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.GenHTTP;

public sealed class LiquidContentHandler : IHandler
{
    private readonly ILiquidResponseMiddleware _middleware;

    public LiquidContentHandler(ILiquidResponseMiddleware middleware)
    {
        _middleware = middleware;
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var headers = new NameValueCollection();
        foreach (var (key, value) in request.Headers)
        {
            headers[key] = value;
        }

        var liquidRequest = new LiquidRequestModel
        {
            Route = request.Target.Path.ToString(),
            QueryParams = request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Headers = headers,
            Method = request.Method.RawMethod
        };

        if (request.Content != null)
        {
            using var reader = new StreamReader(request.Content, Encoding.UTF8, leaveOpen: true);
            liquidRequest.Body = await reader.ReadToEndAsync();
        }

        try
        {
            var liquidResponse = await _middleware.HandleRequestAsync(liquidRequest);

            return request.Respond()
                .Status((ResponseStatus)liquidResponse.StatusCode)
                .Type(FlexibleContentType.Parse(liquidResponse.ContentType))
                .Content(new ByteArrayContent(liquidResponse.Content))
                .Build();
        }
        catch
        {
            var errorBody = Encoding.UTF8.GetBytes("<h1>Internal Server Error</h1>");
            return request.Respond()
                .Status(500, "Internal Server Error")
                .Type(FlexibleContentType.Parse("text/html"))
                .Content(new ByteArrayContent(errorBody))
                .Build();
        }
    }
}
