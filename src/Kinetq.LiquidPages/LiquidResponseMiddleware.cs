using System.Net;
using System.Text;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetq.LiquidPages;

public class LiquidResponseMiddleware : ILiquidResponseMiddleware
{
    private readonly ILiquidRoutesManager _liquidRoutesManager;
    private readonly IHtmlRenderer _htmlRenderer;
    private readonly ILogger<LiquidResponseMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public LiquidResponseMiddleware(
        ILiquidRoutesManager liquidRoutesManager,
        IHtmlRenderer htmlRenderer,
        ILogger<LiquidResponseMiddleware> logger, 
        IServiceScopeFactory serviceScopeFactory)
    {
        _liquidRoutesManager = liquidRoutesManager;
        _htmlRenderer = htmlRenderer;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<LiquidResponseModel> HandleRequestAsync(LiquidRequestModel request)
    {
        var renderModel = new RenderModel
        {
            Route = request.Route,
            QueryParams = request.QueryParams
        };

        if (request.ErrorStatusCode.HasValue)
        {
            var requestedStatusCode = (HttpStatusCode)request.ErrorStatusCode.Value;
            return await GetErrorRouteResponse(requestedStatusCode, renderModel, request);
        }

        RouteValueDictionary routeValues = request.RouteValues;
        LiquidRoute? liquidRoute = request.LiquidRoute;
        if (liquidRoute == null)
        {
            liquidRoute = _liquidRoutesManager.GetRouteForPath(request.Route, out routeValues);
        }

        request.RouteValues = routeValues;
        HttpStatusCode? statusCode = await ProcessRoute(liquidRoute, renderModel, request);
        if (statusCode != null && (int)statusCode.Value >= 400)
        {
            return await GetErrorRouteResponse(statusCode.Value, renderModel, request);
        }

        // Handle static routes
        var htmlResponse = await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
        if (htmlResponse != null)
        {
            return new LiquidResponseModel
            {
                Content = Encoding.UTF8.GetBytes(htmlResponse),
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        return await GetErrorRouteResponse(HttpStatusCode.NotFound, renderModel, request);
    }

    private async Task<LiquidResponseModel> GetErrorRouteResponse(
        HttpStatusCode httpStatusCode,
        RenderModel renderModel,
        LiquidRequestModel request,
        int callStack = 0
        )
    {
        var statusCodeRoute = _liquidRoutesManager.GetRouteForStatusCode(httpStatusCode);
        HttpStatusCode? processStatusCodeRoute = await ProcessRoute(statusCodeRoute, renderModel, request);
        if (processStatusCodeRoute != null && callStack < 3)
        {
            return await GetErrorRouteResponse(processStatusCodeRoute.Value, renderModel, request, callStack + 1);
        }

        if (processStatusCodeRoute != null && callStack >= 3)
        {
            return new LiquidResponseModel
            {
                Content = Encoding.UTF8.GetBytes($"<h1>500 - Internal Server Error</h1>"),
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }

        var statusCodeHtmlResponse = await _htmlRenderer.RenderHtml(renderModel, statusCodeRoute);
        if (!string.IsNullOrEmpty(statusCodeHtmlResponse))
        {
            return new LiquidResponseModel
            {
                Content = Encoding.UTF8.GetBytes(statusCodeHtmlResponse),
                ContentType = "text/html",
                StatusCode = (int)httpStatusCode
            };
        }

        return new LiquidResponseModel
        {
            Content = Encoding.UTF8.GetBytes($"<h1>500 - Internal Server Error</h1>"),
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
    }

    private async Task<HttpStatusCode?> ProcessRoute(
        LiquidRoute? liquidRoute,
        RenderModel renderModel,
        LiquidRequestModel request)
    {
        if (liquidRoute?.Execute == null)
        {
            return null;
        }

        try
        {
            if (liquidRoute.PageModelType != null)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                request.LiquidPageModel = (LiquidPageModel?)scope.ServiceProvider.GetService(liquidRoute.PageModelType);
            }
            
            renderModel.ViewModel = await liquidRoute.Execute(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error executing route logic for path {Path}", request.Route);
            return ex.StatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing route logic for path {Path}", request.Route);
            return HttpStatusCode.InternalServerError;
        }

        return null;
    }
}