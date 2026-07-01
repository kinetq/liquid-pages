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

    public static IApplicationBuilder UseLiquidPages(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app is not IEndpointRouteBuilder endpoints)
        {
            throw new InvalidOperationException("LiquidPages endpoint mapping requires an endpoint route builder.");
        }

        endpoints.MapLiquidPages();

        return app;
    }

    public static IApplicationBuilder UseLiquidPagesErrorHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseExceptionHandler(exceptionApp =>
        {
            exceptionApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("LiquidPages request failed.");
            });
        });
    }

    private static LiquidPagesEndpointDataSource GetOrCreateDataSource(IEndpointRouteBuilder endpoints)
    {
        var dataSource = endpoints.DataSources.OfType<LiquidPagesEndpointDataSource>().FirstOrDefault();
        if (dataSource == null)
        {
            var routesManager = endpoints.ServiceProvider.GetRequiredService<ILiquidRoutesManager>();
            dataSource = new LiquidPagesEndpointDataSource(routesManager, HandleLiquidRequestAsync);
            endpoints.DataSources.Add(dataSource);
        }

        return dataSource;
    }

    private static async Task HandleLiquidRequestAsync(HttpContext context, LiquidRoute liquidRoute)
    {
        var request = context.Request;
        var liquidRequest = new LiquidRequestModel
        {
            Route = request.Path.Value ?? "/",
            QueryParams = (request.QueryString.Value ?? string.Empty).GetQueryParams(),
            Headers = new AspNetCoreHeaderDictionary(request.Headers),
            Method = request.Method,
            LiquidRoute = liquidRoute,
            RouteValues = new AspNetCoreRouteValuesDictionary(context.Request.RouteValues)
        };

        if (request.ContentLength > 0)
        {
            liquidRequest.Body = await ReadRequestBodyAsync(request);
        }

        var liquidResponseMiddleware = context.RequestServices.GetRequiredService<ILiquidResponseMiddleware>();
        var response = context.Response;
        using var responseBodyWriter = new HttpResponseStreamWriter(
            response.Body,
            Encoding.UTF8,
            1024,
            ArrayPool<byte>.Shared,
            ArrayPool<char>.Shared);

        var responseModel = new LiquidResponseBuilder
        {
            BodyWriter = responseBodyWriter,
            SetContentType = contentType =>
            {
                response.ContentType = contentType;
            },
            SetStatusCode = statusCode =>
            {
                response.StatusCode = statusCode;
            }
        };

        await liquidResponseMiddleware.HandleRequestAsync(liquidRequest, responseModel);
        await responseBodyWriter.FlushAsync();
    }

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
