using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using SimpleW;
using SimpleW.Modules;
using System.Collections.Specialized;
using System.Text;

namespace Kinetq.LiquidPages.SimpleW
{
    public class LiquidPagesModule : IHttpModule
    {
        private readonly ILiquidRoutesManager _routesManager;
        private readonly ILiquidResponseMiddleware _liquidResponseMiddleware;

        public LiquidPagesModule(ILiquidRoutesManager routesManager, ILiquidResponseMiddleware liquidResponseMiddleware)
        {
            _routesManager = routesManager;
            _liquidResponseMiddleware = liquidResponseMiddleware;
        }

        public bool MapFallback404 { get; set; } = false;

        public void Install(SimpleWServer server)
        {
            foreach (var liquidRoute in _routesManager.LiquidRoutes)
            {
                server.MapGet(liquidRoute.RouteTemplate, () => liquidRoute);
                server.MapPost(liquidRoute.RouteTemplate, () => liquidRoute);
            }

            if (MapFallback404)
            {
                server.Router.MapFallback(async (HttpSession session) =>
                {
                    await RenderLiquidViewAsync(session, null, 404).ConfigureAwait(false);
                });
            }

            // wrap existing handler-result (default is JSON sender) 
            HttpResultHandler next = server.Router.ResultHandler;

            server.ConfigureResultHandler(async (session, result) =>
            {
                // add Razor render
                if (result is LiquidRoute vr)
                {
                    await RenderLiquidViewAsync(session, vr).ConfigureAwait(false);
                    return;
                }

                await next(session, result).ConfigureAwait(false);
            });
        }

        private async ValueTask RenderLiquidViewAsync(HttpSession session, LiquidRoute? liquidRoute = null, int? statusCode = null)
        {
            var request = session.Request;
            var liquidRequest = new LiquidRequestModel
            {
                Route = request.Path,
                QueryParams = (request.QueryString).GetQueryParams(),
                Headers = new SimpleWHeaderDictionary(request.Headers),
                Method = request.Method,
                LiquidRoute = liquidRoute,
                ErrorStatusCode = statusCode,
                RouteValues = session.Request.RouteValues?
                                  .ToDictionary(pair => pair.Key, pair => (object?)pair.Value) ??
                              new Dictionary<string, object?>()
            };

            if (!string.IsNullOrWhiteSpace(request.BodyString))
            {
                liquidRequest.Body = request.BodyString;
            }

            try
            {
                var response = session.Response;
                var responseContentType = "text/html";

                await using var contentStream = new MemoryStream();
                await using var streamWriter = new StreamWriter(contentStream, Encoding.UTF8, leaveOpen: true);

                var responseModel = new LiquidResponseModel
                {
                    BodyWriter = streamWriter,
                    SetContentType = contentType =>
                    {
                        responseContentType = contentType;
                        response.ContentType(contentType);
                    },
                    SetStatusCode = sc =>
                    {
                        response.Status(sc, null);
                    },
                    StartResponse = _ => { }
                };

                await _liquidResponseMiddleware.HandleRequestAsync(liquidRequest, responseModel);
                await streamWriter.FlushAsync();

                await response
                    .Body(contentStream.ToArray(), responseContentType)
                    .SendAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await session.Response
                    .Status(500)
                    .Html($"<h1>Internal Server Error</h1><pre>{System.Net.WebUtility.HtmlEncode(ex.ToString())}</pre>")
                    .SendAsync().ConfigureAwait(false);
            }
        }
    }
}
