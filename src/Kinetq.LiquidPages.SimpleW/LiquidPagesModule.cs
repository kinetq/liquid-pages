using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using SimpleW;
using SimpleW.Modules;
using System.Buffers;
using System.Collections.Specialized;
using System.Text;
using HttpRequestException = SimpleW.HttpRequestException;

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

        public void Install(SimpleWServer server)
        {
            foreach (var liquidRoute in _routesManager.LiquidRoutes)
            {
                server.MapGet(liquidRoute.RouteTemplate, () => liquidRoute);
                server.MapPost(liquidRoute.RouteTemplate, () => liquidRoute);
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

                try
                {

                    await next(session, result).ConfigureAwait(false);
                }
                catch (HttpRequestException ex)
                {
                    await RenderLiquidViewAsync(session, null, ex.StatusCode).ConfigureAwait(false);
                }
            });
        }

        private async ValueTask RenderLiquidViewAsync(HttpSession session, LiquidRoute? liquidRoute = null, int? statusCode = null)
        {
            var request = session.Request;
            var headers = new NameValueCollection();
            foreach (var header in request.Headers.EnumerateAll())
            {
                headers.Add(header.Key, header.Value);
            }

            var liquidRequest = new LiquidRequestModel
            {
                Route = request.Path,
                QueryParams = (request.QueryString).GetQueryParams(),
                Headers = headers,
                Method = request.Method,
                LiquidRoute = liquidRoute,
                ErrorStatusCode = statusCode,
                RouteValues = session.Request.RouteValues?
                                  .ToDictionary(pair => pair.Key, pair => (object?)pair.Value) ??
                              new Dictionary<string, object?>()
            };

            if (!request.Body.IsEmpty)
            {
                var stream = new MemoryStream(request.Body.ToArray());
                using var reader = new StreamReader(stream, Encoding.UTF8, true, -1, true);
                liquidRequest.Body = await reader.ReadToEndAsync();
            }

            try
            {
                var responseModel = await _liquidResponseMiddleware.HandleRequestAsync(liquidRequest);
                await session.Response
                    .Status(responseModel.StatusCode)
                    .Body(responseModel.Content, responseModel.ContentType)
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
