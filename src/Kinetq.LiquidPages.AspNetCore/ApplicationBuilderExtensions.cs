using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;
using System.Collections.Specialized;
using System.Text;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetq.LiquidPages.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseLiquidPagesErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler("/__liquid-error/500");
        app.UseStatusCodePagesWithReExecute("/__liquid-error/{0}");
        return app;
    }

    public static PageActionEndpointConventionBuilder MapLiquidPages(this IEndpointRouteBuilder endpoints)
    {
        var routesManager = endpoints.ServiceProvider.GetRequiredService<ILiquidRoutesManager>();
        var methods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS", "HEAD" };

        async Task HandleLiquidRequest(HttpContext context)
        {
            var request = context.Request;
            var headers = new NameValueCollection();
            foreach (var header in request.Headers)
            {
                headers.Add(header.Key, header.Value.ToString());
            }

            var liquidRequest = new LiquidRequestModel
            {
                Route = request.Path.Value ?? "/",
                QueryParams = (request.QueryString.Value ?? string.Empty).GetQueryParams(),
                Headers = headers,
                Method = request.Method,
                LiquidRoute = context.GetEndpoint()?.Metadata.GetMetadata<LiquidRoute>()
            };

            liquidRequest.RouteValues = context.Request.RouteValues.ToDictionary();

            const string errorPrefix = "/__liquid-error/";
            if (liquidRequest.Route.StartsWith(errorPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var statusSegment = liquidRequest.Route.Substring(errorPrefix.Length);
                if (int.TryParse(statusSegment, out var statusCode))
                {
                    liquidRequest.ErrorStatusCode = statusCode;
                }

                var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionFeature != null && !string.IsNullOrWhiteSpace(exceptionFeature.Path))
                {
                    liquidRequest.Route = exceptionFeature.Path;
                }
            }

            if (request.ContentLength > 0)
            {
                using var reader = new StreamReader(request.Body, Encoding.UTF8, true, -1, true);
                liquidRequest.Body = await reader.ReadToEndAsync();
            }

            var liquidResponseMiddleware = context.RequestServices.GetRequiredService<ILiquidResponseMiddleware>();
            var responseModel = await liquidResponseMiddleware.HandleRequestAsync(liquidRequest);

            var response = context.Response;
            response.ContentLength = responseModel.Content.Length;
            response.ContentType = responseModel.ContentType;
            response.StatusCode = responseModel.StatusCode;
            await response.Body.WriteAsync(responseModel.Content);
        }

        foreach (var route in routesManager.LiquidRoutes)
        {
            if (string.IsNullOrWhiteSpace(route.RouteTemplate))
            {
                continue;
            }

            endpoints
                .MapMethods(route.RouteTemplate, methods, HandleLiquidRequest)
                .WithMetadata(route);
        }

        endpoints.MapFallback(HandleLiquidRequest);
        endpoints.MapMethods("/__liquid-error/{statusCode:int}", methods, HandleLiquidRequest);
        return null!;
    }
}
