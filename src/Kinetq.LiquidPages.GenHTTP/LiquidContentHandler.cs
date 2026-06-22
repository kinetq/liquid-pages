using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.GenHTTP;

public sealed class LiquidContentHandler : IHandler
{
    private readonly ILiquidResponseMiddleware _middleware;
    private readonly LiquidRoute? _liquidRoute;

    public LiquidContentHandler(ILiquidResponseMiddleware middleware, LiquidRoute? liquidRoute = null)
    {
        _middleware = middleware;
        _liquidRoute = liquidRoute;
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var requestPath = request.Target.Path.ToString();

        var liquidRequest = new LiquidRequestModel
        {
            Route = requestPath,
            QueryParams = request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Headers = new GenHTTPHeaderDictionary(request.Headers),
            Method = request.Method.RawMethod,
            LiquidRoute = _liquidRoute,
            RouteValues = new GenHTTPRouteValuesDictionary(ExtractRouteValues(_liquidRoute?.RouteTemplate, requestPath))
        };
        
        if (request.Content != null)
        {
            using var reader = new StreamReader(request.Content, Encoding.UTF8, leaveOpen: true);
            liquidRequest.Body = await reader.ReadToEndAsync();
        }

        try
        {
            var responseStatusCode = 200;
            var responseContentType = "text/html";

            await using var contentStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(contentStream, Encoding.UTF8, leaveOpen: true);

            var responseModel = new GenHTTPResponseBuilder();

            var responseBuilder = request.Respond();
            responseModel.Initialize(responseBuilder, streamWriter);
            
            await _middleware.HandleRequestAsync(liquidRequest, responseModel);
            await streamWriter.FlushAsync();

            var contentBytes = contentStream.ToArray();

            return responseBuilder
                .Content(new ByteArrayContent(contentBytes))
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

    private static IReadOnlyDictionary<string, object?> ExtractRouteValues(string? routeTemplate, string requestPath)
    {
        var routeValues = new Dictionary<string, object?>();

        if (string.IsNullOrWhiteSpace(routeTemplate))
        {
            return routeValues;
        }

        var templateSegments = routeTemplate.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var pathSegments = requestPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        var maxSegments = Math.Min(templateSegments.Length, pathSegments.Length);
        for (var i = 0; i < maxSegments; i++)
        {
            if (!TryGetParameterName(templateSegments[i], out var parameterName, out var isCatchAll))
            {
                continue;
            }

            if (isCatchAll)
            {
                routeValues[parameterName] = string.Join('/', pathSegments.Skip(i).Select(Uri.UnescapeDataString));
                break;
            }

            routeValues[parameterName] = Uri.UnescapeDataString(pathSegments[i]);
        }

        return routeValues;
    }

    private static bool TryGetParameterName(string templateSegment, out string parameterName, out bool isCatchAll)
    {
        parameterName = string.Empty;
        isCatchAll = false;

        if (templateSegment.Length < 3 || templateSegment[0] != '{' || templateSegment[^1] != '}')
        {
            return false;
        }

        var segmentContent = templateSegment[1..^1];
        if (string.IsNullOrWhiteSpace(segmentContent))
        {
            return false;
        }

        if (segmentContent.StartsWith("**", StringComparison.Ordinal))
        {
            isCatchAll = true;
            segmentContent = segmentContent[2..];
        }
        else if (segmentContent.StartsWith("*", StringComparison.Ordinal))
        {
            isCatchAll = true;
            segmentContent = segmentContent[1..];
        }

        var constraintSeparatorIndex = segmentContent.IndexOf(':');
        if (constraintSeparatorIndex >= 0)
        {
            segmentContent = segmentContent[..constraintSeparatorIndex];
        }

        if (segmentContent.EndsWith("?", StringComparison.Ordinal))
        {
            segmentContent = segmentContent[..^1];
        }

        if (string.IsNullOrWhiteSpace(segmentContent))
        {
            return false;
        }

        parameterName = segmentContent;
        return true;
    }
}
