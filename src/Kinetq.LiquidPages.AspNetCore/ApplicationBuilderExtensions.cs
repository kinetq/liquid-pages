using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using System.Text;

namespace Kinetq.LiquidPages.AspNetCore;

public static class ApplicationBuilderExtensions
{

    internal const string EndpointRouteBuilderKey = "__EndpointRouteBuilder";

    public static PageActionEndpointConventionBuilder MapLiquidPages(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var builder = GetOrCreateDataSource(endpoints).DefaultBuilder;
        if (!builder.Items.ContainsKey(EndpointRouteBuilderKey))
        {
            builder.Items[EndpointRouteBuilderKey] = endpoints;
        }
        return builder;
    }

    private static PageActionEndpointDataSource GetOrCreateDataSource(IEndpointRouteBuilder endpoints)
    {
        var dataSource = endpoints.DataSources.OfType<PageActionEndpointDataSource>().FirstOrDefault();
        if (dataSource == null)
        {
            var orderProviderCache = endpoints.ServiceProvider.GetRequiredService<OrderedEndpointsSequenceProviderCache>();
            var factory = endpoints.ServiceProvider.GetRequiredService<PageActionEndpointDataSourceFactory>();
            dataSource = factory.Create(orderProviderCache.GetOrCreateOrderedEndpointsSequenceProvider(endpoints));
            endpoints.DataSources.Add(dataSource);
        }

        return dataSource;
    }

    //public static PageActionEndpointConventionBuilder MapLiquidPages(this IEndpointRouteBuilder endpoints)
    //{
    //    var routesManager = endpoints.ServiceProvider.GetRequiredService<ILiquidRoutesManager>();
    //    var methods = new[] { "GET", "POST" };

    //    async Task HandleLiquidRequest(HttpContext context, LiquidRoute liquidRoute)
    //    {
    //        var request = context.Request;
    //        var liquidRequest = new LiquidRequestModel
    //        {
    //            Route = request.Path.Value ?? "/",
    //            QueryParams = (request.QueryString.Value ?? string.Empty).GetQueryParams(),
    //            Headers = new AspNetCoreHeaderDictionary(request.Headers),
    //            Method = request.Method,
    //            LiquidRoute = liquidRoute,
    //            RouteValues = new AspNetCoreRouteValuesDictionary(context.Request.RouteValues)
    //        };

    //        if (request.ContentLength > 0)
    //        {
    //            liquidRequest.Body = await ReadRequestBodyAsync(request);
    //        }

    //        var liquidResponseMiddleware = context.RequestServices.GetRequiredService<ILiquidResponseMiddleware>();
    //        var response = context.Response;
    //        using var responseBodyWriter = new HttpResponseStreamWriter(
    //            response.Body,
    //            Encoding.UTF8,
    //            1024,
    //            ArrayPool<byte>.Shared,
    //            ArrayPool<char>.Shared);

    //        var responseModel = new LiquidResponseBuilder
    //        {
    //            BodyWriter = responseBodyWriter,
    //            SetContentType = contentType =>
    //            {
    //                response.ContentType = contentType;
    //            },
    //            SetStatusCode = (statusCode) =>
    //            {
    //                response.StatusCode = statusCode;
    //            }
    //        };

    //        await liquidResponseMiddleware.HandleRequestAsync(liquidRequest, responseModel);
    //        await responseBodyWriter.FlushAsync();
    //    }

    //    foreach (var route in routesManager.LiquidRoutes)
    //    {
    //        if (string.IsNullOrWhiteSpace(route.RouteTemplate))
    //        {
    //            continue;
    //        }

    //        endpoints
    //            .MapMethods(route.RouteTemplate, methods, HandleLiquidRequest)
    //            .WithMetadata(route);
    //    }

    //    endpoints.MapFallback(HandleLiquidRequest);
    //    return null!;
    //}

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        var contentLength = request.ContentLength;
        if (!contentLength.HasValue || contentLength.Value <= 0)
        {
            return string.Empty;
        }

        if (contentLength.Value > int.MaxValue)
        {
            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, -1, true);
            return await reader.ReadToEndAsync();
        }

        var byteCount = (int)contentLength.Value;
        var rentedBuffer = ArrayPool<byte>.Shared.Rent(byteCount);

        try
        {
            var totalRead = 0;
            while (totalRead < byteCount)
            {
                var read = await request.Body.ReadAsync(
                    rentedBuffer.AsMemory(totalRead, byteCount - totalRead),
                    request.HttpContext.RequestAborted);

                if (read == 0)
                {
                    break;
                }

                totalRead += read;
            }

            return Encoding.UTF8.GetString(rentedBuffer, 0, totalRead);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedBuffer);
        }
    }
}
