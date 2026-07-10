using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using SimpleW;
using SimpleW.Modules;
using System.Globalization;

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

        public bool MapFallback404 { get; init; }

        public void Install(SimpleWServer server)
        {
            foreach (var liquidRoute in _routesManager.LiquidRoutes)
            {
                server.MapGet(liquidRoute.RouteTemplate, async (HttpSession session) =>
                {
                    await RenderLiquidViewAsync(session, liquidRoute);
                });
                server.MapPost(liquidRoute.RouteTemplate, async (HttpSession session) =>
                {
                    await RenderLiquidViewAsync(session, liquidRoute);
                });
            }

            if (MapFallback404)
            {
                server.Router.MapFallback(async (HttpSession session) =>
                {
                    await RenderLiquidViewAsync(session, null, 404);
                });
            }

            //// wrap existing handler-result (default is JSON sender) 
            //HttpResultHandler next = server.Router.ResultHandler;

            //server.ConfigureResultHandler(async (session, result) =>
            //{
            //    // add Liquid render
            //    if (result is LiquidRoute vr)
            //    {
            //        await RenderLiquidViewAsync(session, vr);
            //        return;
            //    }

            //    await next(session, result);
            //});
        }

        private async ValueTask RenderLiquidViewAsync(HttpSession session, LiquidRoute? liquidRoute = null, int? statusCode = null)
        {
            var request = session.Request;
            var liquidRequest = new LiquidRequestModel
            {
                Route = request.Path,
                QueryParams = new SimpleWQueryDictionary(request.Query),
                Headers = new SimpleWHeaderDictionary(request.Headers),
                Method = request.Method,
                LiquidRoute = liquidRoute,
                ErrorStatusCode = statusCode,
                RouteValues = session.Request.RouteValues != null
                    ? new SimpleWRouteValuesDictionary(session.Request.RouteValues)
                    : EmptyRouteValuesDictionary.Instance
            };

            if (!string.IsNullOrWhiteSpace(request.BodyString))
            {
                liquidRequest.Body = request.BodyString;
            }

            try
            {
                var response = session.Response;

                using var contentWriter = new StringWriter(CultureInfo.InvariantCulture);

                var responseModel = new SimpleWLiquidResponseBuilder(response, contentWriter);

                await _liquidResponseMiddleware.HandleRequestAsync(liquidRequest, responseModel);
                await contentWriter.FlushAsync();
                
                await response.Html(contentWriter.ToString())
                    .SendAsync();
            }
            catch (Exception ex)
            {
                await session.Response
                    .Status(500)
                    .Html($"<h1>Internal Server Error</h1><pre>{System.Net.WebUtility.HtmlEncode(ex.ToString())}</pre>")
                    .SendAsync();
            }
        }
    }
}
