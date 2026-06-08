using System.Net;
using System.Text;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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

        LiquidRoute? liquidRoute = request.LiquidRoute ?? _liquidRoutesManager.GetRouteForPath(request.Route);
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

        string extension = Path.GetExtension(request.Route);
        string assetContentType = extension.GetContentType();
        if (!string.IsNullOrEmpty(assetContentType))
        {
            try
            {
                string referer = request.Headers["Referer"];
                IFileProvider? assetFileProvider = null;
                if (!string.IsNullOrEmpty(referer))
                {
                    Uri refererUri = new Uri(referer);
                    LiquidRoute? referrerLiquidRoute = _liquidRoutesManager.GetRouteForPath(refererUri.AbsolutePath);
                    assetFileProvider = referrerLiquidRoute?.FileProvider;
                }

                assetFileProvider ??= _liquidRoutesManager.GetFileProviderForAsset(request.Route);

                var fileInfo = assetFileProvider?.GetFileInfo(request.Route);
                if (fileInfo is { Exists: true })
                {
                    var fileContent = await fileInfo.GetFileContentsBytes();
                    var contentType = await fileInfo.GetFileContentType();
                    return new LiquidResponseModel
                    {
                        Content = fileContent,
                        ContentType = contentType,
                        StatusCode = 200
                    };
                }
            }
            catch
            {
                // Fall through to 404
            }
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