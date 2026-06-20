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

    public async Task HandleRequestAsync(LiquidRequestModel request, LiquidResponseModel response)
    {
        var renderModel = new RenderModel();

        if (request.ErrorStatusCode.HasValue)
        {
            var requestedStatusCode = (HttpStatusCode)request.ErrorStatusCode.Value;
            await GetErrorRouteResponse(requestedStatusCode, renderModel, request, response);
            return;
        }

        LiquidRoute? liquidRoute = request.LiquidRoute;
        if (liquidRoute == null)
        {
            await GetErrorRouteResponse(HttpStatusCode.NotFound, renderModel, request, response);
            return;
        }

        HttpStatusCode? statusCode = await ProcessRoute(liquidRoute, renderModel, request);
        if (statusCode != null && (int)statusCode.Value >= 400)
        {
            await GetErrorRouteResponse(statusCode.Value, renderModel, request, response);
            return;
        }

        response.SetStatusCode(200);
        response.SetContentType("text/html");
        response.StartResponse?.Invoke(CancellationToken.None);

        await _htmlRenderer.RenderHtml(renderModel, liquidRoute, response.BodyWriter);
    }

    private async Task GetErrorRouteResponse(
        HttpStatusCode httpStatusCode,
        RenderModel renderModel,
        LiquidRequestModel request,
        LiquidResponseModel response,
        int callStack = 0
        )
    {
        var statusCodeRoute = _liquidRoutesManager.GetRouteForStatusCode(httpStatusCode);
        HttpStatusCode? processStatusCodeRoute = await ProcessRoute(statusCodeRoute, renderModel, request);
        if (processStatusCodeRoute != null && callStack < 3)
        {
            await GetErrorRouteResponse(processStatusCodeRoute.Value, renderModel, request, response, callStack + 1);
            return;
        }

        if (processStatusCodeRoute != null && callStack >= 3)
        {
            response.SetStatusCode((int)HttpStatusCode.InternalServerError);
            response.SetContentType("text/html");
            response.StartResponse?.Invoke(CancellationToken.None);

            await response.BodyWriter.WriteAsync("<h1>500 - Internal Server Error</h1>");
            return;
        }

        response.SetStatusCode((int)httpStatusCode);
        response.SetContentType("text/html");
        response.StartResponse?.Invoke(CancellationToken.None);
        
        await _htmlRenderer.RenderHtml(renderModel, statusCodeRoute, response.BodyWriter);
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