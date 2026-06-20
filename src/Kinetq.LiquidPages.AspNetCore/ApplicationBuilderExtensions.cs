using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.IO;

namespace Kinetq.LiquidPages.AspNetCore;

public static class ApplicationBuilderExtensions
{
    private static readonly RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();

    public static IApplicationBuilder UseLiquidPagesErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler("/__liquid-error/500");
        app.UseStatusCodePagesWithReExecute("/__liquid-error/{0}");
        return app;
    }

    public static IApplicationBuilder UseLiquidPages(this WebApplication app)
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

        return app;
    }

    private static async Task HandleLiquidRequest(HttpContext context, LiquidRoute liquidRoute)
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
            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, -1, true);
            liquidRequest.Body = await reader.ReadToEndAsync();
        }

        var liquidResponseMiddleware = context.RequestServices.GetRequiredService<ILiquidResponseMiddleware>();
        var response = context.Response;

        using var pooledMemoryStream = manager.GetStream();

        var responseModel = new LiquidResponseModel
        {
            BodyWriter = new StreamWriter(pooledMemoryStream),
            SetContentType = contentType =>
            {
                response.ContentType = contentType;
            },
            SetStatusCode = (statusCode) =>
            {
                response.StatusCode = statusCode;
            }
        };

        await liquidResponseMiddleware.HandleRequestAsync(liquidRequest, responseModel);
        await responseModel.BodyWriter.FlushAsync();
        
        pooledMemoryStream.Position = 0;
        await pooledMemoryStream.CopyToAsync(context.Response.Body);
        // Note: We don't call EndAsync here as the StreamWriter might be reused if there are other middleware chained.
        // The framework will handle the final disposal of the response body stream.
    }
}
