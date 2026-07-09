using System.Buffers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Kinetq.LiquidPages.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseLiquidPages(this WebApplication app, bool map404Fallback = false)
    {
        var routesManager = app.Services.GetRequiredService<ILiquidRoutesManager>();
        foreach (var route in routesManager.LiquidRoutes)
        {
            if (string.IsNullOrWhiteSpace(route.RouteTemplate))
            {
                continue;
            }

            app.MapGet(route.RouteTemplate, async (httpContext) =>
            {
                await HandleLiquidRequest(httpContext, route);
            });

            app.MapPost(route.RouteTemplate, async (httpContext) =>
            {
                await HandleLiquidRequest(httpContext, route);
            });
        }

        if (map404Fallback)
        {
            app.MapFallback(async (context) =>
            {
                await HandleLiquidRequest(context, null);
            });
        }

        return app;
    }

    private static async Task HandleLiquidRequest(HttpContext context, LiquidRoute? liquidRoute)
    {
        var request = context.Request;
        var liquidRequest = new LiquidRequestModel
        {
            Route = request.Path.Value ?? "/",
            QueryParams = new AspNetCoreQueryParams(request.Query),
            Headers = new AspNetCoreHeaderDictionary(request.Headers),
            Method = request.Method,
            LiquidRoute = liquidRoute,
            RouteValues = new AspNetCoreRouteValuesDictionary(context.Request.RouteValues)
        };

        if (request.ContentLength > 0)
        {
            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, -1, true);
            liquidRequest.Body = await reader.ReadToEndAsync();
        }

        var liquidResponseMiddleware = context.RequestServices.GetRequiredService<ILiquidResponseMiddleware>();
        var response = context.Response;

        var responseBodyWriter = new HttpResponseStreamWriter(
            response.Body,
            Encoding.UTF8,
            1024,
            ArrayPool<byte>.Shared,
            ArrayPool<char>.Shared);

        var responseModel = new AspNetCoreLiquidResponseBuilder(response, responseBodyWriter);

        await liquidResponseMiddleware.HandleRequestAsync(liquidRequest, responseModel);
        await responseModel.BodyWriter.FlushAsync();
    }
}
