using System.Net;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
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

    public async Task<string?> HandleRequestAsync(LiquidRequestModel request, ILiquidResponseBuilder responseBuilder)
    {
        var renderModel = new RenderModel();

        if (request.ErrorStatusCode.HasValue)
        {
            var requestedStatusCode = (HttpStatusCode)request.ErrorStatusCode.Value;
            return await GetErrorRouteResponse(requestedStatusCode, renderModel, request, responseBuilder);
        }

        LiquidRoute? liquidRoute = request.LiquidRoute;
        if (liquidRoute == null)
        {
            return await GetErrorRouteResponse(HttpStatusCode.NotFound, renderModel, request, responseBuilder);
        }

        HttpStatusCode? statusCode = await ProcessRoute(liquidRoute, renderModel, request, responseBuilder);
        if (statusCode != null && (int)statusCode.Value >= 400)
        {
            return await GetErrorRouteResponse(statusCode.Value, renderModel, request, responseBuilder);
        }

        responseBuilder.SetStatusCode(200);
        responseBuilder.SetContentType("text/html");
        await responseBuilder.StartResponse();

        if (responseBuilder.BodyWriter != null)
        {
            await _htmlRenderer.RenderHtml(renderModel, liquidRoute, responseBuilder.BodyWriter);
            return null;
        }

        return await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
    }

    private async Task<string?> GetErrorRouteResponse(
        HttpStatusCode httpStatusCode,
        RenderModel renderModel,
        LiquidRequestModel request,
        ILiquidResponseBuilder response,
        int callStack = 0
        )
    {
        var statusCodeRoute = _liquidRoutesManager.GetRouteForStatusCode(httpStatusCode);
        HttpStatusCode? processStatusCodeRoute = await ProcessRoute(statusCodeRoute, renderModel, request, response);
        if (processStatusCodeRoute != null && callStack < 3)
        {
            return await GetErrorRouteResponse(processStatusCodeRoute.Value, renderModel, request, response, callStack + 1);
        }

        if (processStatusCodeRoute != null && callStack >= 3)
        {
            response.SetStatusCode((int)HttpStatusCode.InternalServerError);
            response.SetContentType("text/html");

            if (response.BodyWriter != null)
            {
                await response.BodyWriter.WriteAsync("<h1>500 - Internal Server Error</h1>");
                return null;
            }
            
            return "<h1>500 - Internal Server Error</h1>";
        }

        response.SetStatusCode((int)httpStatusCode);
        response.SetContentType("text/html");

        return await ProcessResponse(renderModel, statusCodeRoute, response);
    }

    private async Task<string?> ProcessResponse(RenderModel renderModel, LiquidRoute liquidRoute,
        ILiquidResponseBuilder responseBuilder)
    {
        if (responseBuilder.BodyWriter != null)
        {
            await _htmlRenderer.RenderHtml(renderModel, liquidRoute, responseBuilder.BodyWriter);
            return null;
        }

        return await _htmlRenderer.RenderHtml(renderModel, liquidRoute);
    }

    private async Task<HttpStatusCode?> ProcessRoute(
        LiquidRoute? liquidRoute,
        RenderModel renderModel,
        LiquidRequestModel request,
        ILiquidResponseBuilder responseBuilder)
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
                request.LiquidPageModel!.ResponseBuilder = responseBuilder;
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